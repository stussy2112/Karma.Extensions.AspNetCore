// -----------------------------------------------------------------------
// <copyright file="StringOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles string-based operators (StartsWith, EndsWith).
  /// </summary>
  internal sealed class StringOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.StartsWith or Operator.EndsWith)
  {
    private static readonly MethodInfo _startsWith = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;
    private static readonly MethodInfo _endsWith = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;

    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter == null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return Expression.Constant(true); // No valid filter to compare
      }

      Expression? property = BuildPropertyAccessExpression(parameter, filter.Path!);
      if (property is null or ConstantExpression { Value: null })
      {
        return TrueExpression; // Property does not exist
      }

      return filter.Operator switch
      {
        Operator.StartsWith => BuildStringExpression(property, _startsWith, filter.Values.FirstOrDefault()),
        Operator.EndsWith => BuildStringExpression(property, _endsWith, filter.Values.FirstOrDefault()),
        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported by {nameof(StringOperatorHandler)}")
      };
    }
  }
}