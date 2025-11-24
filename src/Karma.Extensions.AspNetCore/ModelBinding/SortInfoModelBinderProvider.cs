// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder for binding models of type <see cref="IEnumerable{SortInfo}"/>.
  /// </summary>
  /// <remarks>This provider is responsible for creating a <see cref="SortInfoModelBinder"/> when the model type
  /// is assignable to <see cref="IEnumerable{SortInfo}"/>. The binder uses a parsing strategy (<see
  /// cref="IParseStrategy{T}"/>) to convert input data into a collection of <see cref="SortInfo"/> objects.</remarks>
  public sealed class SortInfoModelBinderProvider : IModelBinderProvider
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);
      if (!context.Metadata.ModelType.IsAssignableTo(typeof(IEnumerable<SortInfo>)))
      {
        return null;
      }

      IParseStrategy<IEnumerable<SortInfo>> parser = context.Services.GetRequiredService<IParseStrategy<IEnumerable<SortInfo>>>();
      return new SortInfoModelBinder(parser);
    }
  }

  /// <summary>
  /// A model binder that binds a collection of <see cref="SortInfo"/> objects from a request's input data.
  /// </summary>
  /// <remarks>This binder uses a parsing strategy, provided via the constructor, to interpret the input data
  /// (e.g., query string, route data, or form data)  and convert it into a collection of <see cref="SortInfo"/>
  /// objects. If the input data is missing or cannot be parsed, the binding operation fails.</remarks>
  public sealed class SortInfoModelBinder : IModelBinder
  {
    private readonly IParseStrategy<IEnumerable<SortInfo>> _parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortInfoModelBinder"/> class with the specified parsing strategy.
    /// </summary>
    /// <param name="parser">
    /// An instance of <see cref="IParseStrategy{IEnumerable}"/> used to parse the input data into a collection of <see cref="SortInfo"/> objects.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parser"/> is <see langword="null"/>.</exception>
    public SortInfoModelBinder(IParseStrategy<IEnumerable<SortInfo>> parser) =>
      _parser = parser ?? throw new ArgumentNullException(nameof(parser));

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
      // Check if the binding context is null
      ArgumentNullException.ThrowIfNull(bindingContext);

      // Get the value from the request (e.g., from query string, route data, form data)
      string modelName = bindingContext.ModelName ?? bindingContext.FieldName;
      ValueProviderResult model = bindingContext.ValueProvider.GetValue(modelName);

      if (model == ValueProviderResult.None)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask; // No value found
      }

      string queryString = model.FirstValue ?? string.Empty;
      IEnumerable<SortInfo>? parsed = _parser.Parse(queryString);

      if (parsed is null)
      {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
      }

      IEnumerable? converted = parsed.ConvertEnumerable(bindingContext.ModelType);
      bindingContext.Result = ModelBindingResult.Success(converted);
      return Task.CompletedTask;
    }
  }
}
