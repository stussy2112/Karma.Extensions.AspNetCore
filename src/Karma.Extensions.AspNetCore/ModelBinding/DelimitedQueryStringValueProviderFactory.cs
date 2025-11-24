// -----------------------------------------------------------------------
// <copyright file="DelimitedQueryStringValueProviderFactory.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// A factory for creating <see cref="DelimitedQueryStringValueProvider"/> instances, which parse query string values
  /// separated by a specified character.
  /// </summary>
  /// <remarks>This factory is designed to handle query string parameters where multiple values are encoded as a
  /// single string separated by a specific character (e.g., a comma). The resulting value provider splits the parameter
  /// value and makes the individual values available for model binding.</remarks>
  public class DelimitedQueryStringValueProviderFactory : IValueProviderFactory
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

  /// <summary>
  /// Provides values from a query string parameter, splitting the parameter's value into multiple entries based on a
  /// specified separator.
  /// </summary>
  /// <remarks>This value provider is useful when a query string parameter contains multiple values separated by
  /// a specific character (e.g., a comma). It retrieves the parameter's value, splits it using the defined separator,
  /// and makes the resulting entries available as individual values.</remarks>
  public class DelimitedQueryStringValueProvider : QueryStringValueProvider
  {
    private readonly string? _parameterKey;
    private readonly char _delimiter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelimitedQueryStringValueProvider"/> class,  which provides values
    /// from a query string parameter, splitting the parameter's value  into multiple entries based on a specified
    /// separator.
    /// </summary>
    /// <param name="values">The collection of query string values to retrieve data from.</param>
    /// <param name="parameterKey">The key of the query string parameter whose value will be split and provided.</param>
    /// <param name="delimiter">The character used to separate the values within the query string parameter. Defaults to ','.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is <see langword="null"/>.</exception>
    public DelimitedQueryStringValueProvider(IQueryCollection values, string parameterKey, char delimiter = ',')
      : base(BindingSource.Query, values, System.Globalization.CultureInfo.InvariantCulture)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);
      _parameterKey = parameterKey;
      _delimiter = delimiter;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key from the value provider,  splitting the value into
    /// multiple entries if it contains the defined separator.
    /// </summary>
    /// <remarks>If the key does not match the parameter key, the method returns the result from the base
    /// implementation. If the value contains the separator, it is split into multiple entries using the
    /// separator.</remarks>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>A <see cref="ValueProviderResult"/> containing the value(s) associated with the specified key.  If the key
    /// matches the parameter key and the value contains the separator, the value is split  into multiple entries.
    /// Returns <see cref="ValueProviderResult.None"/> if no value is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <see langword="null"/>.</exception>
    public override ValueProviderResult GetValue(string key)
    {
      ValueProviderResult result = base.GetValue(key);
  
      if (!string.Equals(_parameterKey, key, StringComparison.Ordinal))
      {
        return ValueProviderResult.None;
      }
  
      if (!IsParsable(result, _delimiter))
      {
        return result;
      }
  
      var splitValues = new StringValues([.. result.Values.SelectMany((val) => val?.Split(_delimiter, StringSplitOptions.None) ?? [])]);
      return new ValueProviderResult(splitValues, result.Culture);
    }

    private static bool IsParsable(ValueProviderResult result, char delimiter) =>
      result != ValueProviderResult.None
      && result.Values.Any((val) => !string.IsNullOrWhiteSpace(val) && val.Contains(delimiter, StringComparison.Ordinal));
  }
}
