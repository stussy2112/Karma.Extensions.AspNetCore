// -----------------------------------------------------------------------
// <copyright file="QueryStringParserModelBinderProviderBase.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Provides a generic model binder provider for types that can be parsed from query string parameters.
  /// </summary>
  /// <typeparam name="T">The type of the model being bound.</typeparam>
  /// <typeparam name="TBinder">The type of the model binder.</typeparam>
  public sealed class QueryStringParserModelBinderProvider<TBinder, T> : IModelBinderProvider
    where TBinder : QueryStringParserModelBinderBase<T>
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);

      if (!context.Metadata.ModelType.IsAssignableTo(typeof(T)))
      {
        return null;
      }

      IParseStrategy<T> parser = context.Services.GetRequiredService<IParseStrategy<T>>();
      var binder = Activator.CreateInstance(typeof(TBinder), parser) as TBinder;
      return binder;
    }
  }
}
