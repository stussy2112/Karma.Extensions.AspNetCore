// -----------------------------------------------------------------------
// <copyright file="SortInfo.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents sorting information, including the field to sort by and the sort direction.
  /// </summary>
  /// <remarks>This type is immutable and provides functionality for creating and managing sorting
  /// configurations. It supports implicit conversions to and from <see cref="string"/> for convenience.</remarks>
  [DebuggerDisplay("{OriginalFieldName,nq} : {FieldName,nq} : {Direction}")]
  public sealed record SortInfo
  {
    /// <summary>
    /// Initializes a new <see cref="SortInfo" /> instance.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="direction"></param>
    public SortInfo([Required] string fieldName, ListSortDirection direction = ListSortDirection.Ascending)
    {
      if (string.IsNullOrWhiteSpace(fieldName))
      {
        throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or empty.", nameof(fieldName));
      }

      Direction = direction == ListSortDirection.Ascending && fieldName.StartsWith('-') ? ListSortDirection.Descending : direction;
      FieldName = fieldName.TrimStart('-');
      OriginalFieldName = fieldName;
    }

    /// <summary>
    /// The direction of the sort
    /// </summary>
    public ListSortDirection Direction
    {
      get;
    }

    /// <summary>
    /// The field on which to sort
    /// </summary>
    public string FieldName
    {
      get;
    }

    /// <summary>
    /// Gets the original field name.
    /// </summary>
    public string OriginalFieldName
    {
      get;
    }

    /// <summary>
    /// Performs a conversion from <see cref="SortInfo"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="sortParameter">The sort parameter.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    [return: NotNullIfNotNull(nameof(sortParameter))]
    public static implicit operator string(SortInfo? sortParameter) => sortParameter?.ToString() ?? string.Empty;

    /// <summary>
    /// Performs a conversion from <see cref="string"/> to <see cref="SortInfo"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator SortInfo(string value) => new SortInfo(value);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => OriginalFieldName;

    /// <summary>
    /// Creates a collection of <see cref="SortInfo"/> instances with a default comparer
    /// </summary>
    /// <param name="sortInfo">The source <see cref="IEnumerable{T}"/> to seed the collection.</param>
    /// <returns>An <see cref="ICollection{T}"/> of <see cref="SortInfo"/> instances.</returns>
    public static ICollection<SortInfo> CreateCollection(IEnumerable<SortInfo>? sortInfo = null) =>
      new HashSet<SortInfo>(sortInfo ?? []);
  }
}
