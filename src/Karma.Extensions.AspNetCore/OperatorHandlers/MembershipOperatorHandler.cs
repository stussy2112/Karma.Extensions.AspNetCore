// -----------------------------------------------------------------------
// <copyright file="MembershipOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles collection membership operators (In, NotIn).
  /// </summary>
  internal sealed class MembershipOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.In or Operator.NotIn)
  {
    private static readonly MethodInfo _enumerableContains = typeof(Enumerable).GetMethods().First(m => nameof(Enumerable.Contains).Equals(m.Name, StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length == 2);

    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      (Expression? property, Expression? _) = BuildValueAccessExpressions(parameter, filter.Path!, filter.Values.FirstOrDefault());
      if (property is null or ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return filter.Operator switch
      {
        Operator.In => BuildInExpression(property, filter.Values),
        Operator.NotIn => Expression.Not(BuildInExpression(property, filter.Values)),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(MembershipOperatorHandler)}")
      };
    }

    private static MethodCallExpression BuildInExpression(Expression property, IEnumerable<object> values)
    {
      // In -> the property value is in the value of the filterInfo
      Type elementType = property.Type;
      var typedArray = Array.CreateInstance(elementType, values.Count());
      int i = 0;

      foreach (object v in values)
      {
        // Handle nulls and type conversion
        object? converted = TypeConversions.GetOrAdd((v, elementType), (key) => key.Value == null ? null : ConvertTypeHelpers.ConvertToTargetType(key.Value, key.TargetType));
        typedArray.SetValue(converted, i++);
      }

      ConstantExpression valuesExpr = Expression.Constant(typedArray, typedArray.GetType());
      MethodInfo containsMethod = _enumerableContains.MakeGenericMethod(elementType);

      return Expression.Call(
        containsMethod,
        valuesExpr,
        property
      );
    }
  }
}