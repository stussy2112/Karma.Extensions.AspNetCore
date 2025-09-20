// -----------------------------------------------------------------------
// <copyright file="PageInfoPatternProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides functionality for parsing and extracting page information from query strings using regular expressions.
  /// </summary>
  /// <remarks>The <see cref="PageInfoPatternProvider"/> class is designed to match and extract
  /// pagination-related information (e.g., "after", "before", "cursor", "limit", "offset") from query strings. It
  /// supports both a default pattern and custom patterns provided by the user. The default pattern is optimized using
  /// source-generated regular expressions for improved performance.</remarks>
  public sealed partial class PageInfoPatternProvider : PatternProviderBase
  {
    private const string DefaultPropertyGroupName = "property";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string DefaultPageInfoPattern = @"(?<=^|\?|&)page\[(?<property>after|before|cursor|limit|offset)\]=(?<value>[^&\r\n]*)";

    /// <summary>
    /// Generates a compiled regex for the default page info pattern using source generation.
    /// </summary>
    /// <returns>A compiled <see cref="Regex"/> instance optimized for the default page info pattern.</returns>
    [GeneratedRegex(DefaultPageInfoPattern, RegExConstants.RegExOptions | RegexOptions.IgnoreCase, RegExConstants.Culture)]
    private static partial Regex GetDefaultPageInfoRegex();

    /// <summary>
    /// Gets the default instance of the <see cref="PageInfoPatternProvider"/> class.
    /// </summary>
    public static readonly PageInfoPatternProvider Default = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoPatternProvider"/> class with the default page info pattern.
    /// </summary>
    public PageInfoPatternProvider()
      : base(DefaultPageInfoPattern) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoPatternProvider"/> class with the specified regular
    /// expression pattern and optional group names for extracting property and value information.
    /// </summary>
    /// <param name="pattern">A regular expression pattern used to match and extract information. The pattern must include named groups for
    /// properties and values, as specified by <paramref name="propertyGroupName"/> and <paramref
    /// name="valueGroupName"/>.</param>
    /// <param name="propertyGroupName">The name of the regular expression group used to extract property names. This value cannot be null, empty, or
    /// whitespace. Defaults to <see cref="DefaultPropertyGroupName"/>.</param>
    /// <param name="valueGroupName">The name of the regular expression group used to extract property values. Defaults to <see
    /// cref="PatternProviderBase.DefaultValueGroupName"/>.</param>
    public PageInfoPatternProvider([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string propertyGroupName = DefaultPropertyGroupName, string? valueGroupName = DefaultValueGroupName)
      : base(pattern, valueGroupName)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(propertyGroupName);
      PropertyGroupName = propertyGroupName;
    }

    /// <summary>
    /// Gets the name of the group used to capture the property component of the page info pattern.
    /// </summary>
    public string PropertyGroupName
    {
      get;
      init;
    } = DefaultPropertyGroupName;

    /// <summary>
    /// Gets the regular expression used to match patterns based on the specified configuration.
    /// </summary>
    /// <remarks>The regular expression uses source generation for optimal performance when using the default pattern,
    /// otherwise falls back to runtime compilation.</remarks>
    public override Regex RegularExpression =>
      Pattern == DefaultPageInfoPattern
        ? GetDefaultPageInfoRegex()
        : base.RegularExpression;
  }
}
