// -----------------------------------------------------------------------
// <copyright file="IFilterInfo.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents metadata about a filter, including its name and the group it belongs to.
  /// </summary>
  /// <remarks>This interface is typically used to provide descriptive information about a filter, such as its
  /// name and the group or category it is associated with.</remarks>
  public interface IFilterInfo
  {
    /// <summary>
    /// Gets or sets the name of the group or entity to which this filter belongs.
    /// </summary>
    string? MemberOf
    {
      get;
    }

    /// <summary>
    /// Gets or sets the name associated with the filter.
    /// </summary>
    string Name
    {
      get;
    }
  }
}