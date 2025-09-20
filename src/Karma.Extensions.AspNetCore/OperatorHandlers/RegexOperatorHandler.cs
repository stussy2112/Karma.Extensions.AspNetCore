// -----------------------------------------------------------------------
// <copyright file="RegexOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Handles the construction of expressions for filters using the Regex operator.
  /// </summary>
  /// <remarks>This class is responsible for generating an expression that evaluates whether a property value
  /// matches a regular expression pattern. If the filter is invalid (e.g., null or missing a path), the resulting
  /// expression will always evaluate to <see langword="true"/>.</remarks>
  internal sealed class RegexOperatorHandler() : OperatorHandlerBase(static (op) => op is Operator.Regex)
  {
    public override Expression BuildExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter is null || string.IsNullOrWhiteSpace(filter.Path) || filter.Values.Count == 0)
      {
        return TrueExpression; // No valid filter to compare
      }

      Expression? property = BuildPropertyAccessExpression(parameter, filter.Path!);
      if (property is null)
      {
        return TrueExpression; // Property does not exist
      }

      MethodInfo isMatchMethod = typeof(Regex).GetMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string)])
        ?? throw new InvalidOperationException($"{nameof(Regex.IsMatch)} method not found");

      return Expression.AndAlso(
        Expression.NotEqual(property, Expression.Constant(null, property.Type)),
        Expression.Call(
          isMatchMethod,
          property,
          Expression.Constant(filter.Values.FirstOrDefault()?.ToString() ?? string.Empty, typeof(string))));
    }
  }
}