// -----------------------------------------------------------------------
// <copyright file="FilterInfo.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents a filter definition used to specify criteria for filtering data.
  /// </summary>
  /// <remarks>A <see cref="FilterInfo"/> instance encapsulates the details of a filter, including its name, 
  /// the path to the data being filtered, the operator to apply, and the values to compare against.  This type is
  /// immutable and can be used to define filtering logic in data processing scenarios.</remarks>
  [System.Diagnostics.DebuggerDisplay("{MemberOf,nq}.{Name,nq} : '{Path,nq}' {Operator} {Values}")]
  public record FilterInfo : IFilterInfo
  {
    /// <summary>
    /// Instantiates a new <see cref="FilterInfo"/> instance with an <see cref="Operator"/> value of <see cref="Operator.EqualTo"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="FilterInfo"/>.</param>
    /// <param name="path">The path or property name to which the filter applies. This typically represents the field or property being filtered.</param>
    /// <param name="values">The values to which to compare the value in the <paramref name="path"/>.</param>
    public FilterInfo([Required] string name, string path, params object[] values)
      : this(string.Empty, name, path, Operator.None, values)
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="FilterInfo"/> instance.
    /// </summary>
    /// <param name="name">The name of the <see cref="FilterInfo"/>.</param>
    /// <param name="path">The path or property name to which the filter applies. This typically represents the field or property being filtered.</param>
    /// <param name="values">The values to which to compare the value in the <paramref name="path"/>.</param>
    /// <param name="operator">The operator used to evaluate the filter. This determines how the filter's values are compared to the target data.</param>
    public FilterInfo([Required] string name, string path, Operator @operator, params object[] values)
      : this(string.Empty, name, path, @operator, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterInfo"/> class with the specified filter details.
    /// </summary>
    /// <param name="memberOf">The name of the group or category to which this filter belongs. This value can be used to organize or group
    /// filters logically.</param>
    /// <param name="name">The name of the filter. This is a descriptive identifier for the filter.</param>
    /// <param name="path">The path or property name to which the filter applies. This typically represents the field or property being filtered.</param>
    /// <param name="operator">The operator used to evaluate the filter. This determines how the filter's values are compared to the target data.</param>
    /// <param name="values">The set of values used by the filter for comparison. This can include one or more values depending on the operator.</param>
    public FilterInfo(string memberOf, [Required] string name, string path, Operator @operator, params object[] values)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
      }

      Name = name;
      Path = path;
      Values = [.. new HashSet<object>(values ?? [])];
      Operator = @operator;
      MemberOf = memberOf;
    }

    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    [Required]
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

    /// <summary>
    /// Gets or sets the operator associated with the current operation.
    /// </summary>
    public Operator Operator
    {
      get;
    }

    /// <summary>
    /// The path to the data being filtered.
    /// </summary>
    public string? Path
    {
      get;
    }

    /// <summary>
    /// Gets or sets the collection of objects associated with this instance.
    /// </summary>
    public IReadOnlyCollection<object> Values
    {
      get;
    }
  }
}
