// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides model binding for the <see cref="PageInfo"/> type, supporting binding of pagination-related properties
  /// such as <c>After</c>, <c>Before</c>, <c>Offset</c>, and <c>Limit</c>.
  /// </summary>
  /// <remarks>This model binder attempts to bind the <see cref="PageInfo"/> properties from the incoming
  /// request data, such as query string parameters, route data, or form data. 
  /// <para>The <c>After</c> property supports a fallback binding to "Cursor" if the standard property name fails.</para>
  /// <para>The <c>After</c> and <c>Before</c> properties are bound as strings, while the <c>Offset</c> and 
  /// <c>Limit</c> properties are bound as unsigned integers.</para>
  /// <para>The binding succeeds if at least one property is successfully bound.</para></remarks>
  public sealed class PageInfoModelBinder : IModelBinder
  {
    private readonly IParseStrategy<PageInfo> _parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoModelBinder"/> class with the specified model binders.
    /// </summary>
    /// <param name="parser">The parsing strategy used to convert query string input into a <see cref="PageInfo"/> instance.</param>
    public PageInfoModelBinder(IParseStrategy<PageInfo> parser) =>
      _parser = parser ?? throw new ArgumentNullException(nameof(parser));

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
      // Check if the binding context is null
      ArgumentNullException.ThrowIfNull(bindingContext);

      // Get the model name or use the parser's parameter key as fallback
      string modelName = bindingContext.ModelName ?? _parser.ParameterKey;

      // Use the ValueProvider to get the aggregated query string
      ValueProviderResult result = bindingContext.ValueProvider.GetValue(modelName);

      if (result == ValueProviderResult.None || !_parser.TryParse(result.FirstValue ?? string.Empty, out PageInfo? parsed) || parsed is null)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask; // No value found
      }

      bindingContext.Result = ModelBindingResult.Success(parsed);
      return Task.CompletedTask;
    }
  }
}
