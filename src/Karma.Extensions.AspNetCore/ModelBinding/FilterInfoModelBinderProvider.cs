// -----------------------------------------------------------------------
// <copyright file="FilterInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
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
      if (!modelType.IsAssignableTo(typeof(FilterInfoCollection)) && !modelType.IsAssignableTo(typeof(IEnumerable<IFilterInfo>)))
      {
        return null;
      }

      IParseStrategy<FilterInfoCollection> parser = context.Services.GetRequiredService<IParseStrategy<FilterInfoCollection>>();
      return new FilterInfoModelBinder(parser);
    }
  }
}