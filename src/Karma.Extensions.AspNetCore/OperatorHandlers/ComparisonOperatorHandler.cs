// -----------------------------------------------------------------------
// <copyright file="ComparisonOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles relational comparison operators (greater than, less than, and their equal variants).
  /// </summary>
  internal sealed class ComparisonOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.GreaterThan or Operator.LessThan or Operator.GreaterThanOrEqualTo or Operator.LessThanOrEqualTo)
  {
    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      (Expression? property, ConstantExpression? constant) = BuildValueAccessExpressions(parameter, filter.Path!, filter.Values.FirstOrDefault());
      if (property is null)
      {
        return TrueExpression; // Property does not exist
      }

      return filter.Operator switch
      {
        Operator.GreaterThan => Expression.GreaterThan(property, constant),
        Operator.LessThan => Expression.LessThan(property, constant),
        Operator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(property, constant),
        Operator.LessThanOrEqualTo => Expression.LessThanOrEqual(property, constant),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(ComparisonOperatorHandler)}")
      };
    }
  }
}