// -----------------------------------------------------------------------
// <copyright file="FilterInfoCollection.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents a collection of <see cref="FilterInfo"/> objects, grouped by a specified conjunction and name.
  /// </summary>
  /// <remarks>This class provides a read-only collection of filters, allowing logical grouping and evaluation
  /// of filters using a specified <see cref="Conjunction"/> (e.g., AND or OR). It is commonly used to manage and
  /// evaluate filter criteria in a structured manner.</remarks>
  [System.Diagnostics.DebuggerDisplay("{MemberOf,nq}.{Name,nq} : {Conjunction} : {Count}")]
  public sealed record FilterInfoCollection : IFilterInfo, IReadOnlyCollection<IFilterInfo>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfoCollection"/> class with default values.
    /// </summary>
    /// <remarks>This constructor initializes the collection with an empty filter name, a default root element
    /// of "root", a conjunction type of <see cref="Conjunction.And"/>, and an empty list of filters.</remarks>
    public FilterInfoCollection()
      : this(string.Empty, "root", Conjunction.And, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfoCollection"/> class with the specified name and optional
    /// filters.
    /// </summary>
    /// <param name="name">The name of the filter collection. This parameter is required and cannot be null or empty.</param>
    /// <param name="filters">An optional collection of filters to include in the filter collection. If null, the collection will be
    /// initialized empty.</param>
    public FilterInfoCollection([Required] string name, IEnumerable<IFilterInfo>? filters = null)
      : this(string.Empty, name, Conjunction.And, filters)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfoCollection"/> class with the specified name, conjunction,
    /// and optional filters.
    /// </summary>
    /// <param name="name">The name of the filter collection. This parameter is required and cannot be null or empty.</param>
    /// <param name="conjunction">The logical conjunction to apply between the filters in the collection, such as AND or OR.</param>
    /// <param name="filters">An optional collection of filters to include in the filter collection. If null, the collection will be
    /// initialized empty.</param>
    public FilterInfoCollection([Required] string name, Conjunction conjunction, IEnumerable<IFilterInfo>? filters = null)
      : this(string.Empty, name, conjunction, filters)
    {
    }

    /// <summary>
    /// Represents a collection of filter information, allowing for logical grouping and conjunction-based evaluation of
    /// filters.
    /// </summary>
    /// <param name="memberOf">The name of the parent group or entity that this collection is a member of. Can be <see langword="null"/> if not
    /// part of a group.</param>
    /// <param name="name">The name of the filter collection. This value is required and cannot be <see langword="null"/> or empty.</param>
    /// <param name="conjunction">The logical conjunction used to evaluate the filters in the collection. Defaults to <see
    /// cref="Conjunction.And"/>.</param>
    /// <param name="filters">An optional collection of filters to initialize the filter collection with. Can be <see langword="null"/> to
    /// start with an empty collection.</param>
    public FilterInfoCollection(string? memberOf, [Required] string name, Conjunction conjunction = Conjunction.And, IEnumerable<IFilterInfo>? filters = null)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
      }

      Name = name;
      Conjunction = conjunction;
      MemberOf = memberOf;
      Filters = [.. new HashSet<IFilterInfo>(filters ?? [])];
    }

    /// <summary>
    /// Gets the collection of filters applied to the current context.
    /// </summary>
    /// <remarks>The collection is initialized as an empty <see cref="ImmutableHashSet{T}"/> and cannot be modified
    /// directly.  Filters can be used to define additional processing or behavior for the context.</remarks>
    private IReadOnlyCollection<IFilterInfo> Filters
    {
      get;
    }

    /// <inheritdoc/>
    public int Count => Filters.Count;

    /// <summary>
    /// Gets or sets the conjunction used to combine multiple conditions in a query.
    /// </summary>
    public Conjunction Conjunction
    {
      get;
    }

    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name
    {
      get;
    }

    /// <summary>
    /// Gets or sets the name of the group or entity to which this member belongs.
    /// </summary>
    public string? MemberOf
    {
      get;
    }

    /// <inheritdoc/>
    public IEnumerator<IFilterInfo> GetEnumerator() => Filters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
