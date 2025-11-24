// -----------------------------------------------------------------------
// <copyright file="FilterInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder for types related to filtering, such as <see cref="FilterInfoCollection"/> or <see
  /// cref="IEnumerable{IFilterInfo}"/>.
  /// </summary>
  /// <remarks>This provider returns a <see cref="FilterInfoModelBinder"/> for models of type <see
  /// cref="FilterInfoCollection"/> or <see cref="IEnumerable{IFilterInfo}"/>. For other types, it returns <see
  /// langword="null"/>.</remarks>
  public sealed class FilterInfoModelBinderProvider : IModelBinderProvider
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);

      Type modelType = context.Metadata.ModelType;
      if (modelType.IsAssignableTo(typeof(FilterInfoCollection)) || modelType.IsAssignableTo(typeof(IEnumerable<IFilterInfo>)))
      {
        IParseStrategy<FilterInfoCollection> parser = context.Services.GetRequiredService<IParseStrategy<FilterInfoCollection>>();
        return new FilterInfoModelBinder(parser);
      }

      return null;
    }
  }

  /// <summary>
  /// Provides model binding for <see cref="FilterInfoCollection"/> objects by parsing query string parameters.
  /// </summary>
  /// <remarks>This model binder uses a custom parsing strategy, provided via the <see cref="IParseStrategy{T}"/> interface,
  /// to convert aggregated query string parameters into a <see cref="FilterInfoCollection"/> instance.
  /// If the query string is empty or cannot be parsed, the binding operation will fail.</remarks>
  public class FilterInfoModelBinder : IModelBinder
  {
    private readonly IParseStrategy<FilterInfoCollection> _parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfoModelBinder"/> class with the specified parsing strategy.
    /// </summary>
    /// <param name="parser">
    /// An instance of <see cref="IParseStrategy{FilterInfoCollection}"/> used to parse the filter query string into a <see cref="FilterInfoCollection"/>.
    /// </param>
    public FilterInfoModelBinder(IParseStrategy<FilterInfoCollection> parser)
    {
      ArgumentNullException.ThrowIfNull(parser);
      _parser = parser;
    }

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
      ArgumentNullException.ThrowIfNull(bindingContext);

      // Use the ValueProvider to get the aggregated filter query string
      string filterQueryString = GetFilterQueryStringFromValueProvider(bindingContext);

      if (string.IsNullOrWhiteSpace(filterQueryString))
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
      }

      if (!_parser.TryParse(filterQueryString, out FilterInfoCollection? parsed) || parsed is null)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
      }

      bindingContext.Result = ModelBindingResult.Success(parsed);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the aggregated filter query string from the ValueProvider.
    /// This method leverages the registered ValueProvider to handle complex query string aggregation.
    /// </summary>
    /// <param name="bindingContext">The model binding context.</param>
    /// <returns>A string containing all aggregated filter query parameters, or empty string if none found.</returns>
    private string GetFilterQueryStringFromValueProvider(ModelBindingContext bindingContext)
    {
      // Get the model name or use the parser's parameter key as fallback
      string modelName = bindingContext.ModelName ?? _parser.ParameterKey;

      // Use the ValueProvider to get the aggregated query string
      ValueProviderResult result = bindingContext.ValueProvider.GetValue(modelName);

      return result != ValueProviderResult.None ? result.FirstValue ?? string.Empty : string.Empty;
    }
  }
}