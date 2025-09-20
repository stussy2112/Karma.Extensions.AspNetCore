// -----------------------------------------------------------------------
// <copyright file="AddFilterInfoMiddleware.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Karma.Extensions.AspNetCore.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Middleware
{
  /// <summary>
  /// Middleware that processes HTTP requests to extract filtering information from the query string and stores the
  /// parsed result in the request context.
  /// </summary>
  /// <remarks>This middleware parses filtering information from the query string of an HTTP request into a
  /// <see cref="FilterInfoCollection"/> object. The parsed result is stored in the  <see cref="HttpContext.Items"/>
  /// collection under the key <c>"FilterInfoCollection"</c>.  If the query string is empty or invalid, the stored value
  /// will be <see langword="null"/> or an  empty collection. After processing, the middleware invokes the next delegate
  /// in the request pipeline.</remarks>
  internal sealed class AddFilterInfoMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IParseStrategy<FilterInfoCollection> _parseStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFilterInfoMiddleware"/> class with the specified next middleware delegate.
    /// </summary>
    /// <param name="next">The next <see cref="RequestDelegate"/> in the HTTP request pipeline.</param>
    /// <param name="parseStrategy">The strategy used to parse filter information from the query string. If null, a default parser is used.</param>
    public AddFilterInfoMiddleware(RequestDelegate next, IParseStrategy<FilterInfoCollection>? parseStrategy = null) =>
      (_next, _parseStrategy) = (next, parseStrategy ?? new FilterQueryStringParser());

    /// <summary>
    /// Processes the HTTP request to parse filtering information from the query string and stores the result in the
    /// request context.
    /// </summary>
    /// <remarks>
    /// The method extracts filtering information from the query string of the HTTP request, parses it
    /// into a <see cref="FilterInfoCollection"/> object, and stores the result in the <see
    /// cref="HttpContext.Items"/> collection under the key <c>"FilterInfoCollection"</c>. If the query string is empty or invalid,
    /// the stored value will be <see langword="null"/> or an empty collection.
    /// </remarks>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task InvokeAsync(HttpContext context)
    {
      if (context is null)
      {
        return _next(context);
      }

      QueryString queryString = context.Request.QueryString;

      try
      {
        FilterInfoCollection? filterInfo = _parseStrategy.Parse(queryString.HasValue ? queryString.Value : string.Empty);
        context.Items[ContextItemKeys.Filters] = filterInfo;
      }
#pragma warning disable CA1031 // Do not catch general exception types
      // NOTE: Because the parser is injected, there's no idea what type of exception will be thrown
      catch
      {
        context.Items[ContextItemKeys.Filters] = new FilterInfoCollection("root");
      }
#pragma warning restore CA1031 // Do not catch general exception types

      return _next(context);
    }
  }
}