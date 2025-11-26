// -----------------------------------------------------------------------
// <copyright file="QueryStringParserModelBinderBase.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Provides a base class for model binders that parse query string parameters into a specific type.
  /// </summary>
  /// <typeparam name="T">The type of the model being bound.</typeparam>
  public abstract class QueryStringParserModelBinderBase<T> : IModelBinder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringParserModelBinderBase{T}"/> class.
    /// </summary>
    /// <param name="parser">The parsing strategy used to convert query string input into a <typeparamref name="T"/> instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parser"/> is <see langword="null"/>.</exception>
    protected QueryStringParserModelBinderBase(IParseStrategy<T> parser) =>
      Parser = parser ?? throw new ArgumentNullException(nameof(parser));

    /// <summary>
    /// Gets the parsing strategy used to convert query string input into a <typeparamref name="T"/> instance.
    /// </summary>
    protected IParseStrategy<T> Parser
    {
      get;
    }

    /// <inheritdoc />
    public virtual Task BindModelAsync(ModelBindingContext bindingContext)
    {
      // Check if the binding context is null
      ArgumentNullException.ThrowIfNull(bindingContext);

      // Get the model name or use the parser's parameter key as fallback
      string modelName = bindingContext.ModelName ?? Parser.ParameterKey;

      // Use the ValueProvider to get the aggregated query string
      ValueProviderResult result = bindingContext.ValueProvider.GetValue(modelName);

      if (result == ValueProviderResult.None || !Parser.TryParse(result.FirstValue ?? string.Empty, out T? parsed) || parsed is null)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask; // No value found
      }

      bindingContext.Result = ModelBindingResult.Success(parsed);
      return Task.CompletedTask;
    }
  }
}
