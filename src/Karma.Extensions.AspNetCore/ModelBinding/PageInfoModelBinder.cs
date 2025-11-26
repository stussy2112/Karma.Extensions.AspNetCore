// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinder.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore.Mvc.ModelBinding
{
  /// <summary>
  /// Provides model binding for the <see cref="PageInfo"/> type, supporting binding of pagination-related properties
  /// such as <c>After</c>, <c>Before</c>, <c>Offset</c>, and <c>Limit</c>.
  /// </summary>
  /// <remarks>This model binder attempts to bind the <see cref="PageInfo"/> properties from the incoming
  /// request data, such as query string parameters, route data, or form data. 
  /// <para>The <c>After</c> property supports a fallback binding to "Cursor" if the standard property name fails.</para>
  /// <para>The <c>After</c> and <c>Before</c> properties are bound as strings, while the <c>Offset</c> and 
  /// <c>Limit</c> properties are bound as unsigned integers.</para>
  /// <para>The binding succeeds if at least one property is successfully bound.</para></remarks>
  internal sealed class PageInfoModelBinder : QueryStringParserModelBinderBase<PageInfo>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoModelBinder"/> class with the specified model binders.
    /// </summary>
    /// <param name="parser">The parsing strategy used to convert query string input into a <see cref="PageInfo"/> instance.</param>
    public PageInfoModelBinder(IParseStrategy<PageInfo> parser)
      : base(parser) { }
  }
}
