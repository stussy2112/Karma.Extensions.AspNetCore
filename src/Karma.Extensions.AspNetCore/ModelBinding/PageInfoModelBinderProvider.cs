// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder for the <see cref="PageInfo"/> type.
  /// </summary>
  /// <remarks>This provider determines whether the requested model type is assignable to <see
  /// cref="PageInfo"/>. If so, it returns a <see cref="PageInfoModelBinder"/> instance configured with binders for
  /// string and unsigned integer types. Otherwise, it returns <see langword="null"/>.</remarks>
  public sealed class PageInfoModelBinderProvider : IModelBinderProvider
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);

      if (!context.Metadata.ModelType.IsAssignableTo(typeof(PageInfo)))
      {
        return null;
      }

      IParseStrategy<PageInfo> parser = context.Services.GetRequiredService<IParseStrategy<PageInfo>>();
      return new PageInfoModelBinder(parser);
    }
  }
}
