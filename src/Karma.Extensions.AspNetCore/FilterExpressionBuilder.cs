// -----------------------------------------------------------------------
// <copyright file="FilterExpressionBuilder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides methods for building filter expressions and lambda functions for evaluating objects against a set of
  /// specified conditions.
  /// </summary>
  /// <remarks>This class is designed to construct expression trees and compile them into lambda functions that
  /// can be used to evaluate whether objects of a specified type satisfy a collection of filters. It supports a variety
  /// of filter operations, including logical combinations of conditions.</remarks>
  public static class FilterExpressionBuilder
  {
    private const string DefaultExpressionParameterName = "entity";
    private static readonly FrozenDictionary<Operator, IOperatorHandler> _operatorHandlerMap = CreateOperatorHandlerMap();
    private static readonly Expression _trueExpression = Expression.Constant(true);

    /// <summary>
    /// Builds a LINQ expression that represents a filter predicate based on the provided collection of filter criteria.
    /// </summary>
    /// <typeparam name="T">The type of the object being filtered.</typeparam>
    /// <param name="filters">A collection of filters to apply to the objects.</param>
    /// <returns>A LINQ expression that can be used to filter a collection of objects.</returns>
    public static Expression<Func<T, bool>> BuildExpression<T>(IEnumerable<IFilterInfo> filters)
    {
      if (filters is null || !filters.Any())
      {
        return (entity) => true;
      }

      ParameterExpression parameter = Expression.Parameter(typeof(T), DefaultExpressionParameterName);
      Expression expression = BuildMultipleFilterExpression<T>(parameter, filters);
      return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Builds a lambda function that evaluates whether an object of type <typeparamref name="T"/> satisfies the
    /// specified filters.
    /// </summary>
    /// <remarks>The returned lambda function is compiled from an expression tree that represents the logical
    /// combination of the provided filters. Ensure that the filters in the <paramref name="filters"/> collection are
    /// valid and compatible with the type <typeparamref name="T"/>.</remarks>
    /// <typeparam name="T">The type of the object to be evaluated by the lambda function.</typeparam>
    /// <param name="filters">A collection of filters that define the conditions to be applied to objects of type <typeparamref name="T"/>.</param>
    /// <returns>A compiled lambda function that returns <see langword="true"/> if the object satisfies all the specified
    /// filters; otherwise, <see langword="false"/>.</returns>
    public static Func<T, bool> BuildLambda<T>(IEnumerable<IFilterInfo> filters) =>
      BuildExpression<T>(filters).Compile();

    /// <summary>
    /// Creates a dictionary mapping each operator to its corresponding handler.
    /// </summary>
    /// <returns>A read-only dictionary with operator-to-handler mappings.</returns>
    private static FrozenDictionary<Operator, IOperatorHandler> CreateOperatorHandlerMap()
    {
      IOperatorHandler[] handlers = [
        new EqualityOperatorHandler(),
        new ComparisonOperatorHandler(),
        new ContainsOperatorHandler(),
        new StringOperatorHandler(),
        new MembershipOperatorHandler(),
        new RangeOperatorHandler(),
        new NullOperatorHandler(),
        new RegexOperatorHandler(),
      ];

      Dictionary<Operator, IOperatorHandler> operatorMap = new ();

      // Build mapping by checking which operators each handler can handle
      foreach (IOperatorHandler handler in handlers)
      {
        foreach (Operator op in Enum.GetValues<Operator>().Where(handler.CanHandle))
        {
          operatorMap[op] = handler;
        }
      }

      return operatorMap.ToFrozenDictionary();
    }

    private static List<Expression> BuildExpressions<T>(ParameterExpression parameter, IEnumerable<IFilterInfo> filters)
    {
      List<Expression> expressions = [];

      foreach (IFilterInfo filter in filters.Where((fi) => fi is not null))
      {
        Expression? expression = BuildFilterExpression<T>(parameter, filter);
        if (expression is not null)
        {
          expressions.Add(expression);
        }
      }

      return expressions;
    }

    /// <summary>
    /// Builds a LINQ expression that represents a filter predicate based on the provided collection of filter criteria.
    /// </summary>
    /// <remarks>This method supports both individual filter conditions and nested filter groups.  Nested
    /// filter groups are combined using the conjunction specified in the <see cref="FilterInfoCollection"/>. If no
    /// valid filters are provided, the resulting expression defaults to a predicate that always evaluates to <see
    /// langword="true"/>.</remarks>
    /// <typeparam name="T">The type of the object being filtered.</typeparam>
    /// <param name="parameter">The parameter expression representing the object being filtered.</param>
    /// <param name="filters">A collection of <see cref="IFilterInfo"/> objects that define the filtering criteria.  Nested filter collections
    /// and individual filter conditions are supported.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> of type <see cref="Func{T, Boolean}"/> that evaluates to <see
    /// langword="true"/>  for objects matching the specified filter criteria. If no filters are provided, the
    /// expression always evaluates to <see langword="true"/>.</returns>
    private static Expression BuildMultipleFilterExpression<T>(ParameterExpression parameter, IEnumerable<IFilterInfo>? filters)
    {
      if (filters is null || !filters.Any())
      {
        return _trueExpression;
      }

      List<Expression> expressions = BuildExpressions<T>(parameter, filters);
      Conjunction conjunction = filters is FilterInfoCollection conditionGroup
        ? conditionGroup.Conjunction
        : Conjunction.And;

      return CombineExpressions(expressions, conjunction);
    }

    private static Expression? BuildFilterExpression<T>(ParameterExpression parameter, IFilterInfo filter) =>
      filter switch
      {
        FilterInfoCollection filterInfos => BuildMultipleFilterExpression<T>(parameter, filterInfos),
        FilterInfo filterInfo when !string.IsNullOrWhiteSpace(filterInfo.Path) => BuildSingleFilterExpression(parameter, filterInfo),
        _ => null
      };

    /// <summary>
    /// Builds a comparison expression based on the provided filter criteria.
    /// </summary>
    /// <remarks>
    /// This method constructs LINQ expressions for various comparison operations including equality, 
    /// inequality, range checks, string operations, and null checks. It supports all operators defined 
    /// in the <see cref="Operator"/> enumeration and handles type conversions automatically.
    /// </remarks>
    /// <param name="parameter">The parameter expression representing the object being filtered.</param>
    /// <param name="filter">The filter information containing the property path, operator, and comparison values.</param>
    /// <returns>An expression that evaluates to <see langword="true"/> if the object satisfies the filter criteria; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported operator is encountered.</exception>
    private static Expression BuildSingleFilterExpression(ParameterExpression parameter, FilterInfo filter)
    {
      if (filter is null || string.IsNullOrWhiteSpace(filter.Path))
      {
        return _trueExpression; // No valid filter to compare
      }

      if (_operatorHandlerMap.TryGetValue(filter.Operator, out IOperatorHandler? handler))
      {
        return handler.BuildExpression(parameter, filter);
      }

      throw new NotSupportedException($"Operator '{filter.Operator}' is not supported. Supported operators: {string.Join(", ", _operatorHandlerMap.Keys)}");
    }

    private static Expression CombineExpressions(List<Expression> expressions, Conjunction conjunction)
    {
      // If all filters are null or invalid, return true (match everything)
      if (expressions.Count == 0)
      {
        return _trueExpression;
      }

      return conjunction == Conjunction.Or
        ? expressions.Aggregate(Expression.OrElse)
        : expressions.Aggregate(Expression.AndAlso);
    }
  }
}