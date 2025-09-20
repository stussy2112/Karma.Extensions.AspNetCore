// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Karma.Extensions.AspNetCore;
using Karma.Extensions.AspNetCore.Middleware;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder
{
  /// <summary>
  /// Provides extension methods for configuring middleware to parse query string parameters related to filtering,
  /// paging, and sorting in an ASP.NET Core application.
  /// </summary>
  /// <remarks>These extension methods add middleware to the application's request pipeline to process query
  /// string parameters for filtering, paging, and sorting. The parsed information is made available for downstream
  /// middleware or components, enabling dynamic data manipulation based on client-specified query parameters.</remarks>
  public static class QueryStringInfoExtensions
  {
    /// <summary>
    /// Configures the application to parse query string filters using the specified parsing strategy.
    /// </summary>
    /// <remarks>This method adds middleware to the application's request pipeline that processes query string
    /// filters based on the provided parsing strategy. Use this method to enable dynamic filtering functionality in
    /// your application.</remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <param name="parseStrategy">The strategy used to parse query string filters into a <see cref="FilterInfoCollection"/>.  If <see
    /// langword="null"/>, the default parsing strategy will be used.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance, allowing for further configuration.</returns>
    public static IApplicationBuilder ParseQueryStringFilters(this IApplicationBuilder app, IParseStrategy<FilterInfoCollection>? parseStrategy = null) =>
      parseStrategy is not null
        ? app.UseMiddleware<AddFilterInfoMiddleware>(parseStrategy)
        : app.UseMiddleware<AddFilterInfoMiddleware>();

    /// <summary>
    /// Adds a custom model binder provider for binding <see cref="FilterInfoCollection"/> parameters to the specified
    /// <see cref="MvcOptions"/> instance.
    /// </summary>
    /// <remarks>This method inserts the <see cref="QueryStringInfoModelBinderProvider"/> at the
    /// beginning of the model binder providers list, ensuring it takes precedence over other providers when binding
    /// <see cref="FilterInfoCollection"/> parameters.</remarks>
    /// <param name="options">The <see cref="MvcOptions"/> instance to which the model binder provider will be added. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>The modified <see cref="MvcOptions"/> instance.</returns>
    public static MvcOptions AddFilterInfoParameterBinding(this MvcOptions options)
    {
      System.ArgumentNullException.ThrowIfNull(options);
      options.ModelBinderProviders.Insert(0, new QueryStringInfoModelBinderProvider());
      return options;
    }

    /// <summary>
    /// Configures the application to parse query string parameters for paging information.
    /// </summary>
    /// <remarks>This method adds middleware to the application's request pipeline that extracts paging
    /// information, such as page number and page size, from query string parameters. The extracted information can then
    /// be used by downstream components.</remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <returns>The configured <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder ParseQueryStringPagingInfo(this IApplicationBuilder app) =>
      app.UseMiddleware<AddPagingInfoMiddleware>();

    /// <summary>
    /// Configures the application to parse query string sorting information and make it available for downstream
    /// middleware or components.
    /// </summary>
    /// <remarks>This method adds middleware to the application's request pipeline that processes query string
    /// parameters related to sorting. The parsed sorting information is typically used to influence data ordering in
    /// subsequent processing.</remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to configure.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance, enabling method chaining.</returns>
    public static IApplicationBuilder ParseQueryStringSortingInfo(this IApplicationBuilder app) =>
      app.UseMiddleware<AddSortInfoMiddleware>();
  }
}