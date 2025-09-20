// -----------------------------------------------------------------------
// <copyright file="OperatorHandlerBase.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  internal abstract class OperatorHandlerBase(Func<Operator, bool> canHandle) : IOperatorHandler
  {
    protected static readonly ConcurrentDictionary<(object Value, Type TargetType), object?> TypeConversions = new();
    private static readonly ConcurrentDictionary<(Type type, string memberName), bool> _memberExistsCache = new();
    private readonly Func<Operator, bool> _canHandle = canHandle ?? throw new ArgumentNullException(nameof(canHandle));
    protected static readonly Expression TrueExpression = Expression.Constant(true);

    public abstract Expression BuildExpression(ParameterExpression parameter, FilterInfo filter);

    public bool CanHandle(Operator @operator) => _canHandle(@operator);

    /// <summary>
    /// Builds a property access expression that safely navigates a dotted path (e.g., "A.B.C").
    /// For each segment, adds a null check to prevent NullReferenceException at runtime.
    /// The resulting expression is equivalent to:
    ///   o => o != null ? (o.A != null ? (o.A.B != null ? o.A.B.C : default) : default) : default
    /// </summary>
    protected static Expression? BuildPropertyAccessExpression(ParameterExpression parameter, string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        return null;
      }

      string[] segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      if (segments.Length == 0)
      {
        return null; // No valid segments to process
      }

      Expression parameterExpression = parameter;
      Type type = parameterExpression.Type;

      foreach (string segment in segments)
      {
        if (!MemberExists(type, segment))
        {
          // Property or field does not exist
          return null;
        }

        // If the member exists, build the property or field access expression
        MemberExpression member = Expression.PropertyOrField(parameterExpression, segment);

        // If the current expression is a reference type, add a null check
        parameterExpression = !parameterExpression.Type.IsValueType || IsNullableType(parameterExpression.Type)
          ? AddNullCheck(parameterExpression, member)
          : member; // If it's a value type, just use the member directly

        type = parameterExpression.Type;
      }

      return parameterExpression;
    }

    protected static BinaryExpression BuildStringExpression(Expression property, MethodInfo method, object? comparisonValue)
    {
      Type propertyType = property.Type;

      if (propertyType != typeof(string))
      {
        // Convert to string first
        MethodInfo toStringMethod = propertyType.GetMethod("ToString", Type.EmptyTypes)!;
        property = Expression.Call(property, toStringMethod);
      }

      return Expression.AndAlso(
        Expression.NotEqual(property, Expression.Constant(null, property.Type)),
        Expression.Call(
          property,
          method,
          Expression.Constant(comparisonValue?.ToString() ?? string.Empty, typeof(string)), Expression.Constant(StringComparison.OrdinalIgnoreCase)));
    }

    protected static ConstantExpression BuildValueAccessExpression(object? comparisonValue, Type type)
    {
      // Handle collections: don't try to convert the comparison value to the collection type
      Type workingType = type.IsAssignableTo(typeof(IEnumerable)) && type != typeof(string)
        ? GetEnumerableElementType(type)
        : type;

      if (comparisonValue is null)
      {
        if (IsNotNullableValueType(workingType))
        {
          return Expression.Constant(null, typeof(Nullable<>).MakeGenericType(workingType));
        }

        return Expression.Constant(null, workingType);
      }

      Func<(object Value, Type TargetType), object?> convertAction =
        (key) => ConvertTypeHelpers.ConvertToTargetType(key.Value, key.TargetType);

      if (comparisonValue.GetType() != workingType)
      {
        // Check if we're dealing with nullable types
        if (IsNullableType(workingType) && comparisonValue.GetType() == Nullable.GetUnderlyingType(workingType))
        {
          convertAction = (key) => key.Value;
        }

        comparisonValue = TypeConversions.GetOrAdd((comparisonValue!, workingType), convertAction);
      }

      return Expression.Constant(comparisonValue, workingType);
    }

    protected static (Expression PropertyValue, ConstantExpression ComparisonValueExpression) BuildValueAccessExpressions(ParameterExpression parameter, string propertyName, object? comparisonValue)
    {
      (Expression property, IEnumerable<ConstantExpression> comparisonValueExpressions) = BuildValueAccessExpressions(parameter, propertyName, [comparisonValue]);
      return (property, comparisonValueExpressions.First());
    }

    protected static (Expression PropertyValue, IEnumerable<ConstantExpression> ComparisonValueExpressions) BuildValueAccessExpressions(ParameterExpression parameter, string propertyName, IEnumerable<object?> comparisonValues)
    {
      Expression? property = BuildPropertyAccessExpression(parameter, propertyName);
      if (property is null)
      {
        return (Expression.Constant(null), [Expression.Constant(null)]); // Property does not exist
      }

      Type propertyType = property.Type;

      List<ConstantExpression> comparisonExpressions = [];
      bool makePropertyNullable = false;
      foreach (object? comparisonValue in comparisonValues)
      {
        makePropertyNullable = !makePropertyNullable && comparisonValue is null && IsNotNullableValueType(propertyType);
        comparisonExpressions.Add(BuildValueAccessExpression(comparisonValue, propertyType));
      }

      if (makePropertyNullable)
      {
        property = Expression.Convert(property, typeof(Nullable<>).MakeGenericType(propertyType));
      }

      return (property, comparisonExpressions);
    }

    protected static bool IsNotNullableValueType(Type type) =>
          type.IsValueType && Nullable.GetUnderlyingType(type) is null;

    /// <summary>
    /// Adds a null check to a member access expression. If the instance is null, returns default(member.Type), else member.
    /// </summary>
    private static ConditionalExpression AddNullCheck(Expression instance, Expression member) =>
      Expression.Condition(
        Expression.Equal(instance, Expression.Constant(null, instance.Type)),
        Expression.Default(member.Type),
        member
      );

    private static Type GetEnumerableElementType(Type type) =>
      type.IsArray
        ? type.GetElementType()!
        : type.GetGenericArguments().FirstOrDefault() ?? typeof(object);

    private static bool IsNullableType(Type type) =>
      Nullable.GetUnderlyingType(type) is not null;

    private static bool MemberExists(Type type, string memberName) =>
      // Check for property or field (case-insensitive)
      _memberExistsCache.GetOrAdd((type, memberName), (key) =>
      {
        const BindingFlags bindingAttrs = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
        return type.GetProperty(key.memberName, bindingAttrs) is not null
          || type.GetField(key.memberName, bindingAttrs) is not null;
      });
  }
}