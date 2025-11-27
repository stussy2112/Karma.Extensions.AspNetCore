// -----------------------------------------------------------------------
// <copyright file="RangeOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles range operators (Between, NotBetween).
  /// Between operation: value &gt; lowerBound AND value &lt; upperBound (exclusive bounds).
  /// NotBetween operation: value &lt; lowerBound OR value &gt; upperBound (exclusive bounds, inverted logic).
  /// </summary>
  internal sealed class RangeOperatorHandler() : OperatorHandlerBase((op) => op is Operator.Between or Operator.NotBetween)
  {
    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter is null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      if (filter.Values.Count < 2)
      {
        return Expression.Constant(false); // Need two values for 'Between' and 'NotBetween' operator
      }

      if (filter.Values.Any((v) => v is null))
      {
        throw new InvalidOperationException($"Both values must be non-null for '{filter.Operator}' operator.");
      }

      return filter.Operator switch
      {
        Operator.Between => BuildBetweenExpression(parameter, filter),
        Operator.NotBetween => BuildNotBetweenExpression(parameter, filter),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(RangeOperatorHandler)}")
      };
    }

    private static Expression BuildBetweenExpression(ParameterExpression parameter, FilterInfo filter)
    {
      (Expression? property, Expression lowerBound, Expression upperBound) = GetBetweenValueExpressions(parameter, filter);
      if (property is null or ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return Expression.AndAlso(
          Expression.GreaterThan(property, lowerBound),
          Expression.LessThan(property, upperBound));
    }

    private static Expression BuildNotBetweenExpression(ParameterExpression parameter, FilterInfo filter)
    {
      (Expression? property, Expression lowerBound, Expression upperBound) = GetBetweenValueExpressions(parameter, filter);
      if (property is null or ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return Expression.OrElse(
          Expression.LessThan(property, lowerBound),
          Expression.GreaterThan(property, upperBound));
    }

    private static (Expression? property, ConstantExpression lowerBound, ConstantExpression upperBound) GetBetweenValueExpressions(ParameterExpression parameter, FilterInfo filter)
    {
      string propertyName = filter.Path!;
      (Expression property, IEnumerable<ConstantExpression> comparisonValueExpressions) = BuildValueAccessExpressions(parameter, propertyName, filter.Values);
      
      // If property doesn't exist, return null for all expressions
      if (property is null or ConstantExpression { Value: null })
      {
        return (property, Expression.Constant(null), Expression.Constant(null));
      }
      
      ConstantExpression lowerBoundExpr = comparisonValueExpressions.ElementAt(0);
      ConstantExpression upperBoundExpr = comparisonValueExpressions.ElementAt(1);
      return (property, lowerBoundExpr, upperBoundExpr);
    }
  }
}