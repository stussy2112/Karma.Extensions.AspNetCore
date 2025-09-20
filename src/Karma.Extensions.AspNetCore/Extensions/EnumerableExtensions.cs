// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
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
  /// Provides extension methods for working with <see cref="IEnumerable{T}"/> sequences,  including grouping elements
  /// into dictionaries and filtering sequences based on HTTP context query parameters.
  /// </summary>
  /// <remarks>This static class contains utility methods designed to simplify common operations on enumerable
  /// sequences. It includes methods for grouping elements by a key and filtering sequences based on query parameters 
  /// extracted from an HTTP context. These methods are intended to enhance the usability of LINQ and other 
  /// sequence-processing operations.</remarks>
  public static class EnumerableExtensions
  {
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
    public static IEnumerable<T>? FilterByQuery<T>(this IEnumerable<T> source, HttpContext httpContext)
    {
      if (source is null
        || httpContext is null
        || !httpContext.Items.TryGetValue(ContextItemKeys.Filters, out object? filtersObj)
        || filtersObj is not FilterInfoCollection filters
        || filters.Count <= 0)
      {
        return source;
      }

      // The FilterExpressionBuilder caches compiled expressions per type, 
      // making repeated calls with the same filter structure efficient
      return source.Where(FilterExpressionBuilder.BuildLambda<T>(filters));
    }

    [return: NotNullIfNotNull(nameof(source))]
    public static IEnumerable<T>? PageByQuery<T, TField>(this IEnumerable<T> source, HttpContext httpContext, Func<T, object> fieldValueSelector)
    {
      if (source is null
        || httpContext is null
        || !httpContext.Items.TryGetValue(ContextItemKeys.PageInfo, out object? pageQuery)
        || pageQuery is not PageInfo pageInfo)
      {
        return source;
      }

      int limit = (int)pageInfo.Limit;
      string before = pageInfo.Before;
      string after = pageInfo.After;

      if (string.IsNullOrWhiteSpace(before) && string.IsNullOrWhiteSpace(after))
      {
        int offset = (int)pageInfo.Offset;
        return source.Skip(offset).Take(limit);
      }

      if (fieldValueSelector is null)
      {
        throw new ArgumentNullException(nameof(fieldValueSelector), "Unable to page without a field value selector.");
      }

      string cursorValue = !string.IsNullOrWhiteSpace(before)
          ? before
          : after;

      // Always order by the cursor column to ensure a consistent, deterministic sort.
      return source
        .Where((item) => string.Compare(fieldValueSelector(item)?.ToString(), cursorValue, StringComparison.Ordinal) > 0)
        .OrderBy(fieldValueSelector)
        .Take(limit);
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

      IEnumerable<T> result = [.. source];
      Func<SortInfo, Func<T, object?>?> keySelectorFactory = (si) => GetOrCreatePropertySelector<T>(si.FieldName);
      foreach (SortInfo item in sortInfo)
      {
        Func<T, object?>? keySelector = keySelectorFactory(item);
        if (keySelector is null)
        {
          continue; // Property does not exist, skip this sort info
        }

        result = ApplyOrder(result, item, keySelector);
      }

      return result;
    }

    private static IEnumerable<T> ApplyOrder<T>(IEnumerable<T> source, SortInfo sortInfo, Func<T, object?> keySelector) =>
      sortInfo.Direction is System.ComponentModel.ListSortDirection.Ascending
        ? source.OrderBy(keySelector)
        : source.OrderByDescending(keySelector);

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
  }
}