// -----------------------------------------------------------------------
// <copyright file="FilterInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Provides model binding for <see cref="FilterInfoCollection"/> objects by parsing query string parameters.
  /// </summary>
  /// <remarks>This model binder uses a custom parsing strategy, provided via the <see cref="IParseStrategy{T}"/> interface,
  /// to convert aggregated query string parameters into a <see cref="FilterInfoCollection"/> instance.
  /// If the query string is empty or cannot be parsed, the binding operation will fail.</remarks>
  internal sealed class FilterInfoModelBinder : QueryStringParserModelBinderBase<FilterInfoCollection>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfoModelBinder"/> class with the specified parsing strategy.
    /// </summary>
    /// <param name="parser">
    /// An instance of <see cref="IParseStrategy{FilterInfoCollection}"/> used to parse the filter query string into a <see cref="FilterInfoCollection"/>.
    /// </param>
    public FilterInfoModelBinder(IParseStrategy<FilterInfoCollection> parser)
      : base(parser) { }
  }
}