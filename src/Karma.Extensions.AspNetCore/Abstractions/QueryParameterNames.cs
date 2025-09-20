// -----------------------------------------------------------------------
// <copyright file="QueryParameterNames.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides a collection of constant values representing common query parameter names used in HTTP requests for
  /// filtering, pagination, and sorting data.
  /// </summary>
  /// <remarks>This class contains predefined query parameter names that can be used to construct or interpret
  /// query strings in HTTP requests. These constants are typically used in APIs to standardize query parameter names
  /// for operations such as filtering, selecting specific fields, paginating results, and sorting data.</remarks>
  internal static class QueryParameterNames
  {
    /// <summary>
    /// filter query parameter used for filtering data.
    /// </summary>
    public const string Filter = "filter";

    /// <summary>
    /// Page query parameter used for pagination of returned data.
    /// </summary>
    public const string PageInfo = "page";

    /// <summary>
    /// sort query parameter used for sorting returned data.
    /// </summary>
    public const string SortInfo = "sort";
  }
}
