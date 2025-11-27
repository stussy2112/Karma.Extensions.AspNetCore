// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Karma.Extensions.AspNetCore.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides extension methods for working with <see cref="IEnumerable{T}"/> sequences, including grouping elements
  /// into dictionaries and filtering/paging/sorting sequences.
  /// </summary>
  /// <remarks>This static class contains utility methods designed to simplify common operations on enumerable
  /// sequences. It includes methods for grouping elements by a key and filtering sequences based on query parameters 
  /// extracted from an HTTP context.</remarks>
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Applies sorting operations to the specified source collection based on the provided sort information.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <param name="sortInfos">A collection of <see cref="SortInfo"/> that define the sorting criteria. If null or empty, the source is returned unchanged.</param>
    /// <param name="source">The collection of elements to be sorted. If null, returns null.</param>
    /// <returns>A new collection of elements sorted according to the specified <paramref name="sortInfos"/>, or the original collection if no sorting is applied.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Apply<T>(this IEnumerable<SortInfo>? sortInfos, IEnumerable<T>? source)
    {
      if (source is null || sortInfos is null || !sortInfos.Any())
      {
        return source;
      }

      IOrderedEnumerable<T>? result = null;
      Func<SortInfo, Func<T, object?>?> keySelectorFactory = (si) => GetOrCreatePropertySelector<T>(si.FieldName);
      foreach (SortInfo item in sortInfos)
      {
        Func<T, object?>? keySelector = keySelectorFactory(item);
        if (keySelector is null)
        {
          continue; // Property does not exist, skip this sort info
        }

        result = ApplySort(source, result, item, keySelector);
      }

      return result ?? source;
    }

    /// <summary>
    /// Applies the specified collection of filters to the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="filter">The collection of filters to apply. If null, no filtering is performed.</param>
    /// <param name="source">The sequence of elements to filter. If null or empty, returns the original sequence.</param>
    /// <returns>A filtered sequence of elements that satisfy the conditions defined by the filters, or the original sequence if no filters are applied.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Apply<T>(this FilterInfoCollection? filter, IEnumerable<T>? source)
    {
      if (source is null || filter is null)
      {
        return source;
      }

      return source.Where(FilterExpressionBuilder.BuildLambda<T>(filter));
    }

    /// <summary>
    /// Returns a paginated subset of the specified sequence according to the provided page information using offset-based paging.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="pageInfo">The pagination information. If null, the method returns the source unchanged.</param>
    /// <param name="source">The sequence of elements to paginate. If null or empty, the method returns the original value.</param>
    /// <returns>An enumerable containing the paginated subset of the source sequence, or the original source if it is null, empty, or if page information is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Apply<T>(this PageInfo pageInfo, IEnumerable<T>? source) =>
      source is null || pageInfo is null
        ? source
        : source.PaginateWithOffset(pageInfo);

    /// <summary>
    /// Returns a paginated subset of the source collection using cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// If both 'before' and 'after' cursors are provided, the 'before' cursor takes precedence. The method always orders
    /// the source collection by the cursor property to ensure deterministic pagination. If cursor values cannot be parsed,
    /// the first <see cref="PageInfo.Limit"/> items from the sorted source are returned.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TValue">The type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="pageInfo">The pagination information, including cursor values and limit settings.</param>
    /// <param name="source">The collection of items to paginate. If null or empty, the method returns the input value unchanged.</param>
    /// <param name="cursorProperty">A function that selects the cursor value from each item. Cannot be null.</param>
    /// <returns>A paginated subset of the source collection. Returns null if source is null; otherwise a subset based on the pagination criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Apply<T, TValue>(this PageInfo pageInfo, IEnumerable<T>? source, Func<T, TValue?> cursorProperty)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      IOrderedEnumerable<T> sortedSource = source.OrderBy(cursorProperty);
      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);

      // If the before value is provided and valid, it indicates that we want items that come before the specified cursor.
      //if (!string.IsNullOrWhiteSpace(before) && TValue.TryParse(before, null, out TValue? beforeParsed) && beforeParsed is not null)
      if (UseBeforePaging(pageInfo.Before, out TValue? beforeCursorVal))
      {
        return sortedSource.IterateBefore(beforeCursorVal, cursorProperty, limit);
      }

      //if (!string.IsNullOrWhiteSpace(after) && TValue.TryParse(after, null, out TValue? afterParsed) && afterParsed is not null)
      if (UseAfterPaging(pageInfo.After, out TValue? afterCursorVal))
      {
        return sortedSource.IterateAfter(afterCursorVal, cursorProperty, limit);
      }

      // If cursor values couldn't be parsed, return source unchanged
      return sortedSource.Take(limit);
    }

    /// <summary>
    /// Returns a paginated subset of the source collection using cursor-based pagination for a non-nullable struct cursor type.
    /// </summary>
    /// <remarks>
    /// If both 'before' and 'after' cursors are provided, the 'before' cursor takes precedence. The method always orders
    /// the source collection by the cursor property to ensure deterministic pagination. If cursor values cannot be parsed,
    /// the first <see cref="PageInfo.Limit"/> items from the sorted source are returned.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TValue">The non-nullable struct type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="pageInfo">The pagination information, including cursor values and limit settings.</param>
    /// <param name="source">The collection of items to paginate. If null or empty, the method returns the input value unchanged.</param>
    /// <param name="cursorProperty">A function that selects the cursor value from each item. Cannot be null.</param>
    /// <returns>A paginated subset of the source collection. Returns null if source is null; otherwise a subset based on the pagination criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Apply<T, TValue>(this PageInfo pageInfo, IEnumerable<T>? source, Func<T, TValue?> cursorProperty)
      where TValue : struct, IComparable<TValue>, IParsable<TValue>
    {
      ArgumentNullException.ThrowIfNull(cursorProperty);

      if (source is null || pageInfo is null)
      {
        return source;
      }

      // If the value of 'before' is provided, it takes precedence over 'after'.
      // This means that if both cursors are present, the method will use 'before' for pagination.
      // If only 'after' is provided, it will be used for pagination.
      // This design choice simplifies the pagination logic and avoids potential conflicts between the two cursor values.

      // Always order by the cursor column to ensure a consistent, deterministic sort.
      // For nullable types, nulls sort first
      IOrderedEnumerable<T> sortedSource = source.OrderBy(cursorProperty);
      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);

      // If the before value is provided and valid, it indicates that we want items that come before the specified cursor.
      if (UseBeforePaging(pageInfo.Before, out TValue beforeParsed))
      {
        return sortedSource.IterateBefore(beforeParsed, cursorProperty, limit);
      }

      if (UseAfterPaging(pageInfo.After, out TValue afterParsed))
      {
        return sortedSource.IterateAfter(afterParsed, cursorProperty, limit);
      }

      // If cursor values couldn't be parsed, return source unchanged
      return sortedSource.Take(limit);
    }

    /// <summary>
    /// Filters the elements of the specified sequence according to the provided filter collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to filter. Cannot be null.</param>
    /// <param name="filters">A collection of filters to apply to the sequence. If null or empty, no filtering is performed.</param>
    /// <returns>An enumerable collection containing elements from the source sequence that match the specified filters. If no
    /// filters are provided, returns the original source sequence. Returns null if <paramref name="source"/> is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Filter<T>(this IEnumerable<T> source, FilterInfoCollection? filters)
    {
      if (source is null || filters is null || filters.Count <= 0)
      {
        return source;
      }

      return filters.Apply(source);
    }

    /// <summary>
    /// Filters the elements of the specified sequence based on query parameters provided in the HTTP context.
    /// </summary>
    /// <remarks>The method uses filter information stored in the <see cref="HttpContext.Items"/> collection
    /// under the key <c>ContextItemKeys.Filters</c>. If no filters are found or the filter collection is empty, the
    /// method returns the original sequence.</remarks>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to filter. If <paramref name="source"/> is <see langword="null"/>, the method returns
    /// <see langword="null"/>.</param>
    /// <param name="httpContext">The HTTP context containing query parameters used to build the filter. If <paramref name="httpContext"/> is <see
    /// langword="null"/> or does not contain valid filter information, the method returns the original sequence.</param>
    /// <returns>A filtered sequence of elements that satisfy the query parameters, or the original sequence if no filters are
    /// applied. Returns <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? FilterByQuery<T>(this IEnumerable<T>? source, HttpContext httpContext)
    {
      if (source is null
        || httpContext is null
        || !httpContext.Items.TryGetValue(ContextItemKeys.Filters, out object? filtersObj)
        || filtersObj is not FilterInfoCollection filters
        || filters.Count <= 0)
      {
        return source;
      }

      return filters.Apply(source);
    }

    /// <summary>
    /// Returns a subset of elements from the source sequence according to the specified paging information.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to be paged. If null, the method returns null.</param>
    /// <param name="pageInfo">The paging information that determines which subset of elements to return. If null, the method returns the
    /// source sequence unchanged.</param>
    /// <returns>An enumerable containing the paged subset of elements from the source sequence, or the original sequence if
    /// paging information is not provided. Returns null if the source sequence is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Page<T>(this IEnumerable<T> source, PageInfo? pageInfo)
    {
      if (source is null || pageInfo is null)
      {
        return source;
      }

      return pageInfo.Apply(source);
    }

    /// <summary>
    /// Paginates the specified sequence using the provided page number and page size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to paginate. If null, returns null.</param>
    /// <param name="pageNumber">The page number to retrieve. If less than 1, defaults to 1.</param>
    /// <param name="pageSize">The number of elements to include in each page. If less than 1, returns the original sequence.</param>
    /// <returns>A paginated subset of the source sequence, or the original sequence if <paramref name="pageSize"/> is invalid.</returns>
    /// /// <remarks>
    /// This method uses 1-based page numbering. For large page numbers with in-memory collections,
    /// consider using <see cref="List{T}"/> or <see cref="IQueryable{T}"/> for better performance.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Page<T>(this IEnumerable<T>? source, int pageNumber, int pageSize)
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

      PageInfo pageInfo = new (offset, limit);

      return pageInfo.Apply(source);
    }

    /// <summary>
    /// Paginates the specified sequence using cursor values from the provided <see cref="PageInfo"/>.
    /// </summary>
    /// <remarks>
    /// The sequence is ordered by the cursor property to ensure deterministic pagination. If both 'before' and 'after'
    /// cursors are present, 'before' takes precedence. If no valid cursor is provided, the first <see cref="PageInfo.Limit"/>
    /// items are returned.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TValue">The type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="source">The sequence of elements to paginate. If null, returns null.</param>
    /// <param name="pageInfo">The pagination information containing cursor values and limit. If null, the original sequence is returned.</param>
    /// <param name="cursorProperty">A function that selects the cursor value from each item. Cannot be null.</param>
    /// <returns>A paginated subset of the source sequence, or the original sequence if <paramref name="pageInfo"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Page<T, TValue>(this IEnumerable<T>? source, PageInfo? pageInfo, Func<T, TValue?> cursorProperty)
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
    /// Paginates the specified sequence using cursor values from the provided <see cref="PageInfo"/> for a non-nullable struct cursor type.
    /// </summary>
    /// <remarks>
    /// The sequence is ordered by the cursor property to ensure deterministic pagination. If both 'before' and 'after'
    /// cursors are present, 'before' takes precedence. If no valid cursor is provided, the first <see cref="PageInfo.Limit"/>
    /// items are returned.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TValue">The non-nullable struct type of the cursor value. Must implement <see cref="IComparable{T}"/> and <see cref="IParsable{TSelf}"/>.</typeparam>
    /// <param name="source">The sequence of elements to paginate. If null, returns null.</param>
    /// <param name="pageInfo">The pagination information containing cursor values and limit. If null, the original sequence is returned.</param>
    /// <param name="cursorProperty">A function that selects the cursor value from each item. Cannot be null.</param>
    /// <returns>A paginated subset of the source sequence, or the original sequence if <paramref name="pageInfo"/> is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cursorProperty"/> is null.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Page<T, TValue>(this IEnumerable<T>? source, PageInfo? pageInfo, Func<T, TValue?> cursorProperty)
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
    /// Paginates a collection of items based on query parameters provided in the HTTP context.
    /// </summary>
    /// <remarks>This method supports both offset-based and cursor-based pagination: <list type="bullet">
    /// <item> <description> For offset-based pagination, the method uses the <c>Offset</c> and <c>Limit</c> values from
    /// the <see cref="PageInfo"/> object in the HTTP context. </description> </item> <item> <description> For
    /// cursor-based pagination, the method uses the <c>Before</c> or <c>After</c> cursor values and orders the
    /// collection by the field specified in <paramref name="cursorProperty"/>. </description> </item> </list> If
    /// both <c>Before</c> and <c>After</c> are empty, the method defaults to offset-based pagination.</remarks>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TValue">The type of the field used for cursor-based pagination.</typeparam>
    /// <param name="source">The collection of items to paginate. If <paramref name="source"/> is <see langword="null"/>, the method returns
    /// <see langword="null"/>.</param>
    /// <param name="httpContext">The HTTP context containing pagination information. If <paramref name="httpContext"/> is <see langword="null"/>
    /// or does not contain valid pagination data, the method returns the original <paramref name="source"/>.</param>
    /// <param name="cursorProperty">A function that selects the field value used for cursor-based pagination. This parameter is required if
    /// cursor-based pagination is used.</param>
    /// <returns>A paginated subset of the source collection based on the pagination parameters. If no pagination parameters are
    /// provided, the method returns the original <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cursorProperty"/> is <see langword="null"/> when cursor-based pagination is
    /// required.</exception>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? PageByQuery<T, TValue>(this IEnumerable<T> source, HttpContext httpContext, Func<T, TValue?> cursorProperty)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      if (source is null
        || httpContext is null
        || !httpContext.Items.TryGetValue(ContextItemKeys.PageInfo, out object? pageQuery)
        || pageQuery is not PageInfo pageInfo)
      {
        return source;
      }

      if (string.IsNullOrWhiteSpace(pageInfo.Before) && string.IsNullOrWhiteSpace(pageInfo.After))
      {
        return pageInfo.Apply(source);
      }

      return pageInfo.Apply(source, cursorProperty);
    }

    /// <summary>
    /// Sorts the elements of the specified sequence according to the provided sorting criteria.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to sort. Cannot be null.</param>
    /// <param name="sortInfos">A collection of sorting criteria that defines the order in which to sort the elements. If null or empty, the
    /// original sequence is returned unchanged.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the sorted elements if sorting criteria are provided; otherwise, the
    /// original sequence. Returns null if <paramref name="source"/> is null.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? Sort<T>(this IEnumerable<T> source, IEnumerable<SortInfo>? sortInfos)
    {
      if (source is null || sortInfos is null || !sortInfos.Any())
      {
        return source;
      }

      return sortInfos.Apply(source);
    }

    /// <summary>
    /// Sorts the elements of the specified <paramref name="source"/> sequence based on the sorting criteria provided in
    /// the HTTP context.
    /// </summary>
    /// <remarks>The sorting is performed based on the property name specified in the
    /// <c>SortInfo.FieldName</c> property. If the property does not exist on the elements of the sequence, the original
    /// sequence is returned. The sorting direction is determined by the <c>SortInfo.Direction</c> property, which can
    /// be either ascending or descending.</remarks>
    /// <typeparam name="T">The type of elements in the <paramref name="source"/> sequence.</typeparam>
    /// <param name="source">The sequence of elements to sort. If <paramref name="source"/> is <see langword="null"/>, the method returns
    /// <see langword="null"/>.</param>
    /// <param name="httpContext">The HTTP context containing the sorting information. The sorting criteria must be stored in the <see
    /// cref="HttpContext.Items"/> collection under the key <c>ContextItemKeys.SortInfo</c>, and the value must be of
    /// type <c>SortInfo</c>.</param>
    /// <returns>A new sequence of elements sorted according to the specified sorting criteria, or the original <paramref
    /// name="source"/> sequence if no valid sorting information is found.</returns>
    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? SortByQuery<T>(this IEnumerable<T> source, HttpContext httpContext)
    {
      if (source is null
        || httpContext is null
        || !httpContext.Items.TryGetValue(ContextItemKeys.SortInfo, out object? sortQuery)
        || sortQuery is not IEnumerable<SortInfo> sortInfo
        || !sortInfo.Any())
      {
        return source;
      }

      return sortInfo.Apply(source);
    }

    /// <summary>
    /// Converts an <see cref="IEnumerable"/> from one enumerable type to another.
    /// </summary>
    /// <typeparam name="TEnumerable">The target type for the resulting <see cref="IEnumerable"/>.</typeparam>
    /// <param name="items">The <see cref="IEnumerable"/> that will be converted.</param>
    /// <returns>An <see cref="IEnumerable"/> that will be of type <typeparamref name="TEnumerable"/>.</returns>
    internal static TEnumerable? ConvertEnumerable<TEnumerable>(this IEnumerable? items) =>
      ConvertEnumerable(items, typeof(TEnumerable)) is IEnumerable converted ? (TEnumerable)converted : default;

    /// <summary>
    /// Converts an <see cref="IEnumerable"/> from one enumerable type to another.
    /// </summary>
    /// <param name="items">The <see cref="IEnumerable"/> that will be converted.</param>
    /// <param name="targetEnumerableType">The target type for the resulting <see cref="IEnumerable"/>.</param>
    /// <returns>An <see cref="IEnumerable"/> that will be of type <paramref name="targetEnumerableType"/>.</returns>
    internal static IEnumerable? ConvertEnumerable(this IEnumerable? items, Type targetEnumerableType)
    {
      if (items is null || !targetEnumerableType.IsEnumerable(out Type? contained))
      {
        return default;
      }

      if (!targetEnumerableType.IsAbstract && !targetEnumerableType.IsInterface && !targetEnumerableType.IsArray)
      {
        return ConvertEnumerableByMethod(items, targetEnumerableType, contained);
      }

      IList? tempList = ConvertEnumerableAsList(items, contained);

      if (tempList is not null && targetEnumerableType.IsArray)
      {
        var array = Array.CreateInstance(contained, tempList.Count);
        tempList.CopyTo(array, 0);
        return array;
      }

      return tempList;
    }

    /// <summary>
    /// Groups elements of a sequence into a dictionary based on a specified key selector function.
    /// </summary>
    /// <remarks>This method iterates through the source sequence and groups elements by the key returned from
    /// the <paramref name="keySelector"/> function. If multiple elements share the same key, they are added to the same
    /// list in the resulting dictionary.</remarks>
    /// <typeparam name="TKey">The type of the keys in the resulting dictionary. Must be non-nullable.</typeparam>
    /// <typeparam name="TValue">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of elements to group. Cannot be <see langword="null"/>.</param>
    /// <param name="keySelector">A function to extract the key for each element. Cannot be <see langword="null"/>.</param>
    /// <param name="equalityComparer">An optional equality comparer to compare keys. If <see langword="null"/>, the default equality comparer for
    /// <typeparamref name="TKey"/> is used.</param>
    /// <returns>A dictionary where each key is associated with a list of elements from the source sequence that share the same
    /// key.</returns>
    internal static Dictionary<TKey, List<TValue>> CreateGroupDictionary<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IEqualityComparer<TKey>? equalityComparer = default) where TKey : notnull
    {
      ArgumentNullException.ThrowIfNull(source);
      ArgumentNullException.ThrowIfNull(keySelector);

      Dictionary<TKey, List<TValue>> groupDictionary = new(equalityComparer ?? EqualityComparer<TKey>.Default);
      foreach (TValue item in source)
      {
        TKey key = keySelector(item);
        if (!groupDictionary.TryGetValue(key, out List<TValue>? children))
        {
          children = [];
          groupDictionary[key] = children;
        }

        children.Add(item);
      }

      return groupDictionary;
    }

    private static IOrderedEnumerable<T> ApplySort<T>(IEnumerable<T> source, IOrderedEnumerable<T>? orderedResult, SortInfo sortInfo, Func<T, object?> keySelector)
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

    private static IList? ConvertEnumerableAsList(this IEnumerable? items, Type contained)
    {
      if (items is null)
      {
        return default;
      }

      Type listType = typeof(List<>).MakeGenericType(contained);
      object? instance = Activator.CreateInstance(listType);

      if (instance is not IList tempList)
      {
        return default;
      }

      foreach (object? item in items)
      {
        _ = tempList.Add(item);
      }

      return tempList;
    }

    private static IEnumerable? ConvertEnumerableByMethod(this IEnumerable? items, Type targetType, Type containedType)
    {
      if (items is null)
      {
        return default;
      }

      object? instance = Activator.CreateInstance(targetType);

      if (instance is null)
      {
        return default;
      }

      // Check for Add method using reflection
      MethodInfo? addMethod = targetType.GetAddMethod(containedType);
      if (addMethod is null)
      {
        return default;
      }

      // Use reflection to call Add method for each item
      foreach (object? item in items)
      {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
          _ = addMethod.Invoke(instance, [item]);
        }
        catch
        {
          // If Add method fails, return default to indicate conversion failed
          return default;
        }
#pragma warning restore CA1031 // Do not catch general exception types
      }

      return instance as IEnumerable;
    }

    private static MethodInfo? GetAddMethod(this Type targetType, Type containedType)
    {
      // Check for Add method using reflection
      MethodInfo? addMethod = targetType.GetMethod(nameof(IList.Add), BindingFlags.Public | BindingFlags.Instance)
        ?? targetType.GetMethod(nameof(Stack.Push), BindingFlags.Public | BindingFlags.Instance)
        ?? targetType.GetMethod(nameof(Queue.Enqueue), BindingFlags.Public | BindingFlags.Instance)
        ?? targetType.GetMethod("AddLast", BindingFlags.Public | BindingFlags.Instance, [containedType]);

      // Verify the Add method has exactly one parameter
      if (addMethod is not null && addMethod.GetParameters().Length == 1)
      {
        return addMethod;
      }

      return null;
    }

    private const BindingFlags Binding_Attrs = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object?>> _propertySelectorCache = new();

    private static Func<T, object?>? GetOrCreatePropertySelector<T>(string propertyName)
    {
      (Type, string propertyName) key = (typeof(T), propertyName);
      return _propertySelectorCache.GetOrAdd(key, static (k) =>
      {
        (Type type, string propName) = k;
        PropertyInfo? property = type.GetProperty(propName, Binding_Attrs);
        if (property is null)
        {
          return null!;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(object), "obj");
        UnaryExpression castToType = Expression.Convert(parameter, type);
        MemberExpression propertyAccess = Expression.Property(castToType, property);
        UnaryExpression castToObject = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<object, object?>>(castToObject, parameter).Compile();
      }) as Func<T, object?>;
    }

    private static IEnumerable<T> IterateAfter<T, TValue>(this IEnumerable<T> source, TValue? cursorValue, Func<T, TValue?> cursorProperty, int limit)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      if (cursorValue is null)
      {
        foreach (T? item in source)
        {
          yield return item;
        }

        yield break;
      }

      int itemCount = 0;

      // For 'after' cursor, skip until we find the cursor, then return items
      foreach (T item in source)
      {
        TValue? itemCursorValue = cursorProperty(item);

        if (itemCursorValue is null || cursorValue.CompareTo(itemCursorValue) >= 0)
        {
          continue; // Skip `null` values
        }

        if (itemCount >= limit)
        {
          yield break;
        }

        yield return item;
        itemCount++;
      }
    }

    private static IEnumerable<T> IterateAfter<T, TValue>(this IEnumerable<T> source, TValue cursorValue, Func<T, TValue?> cursorProperty, int limit) where TValue
      : struct, IComparable<TValue>, IParsable<TValue>
    {
      int itemCount = 0;

      // For 'after' cursor, skip until we find the cursor, then return items
      foreach (T item in source)
      {
        TValue? itemCursorValue = cursorProperty(item);

        if (!itemCursorValue.HasValue || cursorValue.CompareTo(itemCursorValue.Value) >= 0)
        {
          continue; // Skip `null` values
        }

        if (itemCount >= limit)
        {
          yield break;
        }

        yield return item;
        itemCount++;
      }
    }

    private static IEnumerable<T> IterateBefore<T, TValue>(this IEnumerable<T> source, TValue? cursorValue, Func<T, TValue?> cursorProperty, int limit)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      if (cursorValue is null)
      {
        foreach (T? item in source)
        {
          yield return item;
        }

        yield break;
      }

      // For 'before' cursor, we need to collect items until we find the cursor
      limit = Math.Min(limit, int.MaxValue / 2);
      Queue<T> window = new(limit);

      foreach (T item in source)
      {
        TValue? itemCursorValue = cursorProperty(item);

        if (itemCursorValue is null)
        {
          continue; // Skip `null` values
        }

        if (cursorValue.CompareTo(itemCursorValue) <= 0)
        {
          break; // Stop when we reach or pass the cursor position 
        }

        window.Enqueue(item);

        // Keep only the last 'limit' items
        if (window.Count > limit)
        {
          _ = window.Dequeue();
        }
      }

      // Yield the items in the window (already the last 'limit' items)
      foreach (T item in window)
      {
        yield return item;
      }
    }

    private static IEnumerable<T> IterateBefore<T, TValue>(this IEnumerable<T> source, TValue cursorValue, Func<T, TValue?> cursorProperty, int limit) where TValue
      : struct, IComparable<TValue>, IParsable<TValue>
    {
      // For 'before' cursor, we need to collect items until we find the cursor
      limit = Math.Min(limit, int.MaxValue / 2);
      Queue<T> window = new(limit);

      foreach (T item in source)
      {
        TValue? itemCursorValue = cursorProperty(item);

        if (!itemCursorValue.HasValue)
        {
          continue; // Skip `null` values
        }

        if (cursorValue.CompareTo(itemCursorValue.Value) <= 0)
        {
          break; // Stop when we reach or pass the cursor position
        }

        window.Enqueue(item);

        // Keep only the last 'limit' items
        if (window.Count > limit)
        {
          _ = window.Dequeue();
        }
      }

      // Yield the items in the window (already the last 'limit' items)
      foreach (T item in window)
      {
        yield return item;
      }
    }

    private static IEnumerable<T> PaginateWithOffset<T>(this IEnumerable<T> source, PageInfo pageInfo)
    {
      int currentIndex = 0;
      int itemCount = 0;

      int limit = (int)Math.Min(pageInfo.Limit, int.MaxValue);
      int offset = (int)Math.Min(pageInfo.Offset, int.MaxValue);

      foreach (T item in source)
      {
        // Skip items until we reach the offset
        if (currentIndex < offset)
        {
          currentIndex++;
          continue;
        }

        // Stop if we've reached the limit
        if (itemCount >= limit)
        {
          yield break;
        }

        yield return item;
        itemCount++;
      }
    }

    private static bool UseAfterPaging<TValue>(string? after, out TValue? afterParsed)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      afterParsed = default;
      return !string.IsNullOrWhiteSpace(after) && TValue.TryParse(after, null, out afterParsed) && afterParsed is not null;
    }

    private static bool UseBeforePaging<TValue>(string? before, out TValue? beforeParsed)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      beforeParsed = default;
      return !string.IsNullOrWhiteSpace(before) && TValue.TryParse(before, null, out beforeParsed) && beforeParsed is not null;
    }
  }
}