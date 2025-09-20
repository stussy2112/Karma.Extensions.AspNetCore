// -----------------------------------------------------------------------
// <copyright file="PatternProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Serves as a base class for defining pattern-based providers that use regular expressions to extract or process
  /// data.
  /// </summary>
  /// <remarks>This class provides a foundation for working with regular expressions by encapsulating a regex
  /// pattern and an optional value group name. Derived classes can use these properties to implement specific
  /// pattern-matching or data-extraction logic.</remarks>
  public abstract class PatternProviderBase
  {
    /// <summary>
    /// Represents the default name of the capturing group in the regular expression pattern whose value will be extracted.
    /// </summary>
    protected const string DefaultValueGroupName = "value";

    /// <summary>
    /// Initializes a new instance of the <see cref="PatternProviderBase"/> class with the specified regular expression
    /// pattern and optional value group name.
    /// </summary>
    /// <param name="pattern">The regular expression pattern used to match input strings. This parameter cannot be null or empty.</param>
    /// <param name="valueGroupName">The name of the capturing group in the regular expression pattern whose value will be extracted.  If null or
    /// whitespace, the default group name <see cref="DefaultValueGroupName"/> is used.</param>
    protected PatternProviderBase([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string? valueGroupName = DefaultValueGroupName)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

      Pattern = pattern;
      ValueGroupName = string.IsNullOrWhiteSpace(valueGroupName) ? DefaultValueGroupName : valueGroupName;
    }

    /// <summary>
    /// Gets the regular expression pattern used for matching input strings.
    /// </summary>
    /// <remarks>This property is immutable and must be initialized when the object is created. The pattern is
    /// annotated with <see cref="StringSyntaxAttribute.Regex"/>,  indicating that it is expected to follow the syntax
    /// of regular expressions.</remarks>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    public string Pattern
    {
      get;
      init;
    }

    /// <summary>
    /// Gets the name of the capturing group in the regular expression pattern whose value will be extracted.
    /// </summary>
    public string ValueGroupName
    {
      get;
      init;
    }

    /// <summary>
    /// Gets the regular expression used to match patterns based on the specified configuration.
    /// </summary>
    /// <remarks>The regular expression is constructed using the <c>Pattern</c> and predefined options from
    /// <see cref="RegExConstants.RegExOptions"/>.</remarks>
    public virtual Regex RegularExpression => new Regex(Pattern, RegExConstants.RegExOptions | RegexOptions.IgnoreCase);
  }
}
