// -----------------------------------------------------------------------
// <copyright file="EqualityOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles equality and inequality comparison operators.
  /// </summary>
  internal sealed class EqualityOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.EqualTo or Operator.NotEqualTo)
  {
    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      (Expression property, ConstantExpression constant) = BuildValueAccessExpressions(parameter, filter.Path!, filter.Values.FirstOrDefault());
      
      // Check if property doesn't exist (BuildValueAccessExpressions returns Expression.Constant(null) in this case)
      if (property is ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return filter.Operator switch
      {
        Operator.EqualTo => Expression.Equal(property, constant),
        Operator.NotEqualTo => Expression.NotEqual(property, constant),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(EqualityOperatorHandler)}")
      };
    }
  }
}