// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoModelBinderProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Karma.Extensions.AspNetCore.ModelBinding
{
  /// <summary>
  /// Provides a model binder for handling query string-based model binding for specific types.
  /// </summary>
  /// <remarks>This provider supports binding models from query string parameters for the following types: <list
  /// type="bullet"> <item><description><see cref="FilterInfoCollection"/></description></item> <item><description>Any
  /// type assignable to <see cref="IEnumerable{T}"/> where <c>T</c> is <see cref="SortInfo"/></description></item>
  /// <item><description><see cref="PageInfo"/></description></item> </list> If the model type is not one of the
  /// supported types, this provider returns <see langword="null"/>.</remarks>
  internal class QueryStringInfoModelBinderProvider : IModelBinderProvider
  {
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
      ArgumentNullException.ThrowIfNull(context);
      
      Type? binderType = context.Metadata.ModelType switch
      {
        Type t when t == typeof(FilterInfoCollection) => typeof(QueryStringInfoModelBinder<FilterQueryStringParser, FilterInfoCollection>),
        Type t when t.IsAssignableTo(typeof(IEnumerable<SortInfo>)) => typeof(QueryStringInfoModelBinder<SortsQueryStringParser, IEnumerable<SortInfo>>),
        Type t when t == typeof(PageInfo) => typeof(QueryStringInfoModelBinder<PageInfoQueryStringParser, PageInfo>),
        _ => null
      };

      return binderType is not null ? new BinderTypeModelBinder(binderType) : null; 
    }
  }
}
