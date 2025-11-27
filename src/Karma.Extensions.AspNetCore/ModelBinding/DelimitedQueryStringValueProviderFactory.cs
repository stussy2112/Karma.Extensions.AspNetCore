// -----------------------------------------------------------------------
// <copyright file="DelimitedQueryStringValueProviderFactory.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// A factory for creating <see cref="DelimitedQueryStringValueProvider"/> instances, which parse query string values
  /// separated by a specified character.
  /// </summary>
  /// <remarks>This factory is designed to handle query string parameters where multiple values are encoded as a
  /// single string separated by a specific character (e.g., a comma). The resulting value provider splits the parameter
  /// value and makes the individual values available for model binding.</remarks>
  public sealed class DelimitedQueryStringValueProviderFactory : IValueProviderFactory
  {
    private readonly string _parameterKey;
    private readonly char _delimiter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelimitedQueryStringValueProviderFactory"/> class with the
    /// specified parameter key and delimiter.
    /// </summary>
    /// <param name="parameterKey">The key used to identify the query string parameter whose value will be processed.</param>
    /// <param name="delimiter">The character used to split the query string parameter value into multiple values. The default is a comma
    /// (<c>,</c>).</param>
    public DelimitedQueryStringValueProviderFactory(string parameterKey, char delimiter = ',')
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);
      _parameterKey = parameterKey;
      _delimiter = delimiter;
    }

    /// <inheritdoc />
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
      ArgumentNullException.ThrowIfNull(context);
      ArgumentNullException.ThrowIfNull(context.ActionContext);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext.Request);
      ArgumentNullException.ThrowIfNull(context.ActionContext.HttpContext.Request.Query);

      IQueryCollection queryCollection = context.ActionContext.HttpContext.Request.Query;
      var valueProvider = new DelimitedQueryStringValueProvider(queryCollection, _parameterKey, _delimiter);
      context.ValueProviders.Insert(0, valueProvider);
      return Task.CompletedTask;
    }
  }
}
