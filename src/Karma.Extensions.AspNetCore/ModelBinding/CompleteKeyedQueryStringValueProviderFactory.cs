// -----------------------------------------------------------------------
// <copyright file="CompleteKeyedQueryStringValueProviderFactory.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Factory for creating <see cref="CompleteKeyedQueryStringValueProvider"/> instances that handle
  /// complex query string aggregation for specific parameter keys.
  /// </summary>
  internal sealed class CompleteKeyedQueryStringValueProviderFactory : IValueProviderFactory
  {
    private readonly string _parameterKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteKeyedQueryStringValueProviderFactory"/> class with the
    /// specified parameter key.
    /// </summary>
    /// <param name="parameterKey">The key used to identify query string parameters. Must not be null, empty, or consist only of whitespace.</param>
    public CompleteKeyedQueryStringValueProviderFactory(string parameterKey)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);
      _parameterKey = parameterKey;
    }

    /// <inheritdoc />
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
      ArgumentNullException.ThrowIfNull(context);
      ArgumentNullException.ThrowIfNull(context.ActionContext);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext.Request);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext.Request.Query);

      var valueProvider = new CompleteKeyedQueryStringValueProvider(context.ActionContext.HttpContext.Request.Query, _parameterKey);

      context.ValueProviders.Insert(0, valueProvider);

      return Task.CompletedTask;
    }
  }

  /// <summary>
  /// Provides a value provider that can access and aggregate multiple related query string parameters
  /// into a single value for complex parsing scenarios like filter expressions.
  /// </summary>
  /// <remarks>
  /// This value provider is designed to handle complex query string patterns where multiple
  /// parameters with similar keys (e.g., filter[...], sort[...]) need to be aggregated
  /// and processed together by a single parser.
  /// </remarks>
  internal sealed class CompleteKeyedQueryStringValueProvider : QueryStringValueProvider
  {
    private readonly IQueryCollection _queryCollection;
    private readonly string _parameterKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteKeyedQueryStringValueProvider"/> class.
    /// </summary>
    /// <param name="queryCollection">The query collection from the HTTP request.</param>
    /// <param name="parameterKey">The parameter key prefix to match (e.g., "filter").</param>
    public CompleteKeyedQueryStringValueProvider(IQueryCollection queryCollection, string parameterKey)
      : base(BindingSource.Query, queryCollection, CultureInfo.InvariantCulture)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);
      _queryCollection = queryCollection ?? throw new ArgumentNullException(nameof(queryCollection));
      _parameterKey = parameterKey;
    }

    /// <inheritdoc />
    public override bool ContainsPrefix(string prefix) =>
      string.Equals(_parameterKey, prefix, StringComparison.Ordinal)
        && _queryCollection.Keys.Any((key) => key.StartsWith($"{_parameterKey}[", StringComparison.Ordinal));

    /// <inheritdoc />
    public override ValueProviderResult GetValue(string key)
    {
      ArgumentNullException.ThrowIfNull(key);

      if (!string.Equals(_parameterKey, key, StringComparison.Ordinal))
      {
        return ValueProviderResult.None;
      }

      // Aggregate all related query parameters
      string aggregatedQueryString = GetAggregatedQueryString();

      if (string.IsNullOrWhiteSpace(aggregatedQueryString))
      {
        return ValueProviderResult.None;
      }

      return new ValueProviderResult(aggregatedQueryString);
    }

    private static void AppendKeyValuePair(StringBuilder sb, KeyValuePair<string, StringValues> kvp)
    {
      foreach (StringValues value in kvp.Value)
      {
        if (sb.Length > 0)
        {
          _ = sb.Append('&');
        }

        _ = string.IsNullOrWhiteSpace(value)
          ? sb.Append(kvp.Key)
          : sb.AppendFormat("{0}={1}", kvp.Key, value);
      }
    }

    /// <summary>
    /// Aggregates all query parameters that match the parameter key pattern into a single query string.
    /// </summary>
    /// <returns>A query string containing all matching parameters.</returns>
    /// <example>
    /// For parameter key "filter" and query string "?filter[name]=john&amp;filter[age]=25&amp;other=value",
    /// returns "filter[name]=john&amp;filter[age]=25".
    /// </example>
    private string GetAggregatedQueryString()
    {
      int estimatedCapacity = _queryCollection.Count * 20;
      var sb = new StringBuilder(estimatedCapacity);

      foreach (KeyValuePair<string, StringValues> kvp in _queryCollection)
      {
        if (!IsMatchingParameter(kvp.Key))
        {
          continue;
        }

        AppendKeyValuePair(sb, kvp);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Determines if a query parameter key matches the pattern for this value provider.
    /// </summary>
    /// <param name="key">The query parameter key to check.</param>
    /// <returns>True if the key matches the pattern; otherwise, false.</returns>
    private bool IsMatchingParameter(string key) =>
      !string.IsNullOrWhiteSpace(key)
      && (string.Equals(key, _parameterKey, StringComparison.Ordinal)
        || key.StartsWith($"{_parameterKey}[", StringComparison.Ordinal));
  }
}
