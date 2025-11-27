// -----------------------------------------------------------------------
// <copyright file="CompleteKeyedQueryStringValueProviderFactory.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Factory for creating <see cref="CompleteKeyedQueryStringValueProvider"/> instances that handle
  /// complex query string aggregation for specific parameter keys.
  /// </summary>
  public sealed class CompleteKeyedQueryStringValueProviderFactory : IValueProviderFactory
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
}
