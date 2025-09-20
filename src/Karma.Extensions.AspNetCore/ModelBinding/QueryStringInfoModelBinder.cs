// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder that binds query string values to a model using a specified parsing strategy.
  /// </summary>
  /// <remarks>
  /// This model binder retrieves a value from the query string (or other value providers) based on the
  /// model name and attempts to parse it into the specified model type using the provided <typeparamref name="TParser"/>.
  /// If parsing succeeds, the model binding result is set to success with the parsed value. If no value is found or
  /// parsing fails, the binding result is set to failure.
  ///
  /// The binder follows this workflow:
  /// 1. Validates the binding context is not null
  /// 2. Retrieves the value from the value provider using the model name
  /// 3. Attempts to parse the retrieved value using the configured parser
  /// 4. Sets the binding result to success with the parsed value, or failure if parsing fails
  /// </remarks>
  /// <typeparam name="TParser">The type of the parser used to parse the query string value.</typeparam>
  /// <typeparam name="TParsed">The type of the parsed model.</typeparam>
  /// <param name="parser">The parser strategy used to parse query string values into the target type.</param>
  internal class QueryStringInfoModelBinder<TParser, TParsed>(TParser parser) : IModelBinder
    where TParser : IParseStrategy<TParsed>
  {
    private readonly TParser _parser = parser ?? throw new ArgumentNullException(nameof(parser));

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
      // Check if the binding context is null
      ArgumentNullException.ThrowIfNull(bindingContext);

      // Get the value from the request (e.g., from query string, route data, form data)
      string modelName = bindingContext.ModelName;
      ValueProviderResult model = bindingContext.ValueProvider.GetValue(modelName);

      if (model == ValueProviderResult.None)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask; // No value found
      }

      string filter = model.FirstValue ?? string.Empty;

      if (!_parser.TryParse(filter, out TParsed? parsed) || parsed is null)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
      }

      bindingContext.Result = ModelBindingResult.Success(parsed);
      return Task.CompletedTask;
    }
  }
}
