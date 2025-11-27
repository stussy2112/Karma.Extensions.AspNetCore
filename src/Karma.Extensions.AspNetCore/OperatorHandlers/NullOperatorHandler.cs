// -----------------------------------------------------------------------
// <copyright file="NullOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles null checking operators (IsNull, IsNotNull).
  /// </summary>
  internal sealed class NullOperatorHandler (): OperatorHandlerBase(static (op) => op is Operator.IsNull or Operator.IsNotNull)
  {
    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return TrueExpression; // No valid filter to compare
      }

      (Expression? property, Expression? _) = BuildValueAccessExpressions(parameter, filter.Path!, filter.Values.FirstOrDefault());
      
      // Check if property doesn't exist (BuildValueAccessExpressions returns Expression.Constant(null) in this case)
      if (property is null or ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return filter.Operator switch
      {
        Operator.IsNull => Expression.Equal(property, Expression.Constant(null, property.Type)),
        Operator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null, property.Type)),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(NullOperatorHandler)}")
      };
    }
  }
}