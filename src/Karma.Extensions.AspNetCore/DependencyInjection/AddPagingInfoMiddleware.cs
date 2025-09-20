// -----------------------------------------------------------------------
// <copyright file="AddPagingInfoMiddleware.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Karma.Extensions.AspNetCore.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Middleware
{
  /// <summary>
  /// Middleware that extracts pagination information from the query string of an HTTP request and adds it to the
  /// request's context items.
  /// </summary>
  /// <remarks>This middleware parses pagination details from the query string using a
  /// <c>PageInfoQueryStringParser</c> and stores the resulting <c>PageInfo</c> object in the <see
  /// cref="HttpContext.Items"/> collection under the key <c>"PageInfo"</c>. If the query string does not contain valid
  /// pagination information, <c>null</c> is stored instead. This allows downstream middleware or request handlers to
  /// access pagination details without needing to re-parse the query string.</remarks>
  internal sealed class AddPagingInfoMiddleware
  {
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPagingInfoMiddleware"/> class with the specified next middleware delegate.
    /// </summary>
    /// <param name="next">The next middleware component in the HTTP request pipeline.</param>
    public AddPagingInfoMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Processes the current HTTP request by parsing pagination information from the query string and adding it to the
    /// request's context items.
    /// </summary>
    /// <remarks>The method extracts pagination details from the query string using a
    /// <c>PageInfoQueryStringParser</c> and stores the resulting <c>PageInfo</c> object in the <see
    /// cref="HttpContext.Items"/> collection under the key "PageInfo". If the query string does not contain pagination
    /// information, <c>null</c> is stored instead.</remarks>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task is completed when the next middleware
    /// in the pipeline has finished executing.</returns>
    public Task InvokeAsync(HttpContext context)
    {
      if (context is not null)
      {
        QueryString query = context.Request.QueryString;
        var parser = new PageInfoQueryStringParser();
        PageInfo? pageInfo = parser.Parse(query.HasValue ? query.Value : string.Empty);
        context.Items[ContextItemKeys.PageInfo] = pageInfo;
      }

      return _next(context);
    }
  }
}