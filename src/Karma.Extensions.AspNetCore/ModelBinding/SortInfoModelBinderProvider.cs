// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

      return new SortInfoModelBinder();
    }
  }
}
