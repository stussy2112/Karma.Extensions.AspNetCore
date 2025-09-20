// -----------------------------------------------------------------------
// <copyright file="ContainsOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  internal sealed class ContainsOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.Contains or Operator.NotContains)
  {
    private static readonly MethodInfo _stringContains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;
    private static readonly MethodInfo _enumerableContains = typeof(Enumerable).GetMethods().First(m => nameof(Enumerable.Contains).Equals(m.Name, StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length == 2);

    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      Expression? property = BuildPropertyAccessExpression(parameter, filter.Path!);
      if (property is null)
      {
        return TrueExpression; // Property does not exist
      }

      BinaryExpression containsExpression =
        BuildContainsExpression(property, filter.Values.FirstOrDefault());

      return filter.Operator switch
      {
        Operator.NotContains => Expression.Not(containsExpression),
        Operator.Contains => containsExpression,
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(ContainsOperatorHandler)}")
      };
    }

    private static BinaryExpression BuildContainsExpression(Expression property, object? comparisonValue)
    {
      // If property is string, use string.Contains
      if (property.Type == typeof(string))
      {
        return BuildStringExpression(property, _stringContains, comparisonValue);
      }

      // If property is IEnumerable<T> (but not string), use Enumerable.Contains<T>
      if (property.Type.IsAssignableTo(typeof(IEnumerable)))
      {
        return BuildEnumerableContainsExpression(property, comparisonValue);
      }

      // For ALL other types, convert property to string and use string.Contains
      // For non-nullable value types, no null check needed
      // For reference types and nullable value types, add null check
      BinaryExpression stringOperation = BuildStringExpression(property, _stringContains, comparisonValue);
      return IsNotNullableValueType(property.Type)
        ? stringOperation
        : Expression.AndAlso(
            Expression.NotEqual(property, Expression.Constant(null, property.Type)),
            stringOperation);
    }

    private static BinaryExpression BuildEnumerableContainsExpression(Expression property, object? comparisonValue)
    {
      ConstantExpression valueExpr = BuildValueAccessExpression(comparisonValue, property.Type);

      Type elementType = valueExpr.Type;
      MethodInfo containsMethod = _enumerableContains.MakeGenericMethod(elementType);

      Expression enumerableExpr = property.Type.IsArray
        ? property
        : Expression.Convert(property, typeof(IEnumerable<>).MakeGenericType(elementType));

      return Expression.AndAlso(
        Expression.NotEqual(enumerableExpr, Expression.Constant(null, enumerableExpr.Type)),
        Expression.Call(
          containsMethod,
          enumerableExpr,
          valueExpr
      ));
    }
  }
}