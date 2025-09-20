// -----------------------------------------------------------------------
// <copyright file="IOperatorHandler.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Defines a contract for building expressions for specific operators.
  /// </summary>
  internal interface IOperatorHandler
  {
    /// <summary>
    /// Determines if this handler can process the specified operator.
    /// </summary>
    /// <param name="operator">The operator to check.</param>
    /// <returns>True if this handler can process the operator; otherwise, false.</returns>
    bool CanHandle(Operator @operator);

    /// <summary>
    /// Builds an expression for the specified operator.
    /// </summary>
    /// <param name="parameter">The parameter expression representing the object being filtered.</param>
    /// <param name="filter">The filter information containing the property path, operator, and comparison values.</param>
    /// <returns>An expression that evaluates the operator condition.</returns>
    Expression BuildExpression(ParameterExpression parameter, FilterInfo filter);
  }
}