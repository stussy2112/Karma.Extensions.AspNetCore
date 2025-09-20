// -----------------------------------------------------------------------
// <copyright file="ContextItemKeys.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore.DependencyInjection
{
  /// <summary>
  /// Provides a collection of predefined keys for accessing or storing context-specific data, such as filters,
  /// pagination, and sorting information.
  /// </summary>
  /// <remarks>The <see cref="ContextItemKeys"/> class defines constant string keys that can be used in contexts
  /// such as configuration settings,  dictionaries, or other data structures to standardize the identification of
  /// common context items.  These keys are case-sensitive and should be used consistently to avoid
  /// mismatches.</remarks>
  public static class ContextItemKeys
  {
    /// <summary>
    /// Represents the key used to identify filters in the <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> collection.
    /// </summary>
    /// <remarks>This constant can be used as a key for accessing or storing filter-related data in a
    /// dictionary or similar data structure. The value is case-sensitive.</remarks>
    public const string Filters = "filters";

    /// <summary>
    /// Represents the key used to identify page information in the <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> collection.
    /// </summary>
    /// <remarks>This constant can be used as a key for accessing or storing page-related metadata, such as
    /// pagination details.</remarks>
    public const string PageInfo = "pageInfo";

    /// <summary>
    /// Represents the key used to store or retrieve sorting information the <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> collection.
    /// </summary>
    /// <remarks>This constant can be used as a key for accessing or storing sort-related metadata, such as
    /// pagination details.</remarks>
    public const string SortInfo = "sortInfo";
  }
}