// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// A model binder that binds a collection of <see cref="SortInfo"/> objects from a request's input data.
  /// </summary>
  /// <remarks>This binder uses a parsing strategy, provided via the constructor, to interpret the input data
  /// (e.g., query string, route data, or form data)  and convert it into a collection of <see cref="SortInfo"/>
  /// objects. If the input data is missing or cannot be parsed, the binding operation fails.</remarks>
  public sealed class SortInfoModelBinder : IModelBinder
  {
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

      IEnumerable<SortInfo>? parsed = Parse(model.Values);

      IEnumerable? converted = parsed.ConvertEnumerable(bindingContext.ModelType);
      bindingContext.Result = ModelBindingResult.Success(converted);
      return Task.CompletedTask;
    }

    private static IEnumerable<SortInfo> Parse(StringValues values)
    {
      if (string.IsNullOrWhiteSpace(values))
      {
        yield break;
      }

      HashSet<string> seen = new(StringComparer.Ordinal);
      foreach (string? item in values)
      {
        if (string.IsNullOrWhiteSpace(item))
        {
          continue;
        }

        string[] split = item.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string field in split)
        {
          SortInfo? sortInfo;
          try
          {
            // TRICKY: The `SortInfo` constructor will validate the field name ...
            sortInfo = field; // Implicit conversion from string to SortInfo
          }
          catch (ArgumentException)
          {
            // ... and throw an `ArgumentException` if invalid
            // Skip invalid field names (e.g., just "-" or empty after trimming "-")
            continue;
          }

          // Only yield if successfully created and not already seen
          if (seen.Add(sortInfo.FieldName))
          {
            yield return sortInfo;
          }
        }
      }
    }
  }
}
