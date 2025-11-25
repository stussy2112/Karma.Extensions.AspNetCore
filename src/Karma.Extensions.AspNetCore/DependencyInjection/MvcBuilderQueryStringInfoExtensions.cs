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
    /// Configures the MVC pipeline to support binding paging information from query string parameters using the
    /// specified parameter key.
    /// </summary>
    /// <param name="builder">The MVC builder used to configure MVC services and options.</param>
    /// <param name="parameterKey">The query string key to use for paging information. If not specified, defaults to the standard page parameter
    /// name.</param>
    /// <returns>The same <see cref="IMvcBuilder"/> instance, enabling further MVC configuration.</returns>
    public static IMvcBuilder AddPagingInfoParameterBinding(this IMvcBuilder builder, string? parameterKey = QueryParameterNames.Page) =>
      builder.AddPagingInfoParameterBinding(new PageInfoQueryStringParser(), parameterKey);

    /// <summary>
    /// Configures the MVC builder to enable parameter binding for paging information from query string parameters
    /// using a custom pattern provider.
    /// </summary>
    /// <remarks>This method adds the necessary services and configuration to enable automatic binding of paging
    /// information from query string parameters to action method parameters using a custom <see cref="PageInfoPatternProvider"/>.
    /// The paging information can be used to implement pagination, including offset-based and cursor-based pagination.</remarks>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="pageInfoPatternProvider">The pattern provider that defines the regular expression patterns for parsing page information from
    /// query strings.</param>
    /// <param name="parameterKey">The query string parameter key used to identify paging information. If not specified,
    /// the default key from <see cref="QueryParameterNames.Page"/> will be used.</param>
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddPagingInfoParameterBinding(this IMvcBuilder builder, PageInfoPatternProvider pageInfoPatternProvider, string? parameterKey = QueryParameterNames.Page) =>
      builder.AddPagingInfoParameterBinding(new PageInfoQueryStringParser(pageInfoPatternProvider), parameterKey);

    /// <summary>
    /// Configures the MVC builder to enable parameter binding for sort information from query string parameters.
    /// </summary>
    /// <remarks>This method adds the necessary services and configuration to enable automatic binding of sort
    /// information from query string parameters to action method parameters. The sort information can be used to
    /// dynamically order data based on client-specified sorting criteria.</remarks>
    /// <param name="builder">The <see cref="IMvcBuilder"/> instance to configure.</param>
    /// <param name="parameterKey">The query string parameter key used to identify sort information. If not specified,
    /// the default key from <see cref="QueryParameterNames.Sort"/> will be used.</param>
    /// 
    /// <returns>The <see cref="IMvcBuilder"/> instance, enabling method chaining for additional configuration.</returns>
    public static IMvcBuilder AddSortInfoParameterBinding(this IMvcBuilder builder, string? parameterKey = QueryParameterNames.Sort)
    {
      ArgumentNullException.ThrowIfNull(builder);
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

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
        .AddPagingInfoParameterBinding(options.PageBindingOptions.PatternProvider, options.PageBindingOptions.ParameterKey)
        .AddSortInfoParameterBinding(options.SortBindingOptions.ParameterKey);
    }

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
    private static IMvcBuilder AddFilterInfoParameterBinding(this IMvcBuilder builder, IParseStrategy<FilterInfoCollection>? parseStrategy, string? parameterKey = QueryParameterNames.Filter)
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
    /// Configures MVC to support binding of paging information parameters using a custom or default parsing strategy.
    /// </summary>
    /// <remarks>This method registers the necessary value provider and model binder to enable automatic
    /// binding of paging information to action parameters. It should be called during MVC service configuration in the
    /// application's startup.</remarks>
    /// <param name="builder">The MVC builder used to configure MVC services and options.</param>
    /// <param name="parseStrategy">An optional strategy for parsing paging information from incoming requests. If null, a default query string
    /// parser is used.</param>
    /// <param name="parameterKey">The query parameter key used to identify paging information in requests. If null, the default key is used.</param>
    /// <returns>The same MVC builder instance, enabling further configuration.</returns>
    private static IMvcBuilder AddPagingInfoParameterBinding(this IMvcBuilder builder, IParseStrategy<PageInfo>? parseStrategy, string? parameterKey = QueryParameterNames.Page)
    {
      ArgumentNullException.ThrowIfNull(builder);
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

      builder.Services.TryAddSingleton(parseStrategy ?? new PageInfoQueryStringParser());

      return builder.AddMvcOptions((o) =>
      {
        o.ValueProviderFactories.Insert(0, new CompleteKeyedQueryStringValueProviderFactory(parameterKey));
        o.ModelBinderProviders.Insert(0, new PageInfoModelBinderProvider());
      });
    }
  }
}