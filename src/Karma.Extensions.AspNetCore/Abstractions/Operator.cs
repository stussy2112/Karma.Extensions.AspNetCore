// -----------------------------------------------------------------------
// <copyright file="Operator.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.Serialization;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Defines a set of operators used for comparison and logical operations.
  /// </summary>
  /// <remarks>The <see cref="Operator"/> enumeration provides a comprehensive list of operators commonly used
  /// in filtering, querying, and conditional logic. Each operator represents a specific type of comparison  or logical
  /// evaluation, such as equality, inequality, range checks, or string matching.  Additionally, attributes such as
  /// <see cref="DescriptionAttribute"/> and <see cref="EnumMemberAttribute"/> provide metadata for display purposes
  /// or serialization.</remarks>
  public enum Operator
  {

    /// <summary>
    /// Represents a placeholder or default value indicating the absence of a specific option or selection.
    /// </summary>
    /// <remarks>This value can be used to signify that no meaningful value has been assigned or selected. It
    /// is commonly used in enumerations or configurations where a default or uninitialized state is required.</remarks>
    None,

    /// <summary>
    /// Represents an equality comparison operation.
    /// </summary>
    /// <remarks>This enumeration value is used to specify that two operands should be compared for equality.</remarks>
    [Description("Equals")]
    [EnumMember(Value = "eq")]
    EqualTo,

    /// <summary>
    /// Represents a comparison operation where two values are checked for inequality.
    /// </summary>
    /// <remarks>This enumeration value is typically used in scenarios where a "not equal to" comparison is
    /// required, such as in filtering, conditional logic, or query expressions.</remarks>
    [Description("Not equals")]
    [EnumMember(Value = "ne")]
    NotEqualTo,

    /// <summary>
    /// Represents a comparison operation where the left operand is less than the right operand.
    /// </summary>
    [Description("Less than")]
    [EnumMember(Value = "lt")]
    LessThan,

    /// <summary>
    /// Represents a comparison operation where the left operand is less than or equal to the right operand.
    /// </summary>
    [Description("Less than or equals")]
    [EnumMember(Value = "le")]
    LessThanOrEqualTo,

    /// <summary>
    /// Represents a comparison operation where the left operand is greater than the right operand.
    /// </summary>
    [Description("Greater than")]
    [EnumMember(Value = "gt")]
    GreaterThan,

    /// <summary>
    /// Represents a comparison operation where the left-hand operand is greater than or equal to the right-hand operand.
    /// </summary>
    [Description("Greater than or equals")]
    [EnumMember(Value = "ge")]
    GreaterThanOrEqualTo,

    /// <summary>
    /// Represents a comparison operation where a specified value is inclusively between two given bounds.
    /// </summary>
    Between,

    /// <summary>
    ///Represents a comparison operation where a value must not fall within a specified range.
    /// </summary>
    /// <remarks>This constraint is typically used to validate that a value lies outside of a defined range.
    /// The range is inclusive of its boundaries unless otherwise specified.</remarks>
    NotBetween,

    /// <summary>
    /// Represents a comparison operation where the specified substring occurs within this string.
    /// </summary>
    /// <remarks>This method performs a case-sensitive and culture-sensitive comparison using the current
    /// culture.</remarks>
    Contains,

    /// <summary>
    /// Represents a comparison operation where the specified substring does notoccurs within this string.
    /// </summary>
    /// <remarks>This method performs a case-sensitive and culture-sensitive comparison using the current
    /// culture.</remarks>
    NotContains,

    /// <summary>
    /// Represents a comparison operation where string instance matches the specified string.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Represents an operator that checks whether a value is within a specified set or range.
    /// </summary>
    /// <remarks>This operator is typically used to evaluate whether a given value exists within a collection,
    /// range, or predefined set of values.</remarks>
    In,

    /// <summary>
    /// Represents a comparison operation where the specified value is not present in the provided collection.
    /// </summary>
    NotIn,

    /// <summary>
    /// Represents a comparison operation where the specified object is null.
    /// </summary>
    [EnumMember(Value = "null")]
    [Description("Is null")]
    IsNull,

    /// <summary>
    /// Represents a comparison operation where the specified object is not null.
    /// </summary>
    [EnumMember(Value = "notnull")]
    [Description("Is not null")]
    IsNotNull,

    /// <summary>
    ///Represents a comparison operation where the beginning of this string instance matches the specified string.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Represents an immutable regular expression used for pattern matching in strings.
    /// </summary>
    /// <remarks>The <see cref="Regex"/> class provides methods for searching, matching, and replacing text
    /// using regular expressions. It supports advanced pattern matching features such as grouping, backreferences, and
    /// lookahead/lookbehind assertions. Instances of <see cref="Regex"/> are immutable and thread-safe, making them
    /// suitable for concurrent use.</remarks>
    Regex
  }
}
