// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides extension methods for applying filtering, sorting, and pagination operations to IQueryable sequences,
  /// supporting both offset-based and cursor-based paging scenarios.
  /// </summary>
  /// <remarks>The methods in this class are designed to facilitate dynamic query composition for LINQ providers
  /// such as Entity Framework Core. They enable efficient server-side filtering, sorting, and paging by building
  /// expressions that can be translated to SQL queries. The extensions support multi-field sorting, offset and
  /// cursor-based pagination, and flexible filter criteria. All methods are null-safe and return the original sequence
  /// unchanged if no applicable criteria are provided or if the source is null.</remarks>
  public static class IQueryableExtensions
  {
    /// <summary>
    /// Applies the specified collection of filters to the given queryable data source.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the queryable data source.</typeparam>
    /// <param name="filters">A collection of filter criteria to apply to the data source. If null or empty, no filtering is performed.</param>
    /// <param name="source">The queryable data source to filter. If null, the method returns null.</param>
    /// <returns>An IQueryable containing elements from the source that match all specified filters. Returns the original source
    /// if filters is null or empty, or if source is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Apply<T>(this FilterInfoCollection? filters, IQueryable<T>? source)
    {
      if (source is null || filters is null || filters.Count <= 0)
      {
        return source;
      }

      return source.Where(FilterExpressionBuilder.BuildExpression<T>(filters));
    }

    /// <summary>
    /// Applies pagination to an <see cref="IQueryable{T}"/> sequence using offset-based pagination.
    /// </summary>
    /// <remarks>
    /// This method is optimized for Entity Framework Core and other LINQ providers that can translate
    /// <see cref="Queryable.Skip{TSource}"/> and <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)"/> to efficient database queries
    /// (e.g., SQL OFFSET and FETCH). If <paramref name="pageInfo"/> is null, the original sequence is returned unchanged.
    /// The query is not executed until materialized with methods like <c>ToList()</c> or <c>ToListAsync()</c>.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="pageInfo">The pagination information containing offset and limit values. If null, returns the source unchanged.</param>
    /// <param name="source">The queryable sequence to paginate. If null, returns null.</param>
    /// <returns>An <see cref="IQueryable{T}"/> representing the paginated query, or the original sequence if no pagination is applied.
    /// Returns <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Apply<T>(this PageInfo? pageInfo, IQueryable<T>? source)
    {
      if (source is null || pageInfo is null)
      {
        return source;
      }

      int offset = (int)Math.Min(pageInfo.Offset, int.MaxValue);
      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);

      return source.Skip(offset).Take(limit);
    }

    /// <summary>
    /// Applies sorting to an <see cref="IQueryable{T}"/> sequence based on the provided sort criteria.
    /// </summary>
    /// <remarks>
    /// This method builds dynamic sorting expressions that can be translated to SQL ORDER BY clauses by Entity Framework Core
    /// and other LINQ providers. If no valid sort criteria are provided or if properties don't exist on type <typeparamref name="T"/>,
    /// the original sequence is returned unchanged. The method supports multi-field sorting with ascending and descending directions.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="sortInfos">A collection of <see cref="SortInfo"/> objects defining the sort criteria. If null or empty, the source is returned unchanged.</param>
    /// <param name="source">The queryable sequence to sort. If null, returns null.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with sorting applied, or the original sequence if no sorting is applied.
    /// Returns <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Apply<T>(this IEnumerable<SortInfo>? sortInfos, IQueryable<T>? source)
    {
      if (source is null || sortInfos is null || !sortInfos.Any())
      {
        return source;
      }

      IOrderedQueryable<T>? result = null;
      Func<SortInfo, Expression<Func<T, object?>>?> keySelectorFactory = (si) => GetOrCreatePropertySelectorExpression<T>(si.FieldName);

      foreach (SortInfo item in sortInfos)
      {
        Expression<Func<T, object?>>? keySelector = keySelectorFactory(item);
        if (keySelector is null)
        {
          continue; // Property does not exist, skip this sort info
        }

        result = ApplySort(source, result, item, keySelector);
      }

      return result ?? source;
    }

    /// <summary>
    /// Applies cursor-based pagination to an <see cref="IQueryable{T}"/> using expression-based filtering.
    /// </summary>
    /// <remarks>
    /// This method builds LINQ expressions that translate to SQL WHERE clauses, enabling efficient
    /// cursor-based pagination at the database level. The 'before' cursor takes precedence if both are provided.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TValue">The type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="pageInfo">The pagination information containing cursor values and limit.</param>
    /// <param name="source">The queryable sequence to paginate. If null, returns null.</param>
    /// <param name="cursorProperty">An expression that selects the cursor property. Cannot be null.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with cursor-based pagination applied, or null if source is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Apply<T, TValue>(this PageInfo? pageInfo, IQueryable<T>? source, Expression<Func<T, TValue?>> cursorProperty)
        where TValue : IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);

      // Before cursor takes precedence
      if (PagingHelpers.UseBeforePaging(pageInfo.Before, out TValue? beforeCursorVal))
      {
        // Note: OrderByDescending for "before" cursor, then reverse results
        return source
            .Where(BuildCursorPredicate(cursorProperty, beforeCursorVal, Expression.LessThan))
            .OrderByDescending(cursorProperty)
            .Take(limit);
      }

      if (PagingHelpers.UseAfterPaging(pageInfo.After, out TValue? afterCursorVal))
      {
        return source
            .Where(BuildCursorPredicate(cursorProperty, afterCursorVal, Expression.GreaterThan))
            .OrderBy(cursorProperty)
            .Take(limit);
      }

      // No cursors - just order and take
      return source.OrderBy(cursorProperty).Take(limit);
    }

    /// <summary>
    /// Applies cursor-based pagination for non-nullable struct cursor types.
    /// </summary>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Apply<T, TValue>(this PageInfo? pageInfo, IQueryable<T>? source, Expression<Func<T, TValue?>> cursorProperty)
        where TValue : struct, IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);

      // Before cursor takes precedence
      if (PagingHelpers.UseBeforePaging(pageInfo.Before, out TValue beforeCursorVal))
      {
        // Note: OrderByDescending for "before" cursor, then reverse results
        return source
            .Where(BuildCursorPredicate(cursorProperty, beforeCursorVal, Expression.LessThan))
            .OrderByDescending(cursorProperty)
            .Take(limit);
      }

      if (PagingHelpers.UseAfterPaging(pageInfo.After, out TValue afterCursorVal))
      {
        return source
            .Where(BuildCursorPredicate(cursorProperty, afterCursorVal, Expression.GreaterThan))
            .OrderBy(cursorProperty)
            .Take(limit);
      }

      // No cursors - just order and take
      return source.OrderBy(cursorProperty).Take(limit);
    }

    /// <summary>
    /// Filters the elements of an <see cref="IQueryable{T}"/> based on the specified filter criteria.
    /// </summary>
    /// <remarks>If <paramref name="filters"/> is null or contains no filter criteria, the method returns the
    /// original <paramref name="source"/> sequence unchanged. The filtering is performed using the expressions built
    /// from the provided filters, which may affect query translation and performance depending on the underlying data
    /// provider.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to filter. Cannot be null.</param>
    /// <param name="filters">A collection of filter criteria to apply to the sequence. If null or empty, no filtering is performed.</param>
    /// <returns>An <see cref="IQueryable{T}"/> containing elements that match the specified filters, 
    /// or the original sequence if no filters are provided. 
    /// Returns <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Filter<T>(this IQueryable<T>? source, FilterInfoCollection? filters)
    {
      if (source is null || filters is null || filters.Count <= 0)
      {
        return source;
      }

      return filters.Apply(source);
    }

    /// <summary>
    /// Returns a paged subset of the source queryable sequence according to the specified paging information.
    /// </summary>
    /// <remarks>This method does not execute the query; it returns a queryable sequence with paging applied.
    /// Paging is typically performed using skip and take operations based on the provided PageInfo.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to page. If null, the method returns null.</param>
    /// <param name="pageInfo">The paging information that determines which subset of items to return. If null, no paging is applied.</param>
    /// <returns>An IQueryable containing the paged subset of items from the source sequence, or the original source if paging
    /// information is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Page<T>(this IQueryable<T>? source, PageInfo? pageInfo)
    {
      if (source is null || pageInfo is null)
      {
        return source;
      }

      return pageInfo.Apply(source);
    }

    /// <summary>
    /// Returns a paged subset of the source queryable, corresponding to the specified page number and page size.
    /// </summary>
    /// <remarks>This method is typically used to implement server-side paging in LINQ queries. The returned
    /// queryable will skip the appropriate number of items and take the specified page size. No items are returned if
    /// the page number exceeds the available data.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to page. Can be null.</param>
    /// <param name="pageNumber">The one-based page number to retrieve. If less than or equal to zero, defaults to 1.</param>
    /// <param name="pageSize">The number of items per page. Must be greater than zero; otherwise, the original source is returned.</param>
    /// <returns>An <see cref="IQueryable{T}"/> containing the items for the specified page, or the original source if it is null or pageSize
    /// is less than or equal to zero.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Page<T>(this IQueryable<T>? source, int pageNumber, int pageSize)
    {
      if (source is null || pageSize <= 0)
      {
        return source;
      }

      if (pageNumber <= 0)
      {
        pageNumber = 1;
      }

      int skip = checked((pageNumber - 1) * pageSize);

      uint offset = (uint)skip;
      uint limit = (uint)pageSize;

      PageInfo pageInfo = new(offset, limit);

      return pageInfo.Apply(source);
    }

    /// <summary>
    /// Paginates the specified queryable using cursor values from the provided <see cref="PageInfo"/>.
    /// </summary>
    /// <remarks>
    /// This method builds LINQ expressions that translate to SQL WHERE clauses for efficient cursor-based
    /// pagination at the database level. If both 'before' and 'after' cursors are present, 'before' takes precedence.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TValue">The type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="source">The queryable sequence to paginate. If null, returns null.</param>
    /// <param name="pageInfo">The pagination information containing cursor values and limit. If null, the original sequence is returned.</param>
    /// <param name="cursorProperty">An expression that selects the cursor property. Cannot be null.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with cursor-based pagination applied, or null if source is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Page<T, TValue>(this IQueryable<T>? source, PageInfo? pageInfo, Expression<Func<T, TValue?>> cursorProperty)
        where TValue : IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      return pageInfo.Apply(source, cursorProperty);
    }

    /// <summary>
    /// Paginates the specified queryable using cursor values for non-nullable struct cursor types.
    /// </summary>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Page<T, TValue>(this IQueryable<T>? source, PageInfo? pageInfo, Expression<Func<T, TValue?>> cursorProperty)
        where TValue : struct, IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      return pageInfo.Apply(source, cursorProperty);
    }

    /// <summary>
    /// Sorts the elements of an <see cref="IQueryable{T}"/> sequence according to the specified sort criteria.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to sort. Cannot be <see langword="null"/>.</param>
    /// <param name="sortInfos">A collection of <see cref="SortInfo"/> objects that define the sort order. If <see langword="null"/> or empty,
    /// the original sequence is returned unchanged.</param>
    /// <returns>An <see cref="IQueryable{T}"/> representing the sorted sequence, or the original sequence if <paramref
    /// name="sortInfos"/> is <see langword="null"/> or empty.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IQueryable<T>? Sort<T>(this IQueryable<T>? source, IEnumerable<SortInfo>? sortInfos)
    {
      if (source is null || sortInfos is null || !sortInfos.Any())
      {
        return source;
      }

      return sortInfos.Apply(source);
    }

    private static IOrderedQueryable<T> ApplySort<T>(IQueryable<T> source, IOrderedQueryable<T>? orderedResult, SortInfo sortInfo, Expression<Func<T, object?>> keySelector)
    {
      if (orderedResult is null)
      {
        return sortInfo.Direction is System.ComponentModel.ListSortDirection.Ascending
          ? source.OrderBy(keySelector)
          : source.OrderByDescending(keySelector);
      }

      return sortInfo.Direction is System.ComponentModel.ListSortDirection.Ascending
        ? orderedResult.ThenBy(keySelector)
        : orderedResult.ThenByDescending(keySelector);
    }

    /// <summary>
    /// Builds a comparison expression for cursor-based pagination that handles nullable types correctly.
    /// </summary>
    /// <typeparam name="TValue">The type of the cursor value.</typeparam>
    /// <param name="property">The member expression representing the property to compare.</param>
    /// <param name="cursorValue">The cursor value to compare against.</param>
    /// <param name="comparisonFactory">A function that creates the comparison expression (GreaterThan or LessThan).</param>
    /// <returns>An expression that performs the comparison with proper null handling.</returns>
    private static BinaryExpression BuildCursorComparisonExpression<TValue>(MemberExpression property, TValue cursorValue, Func<Expression, Expression, BinaryExpression> comparisonFactory)
        where TValue : IComparable<TValue>, IParsable<TValue>
    {
      // For string comparisons, use String.Compare instead of > or <
      if (typeof(TValue) == typeof(string))
      {
        return BuildStringComparisonExpression(property, cursorValue, comparisonFactory);
      }

      Type propertyType = property.Type;
      Type constantPropertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
      ConstantExpression constant = Expression.Constant(cursorValue, constantPropertyType);

      if (Nullable.GetUnderlyingType(propertyType) is not null)
      {
        // Nullable value type (e.g., int?)
        MemberExpression hasValue = Expression.Property(property, "HasValue");
        MemberExpression value = Expression.Property(property, "Value");
        BinaryExpression comparison = comparisonFactory(value, constant);
        return Expression.AndAlso(hasValue, comparison);
      }

      // Non-nullable value type
      return comparisonFactory(property, constant);
    }

    /// <summary>
    /// Builds a string comparison expression using String.Compare.
    /// </summary>
    private static BinaryExpression BuildStringComparisonExpression<TValue>(MemberExpression property, TValue cursorValue, Func<Expression, Expression, BinaryExpression> comparisonFactory)
    {
      // String.Compare(property, cursorValue) > 0 or < 0
      MethodInfo? compareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)]);

      ConstantExpression cursorConstantValue = Expression.Constant(cursorValue, typeof(string));
      ConstantExpression zero = Expression.Constant(0);
      MethodCallExpression compareCall = Expression.Call(compareMethod!, property, cursorConstantValue);
      BinaryExpression comparison = comparisonFactory(compareCall, zero);

      // Handle nullable strings
      if (Nullable.GetUnderlyingType(property.Type) is null && property.Type == typeof(string))
      {
        // Non-nullable string - add null check
        BinaryExpression notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        return Expression.AndAlso(notNull, comparison);
      }

      // For nullable string properties
      return comparison;
    }

    private static Expression<Func<T, bool>> BuildCursorPredicate<T, TValue>(Expression<Func<T, TValue?>> cursorProperty, TValue? cursorValue, Func<Expression, Expression, BinaryExpression> comparisonFactory)
        where TValue : IComparable<TValue>, IParsable<TValue>
    {
      ParameterExpression parameter = cursorProperty.Parameters[0];
      MemberExpression property = GetMemberExpression(cursorProperty);

      Expression comparison = BuildCursorComparisonExpression(property, cursorValue!, comparisonFactory);
      return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    private static Expression<Func<T, bool>> BuildCursorPredicate<T, TValue>(Expression<Func<T, TValue?>> cursorProperty, TValue cursorValue, Func<Expression, Expression, BinaryExpression> comparisonFactory)
        where TValue : struct, IComparable<TValue>, IParsable<TValue>
    {
      ParameterExpression parameter = cursorProperty.Parameters[0];
      MemberExpression property = GetMemberExpression(cursorProperty);

      Expression comparison = BuildCursorComparisonExpression(property, cursorValue, comparisonFactory);
      return Expression.Lambda<Func<T, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Extracts the MemberExpression from a lambda expression, handling UnaryExpression wrappers.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="cursorProperty">The cursor property expression.</param>
    /// <returns>The member expression representing the property access.</returns>
    private static MemberExpression GetMemberExpression<T, TValue>(Expression<Func<T, TValue?>> cursorProperty)
    {
      Expression body = cursorProperty.Body;

      // Handle Convert/UnaryExpression (common with nullable types)
      if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
      {
        body = unaryExpression.Operand;
      }

      if (body is not MemberExpression memberExpression)
      {
        throw new ArgumentException(
            $"Expression must be a property access. Expression type: {body.GetType().Name}",
            nameof(cursorProperty));
      }

      return memberExpression;
    }

    private static Expression<Func<T, object?>>? GetOrCreatePropertySelectorExpression<T>(string propertyName)
    {
      Expression<Func<object, object?>>? cachedExpression = PagingHelpers.GetPropertySelectorExpression<T>(propertyName);
      if (cachedExpression is null)
      {
        return null;
      }

      // Convert Expression<Func<object, object?>> to Expression<Func<T, object?>> without using Expression.Invoke
      ParameterExpression typedParameter = Expression.Parameter(typeof(T), "entity");

      ParameterReplaceVisitor replacer = new(cachedExpression.Parameters[0], typedParameter);
      Expression newBody = replacer.Visit(cachedExpression.Body)!;

      return Expression.Lambda<Func<T, object?>>(newBody, typedParameter);
    }

    // Helper visitor to replace a specific ParameterExpression with another expression
    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
      private readonly ParameterExpression _target;
      private readonly Expression _replacement;

      public ParameterReplaceVisitor(ParameterExpression target, Expression replacement)
      {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(replacement);
        (_target, _replacement) = (target, replacement);
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        if (node == _target)
        {
          return _replacement;
        }

        return base.VisitParameter(node);
      }
    }
  }
}
