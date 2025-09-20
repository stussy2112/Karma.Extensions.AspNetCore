// -----------------------------------------------------------------------
// <copyright file="AddSortInfoMiddleware.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Karma.Extensions.AspNetCore.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Middleware
{
  /// <summary>
  /// Middleware that extracts sorting information from the query string of an HTTP request and stores it in the request
  /// context.
  /// </summary>
  /// <remarks>This middleware parses sorting parameters from the query string of the incoming HTTP request and
  /// converts them into a collection of  <see cref="SortInfo"/> objects. The parsed sorting information is stored in
  /// the <see cref="HttpContext.Items"/> collection under the  key <c>"SortInfo"</c>. If the query string does not
  /// contain valid sorting parameters, the stored value will be <see langword="null"/>  or an empty collection. After
  /// processing, the middleware invokes the next middleware in the pipeline.</remarks>
  internal sealed class AddSortInfoMiddleware
  {
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddSortInfoMiddleware"/> class with the specified next middleware delegate.
    /// </summary>
    /// <param name="next">The next middleware in the HTTP request pipeline.</param>
    public AddSortInfoMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Processes the HTTP request to parse sorting information from the query string and stores the result in the
    /// request context.
    /// </summary>
    /// <remarks>The method extracts sorting information from the query string of the HTTP request, parses it
    /// into a collection of  <see cref="SortInfo"/> objects, and stores the result in the <see
    /// cref="HttpContext.Items"/> collection under the key  <c>"SortInfo"</c>. If the query string is empty or invalid,
    /// the stored value will be <see langword="null"/> or an empty collection.</remarks>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task InvokeAsync(HttpContext context)
    {
      if (context is not null)
      {
        QueryString query = context.Request.QueryString;

        var parser = new SortsQueryStringParser();
        IEnumerable<SortInfo>? sortInfo = parser.Parse(query.HasValue ? query.Value : string.Empty);
        context.Items[ContextItemKeys.SortInfo] = sortInfo;
      }

      return _next(context);
    }
  }
}