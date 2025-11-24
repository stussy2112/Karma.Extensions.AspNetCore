// -----------------------------------------------------------------------
// <copyright file="MvcBuilderQueryStringInfoExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Karma.Extensions.AspNetCore;
using Karma.Extensions.AspNetCore.ModelBinding;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder
{
  /// <summary>
  /// Provides extension methods for configuring middleware to parse query string parameters related to filtering,
  /// paging, and sorting in an ASP.NET Core application.
  /// </summary>
  /// <remarks>These extension methods add middleware to the application's request pipeline to process query
  /// string parameters for filtering, paging, and sorting. The parsed information is made available for downstream
  /// middleware or components, enabling dynamic data manipulation based on client-specified query parameters.</remarks>
  public static class MvcBuilderQueryStringInfoExtensions
  {
    /// <summary>
    /// Configures the MVC builder to enable parameter binding for filter information from query string parameters.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="parameterKey">The query string parameter key used to identify filter information. If not specified,
    /// the default key from <see cref="QueryParameterNames.Filter"/> will be used.</param>
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddFilterInfoParameterBinding(this IMvcBuilder builder, string? parameterKey = QueryParameterNames.Filter) =>
      builder.AddFilterInfoParameterBinding(new FilterQueryStringParser(FilterPatternProvider.Default), parameterKey);

    /// <summary>
    /// Configures the MVC builder to enable parameter binding for filter information from query string parameters.
    /// </summary>
    /// <remarks>This method adds the necessary services and configuration to enable automatic binding of filter
    /// information from query string parameters to action method parameters. The filter information can be used to
    /// dynamically filter data based on client-specified filtering criteria.</remarks>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="parameterKey">The query string parameter key used to identify filter information. If not specified,
    /// the default key from <see cref="QueryParameterNames.Filter"/> will be used.</param>
    /// <param name="filterPatternProvider"></param>
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddFilterInfoParameterBinding(this IMvcBuilder builder, FilterPatternProvider? filterPatternProvider, string? parameterKey = QueryParameterNames.Filter) =>
      builder.AddFilterInfoParameterBinding(new FilterQueryStringParser(filterPatternProvider), parameterKey);

    /// <summary>
    /// Configures MVC to support binding of filter information from query string parameters using a custom or default
    /// parsing strategy.
    /// </summary>
    /// <remarks>This method adds the necessary value provider and model binder to enable automatic binding of
    /// filter information from query string parameters. It should be called during MVC setup in the application's
    /// service configuration.</remarks>
    /// <param name="builder">The MVC builder used to configure services and options for the application. Cannot be null.</param>
    /// <param name="parseStrategy">An optional parsing strategy for converting query string values into a collection of filter information. If
    /// null, a default parser is used.</param>
    /// <param name="parameterKey">The query string key to use for filter information. Cannot be null or whitespace. Defaults to the standard
    /// filter parameter name.</param>
    /// <returns>The same MVC builder instance, configured to support filter information parameter binding.</returns>
    public static IMvcBuilder AddFilterInfoParameterBinding(this IMvcBuilder builder, IParseStrategy<FilterInfoCollection>? parseStrategy, string? parameterKey = QueryParameterNames.Filter)
    {
      ArgumentNullException.ThrowIfNull(builder);
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

      builder.Services.TryAddSingleton(parseStrategy ?? new FilterQueryStringParser());

      return builder.AddMvcOptions((o) =>
      {
        o.ValueProviderFactories.Insert(0, new CompleteKeyedQueryStringValueProviderFactory(parameterKey));
        o.ModelBinderProviders.Insert(0, new FilterInfoModelBinderProvider());
      });
    }

    /// <summary>
    /// Configures the MVC builder to enable parameter binding for paging information from query string parameters.
    /// </summary>
    /// <remarks>This method adds the necessary services and configuration to enable automatic binding of paging
    /// information from query string parameters to action method parameters. The paging information can be used to
    /// dynamically control data pagination based on client-specified paging criteria.</remarks>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="parseStrategy"></param>
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddPagingInfoParameterBinding(this IMvcBuilder builder, IParseStrategy<PageInfo>? parseStrategy = null)
    {
      ArgumentNullException.ThrowIfNull(builder);
      builder.Services.TryAddSingleton(parseStrategy ?? new PageInfoQueryStringParser());
      return builder.AddMvcOptions((o) => o.ModelBinderProviders.Insert(0, new PageInfoModelBinderProvider()));
    }

    /// <summary>
    /// Configures the MVC builder to enable parameter binding for sort information from query string parameters.
    /// </summary>
    /// <remarks>This method adds the necessary services and configuration to enable automatic binding of sort
    /// information from query string parameters to action method parameters. The sort information can be used to
    /// dynamically order data based on client-specified sorting criteria.</remarks>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="parameterKey">The query string parameter key used to identify sort information. If not specified,
    /// the default key from <see cref="QueryParameterNames.Sort"/> will be used.</param>
    /// <param name="sortInfoParseStrategy"></param>
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddSortInfoParameterBinding(this IMvcBuilder builder, string? parameterKey = QueryParameterNames.Sort, IParseStrategy<IEnumerable<SortInfo>>? sortInfoParseStrategy = null)
    {
      ArgumentNullException.ThrowIfNull(builder);
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

      builder.Services.TryAddSingleton(sortInfoParseStrategy ?? new SortsQueryStringParser());

      return builder.AddMvcOptions((o) =>
      {
        o.ValueProviderFactories.Insert(0, new DelimitedQueryStringValueProviderFactory(parameterKey));
        o.ModelBinderProviders.Insert(0, new SortInfoModelBinderProvider());
      });
    }

    /// <summary>
    /// Configures MVC to support binding filter, paging, and sorting information from query string parameters using
    /// customizable options.
    /// </summary>
    /// <remarks>This method enables automatic binding of filter, paging, and sorting parameters from the
    /// query string to controller action parameters. Use the <paramref name="configureOptions"/> delegate to customize
    /// binding behavior, such as parameter names and parsing strategies.</remarks>
    /// <param name="builder">The MVC builder to configure. Cannot be null.</param>
    /// <param name="configureOptions">An optional delegate to configure query string parameter binding options. If null, default options are used.</param>
    /// <returns>The same MVC builder instance, configured to support query string parameter bindings for filter, paging, and
    /// sorting information.</returns>
    public static IMvcBuilder AddQueryStringInfoParameterBinding(this IMvcBuilder builder, Action<QueryStringParameterBindingOptions>? configureOptions = null)
    {
      ArgumentNullException.ThrowIfNull(builder);

      var options = new QueryStringParameterBindingOptions();
      configureOptions?.Invoke(options);

      return builder.AddFilterInfoParameterBinding(options.FilterBindingOptions.PatternProvider, options.FilterBindingOptions.ParameterKey)
        .AddPagingInfoParameterBinding(options.PageBindingOptions.ParsingStrategy)
        .AddSortInfoParameterBinding(options.SortBindingOptions.ParameterKey, options.SortBindingOptions.ParseStrategy);
    }
  }
}