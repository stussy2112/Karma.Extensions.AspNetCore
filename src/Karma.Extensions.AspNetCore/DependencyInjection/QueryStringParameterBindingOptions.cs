// -----------------------------------------------------------------------
// <copyright file="QueryStringParameterBindingOptions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Karma.Extensions.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
  /// <summary>
  /// Provides configuration options for binding query string parameters to filtering, paging, and sorting models in API
  /// requests.
  /// </summary>
  /// <remarks>Use this class to customize how query string parameters are parsed and mapped to filter, page,
  /// and sort information when handling API requests. Each set of options allows you to specify parsing strategies and
  /// parameter keys for the corresponding query string segment. This is typically used in model binding scenarios to
  /// control how client-supplied query parameters are interpreted.</remarks>
  public class QueryStringParameterBindingOptions
  {
    /// <summary>
    /// Gets the configuration options for binding filter-related query string parameters.
    /// </summary>
    /// <value>
    /// An instance of <see cref="FilterInfoBindingOptions"/> containing the pattern provider and parameter key
    /// for filter binding.
    /// </value>
    public FilterInfoBindingOptions FilterBindingOptions { get; } = new();

    /// <summary>
    /// Gets the configuration options for binding page-related query string parameters.
    /// </summary>
    /// <value>
    /// An instance of <see cref="PageInfoBindingOptions"/> containing the parameter key and parsing strategy
    /// for page binding.
    /// </value>
    public PageInfoBindingOptions PageBindingOptions { get; } = new();

    /// <summary>
    /// Gets the configuration options for binding sort-related query string parameters.
    /// </summary>
    /// <value>
    /// An instance of <see cref="SortInfoBindingOptions"/> containing the parameter key and parsing strategy
    /// for sort binding.
    /// </value>
    public SortInfoBindingOptions SortBindingOptions { get; } = new();

    /// <summary>
    /// Provides configuration options for binding filter information from query string parameters.
    /// </summary>
    /// <remarks>
    /// Use this class to customize the pattern provider and parameter key used to extract filter information
    /// from query strings.
    /// </remarks>
    public class FilterInfoBindingOptions
    {
      /// <summary>
      /// Gets or sets the pattern provider used to parse filter expressions from query strings.
      /// </summary>
      /// <value>
      /// A <see cref="FilterPatternProvider"/> instance that defines the regular expression patterns and
      /// group names for parsing filter components. Defaults to <see cref="FilterPatternProvider.Default"/>.
      /// </value>
      public FilterPatternProvider PatternProvider { get; set; } = FilterPatternProvider.Default;

      /// <summary>
      /// Gets or sets the query string parameter key used to identify filter parameters.
      /// </summary>
      /// <value>
      /// A string representing the parameter key for filter parameters in the query string.
      /// Defaults to <see cref="QueryParameterNames.Filter"/>.
      /// </value>
      public string? ParameterKey { get; set; } = QueryParameterNames.Filter;
    }

    /// <summary>
    /// Provides configuration options for binding sort information from query string parameters.
    /// </summary>
    /// <remarks>
    /// Use this class to customize the parameter key and parsing strategy used to extract sort information
    /// from query strings.
    /// </remarks>
    public class SortInfoBindingOptions
    {
      /// <summary>
      /// Gets or sets the query string parameter key used to identify sort parameters.
      /// </summary>
      /// <value>
      /// A string representing the parameter key for sort parameters in the query string.
      /// Defaults to <see cref="QueryParameterNames.Sort"/>.
      /// </value>
      public string? ParameterKey { get; set; } = QueryParameterNames.Sort;

      /// <summary>
      /// Gets or sets the parsing strategy used to convert query string values into sort information.
      /// </summary>
      /// <value>
      /// An <see cref="IParseStrategy{T}"/> implementation that parses query string values into a collection
      /// of <see cref="SortInfo"/> instances. Defaults to a new instance of <see cref="SortsQueryStringParser"/>.
      /// </value>
      public IParseStrategy<IEnumerable<SortInfo>>? ParseStrategy { get; set; } = new SortsQueryStringParser();
    }

    /// <summary>
    /// Provides configuration options for binding page information from query string parameters.
    /// </summary>
    /// <remarks>
    /// Use this class to customize the parameter key and parsing strategy used to extract pagination information
    /// from query strings, including offset, limit, and cursor-based pagination parameters.
    /// </remarks>
    public class PageInfoBindingOptions
    {
      /// <summary>
      /// Gets or sets the query string parameter key used to identify pagination parameters.
      /// </summary>
      /// <value>
      /// A string representing the parameter key for pagination parameters in the query string.
      /// Defaults to <see cref="QueryParameterNames.Page"/>.
      /// </value>
      public string? ParameterKey { get; set; } = QueryParameterNames.Page;

      /// <summary>
      /// Gets or sets the parsing strategy used to convert query string values into page information.
      /// </summary>
      /// <value>
      /// An <see cref="IParseStrategy{T}"/> implementation that parses query string values into a
      /// <see cref="PageInfo"/> instance containing pagination details such as offset, limit, after, and before cursors.
      /// Defaults to a new instance of <see cref="PageInfoQueryStringParser"/>.
      /// </value>
      public IParseStrategy<PageInfo>? ParsingStrategy { get; set; } = new PageInfoQueryStringParser();
    }
  }
}