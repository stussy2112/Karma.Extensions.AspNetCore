// -----------------------------------------------------------------------
// <copyright file="FilterPatternProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides functionality for parsing and extracting components from filter expressions using configurable regular
  /// expression patterns and group names.
  /// </summary>
  /// <remarks>The <see cref="FilterPatternProvider"/> class is designed to parse filter expressions commonly
  /// used in query strings or similar contexts. It supports a default filter pattern and allows customization of the
  /// regular expression and group names for parsing specific components, such as paths, values, operators, and
  /// conjunctions.</remarks>
  public sealed partial class FilterPatternProvider : PatternProviderBase
  {
    private const string DefaultConjunctionGroupName = "conjunction";
    private const string DefaultGroupIndexGroupName = "groupIndex";
    private const string DefaultMemberOfGroupName = "memberOf";
    private const string DefaultOperatorGroupName = "operator";
    private const string DefaultPathGroupName = "path";
    private const string DefaultTypeGroupName = "type";

    [StringSyntax(StringSyntaxAttribute.Regex)]
    private const string DefaultFilterPattern = @"
(?:(?<=^|\?|&)filter)
(?:
    \[(?<type>group)\]
    (?:\[\$(?<conjunction>and|or)\])?
    (?:\[(?<memberOf>[\p{L}\p{N}.\-_ ]+?)\])?(?:\[(?<groupIndex>\d+)\])?
    =(?<path>[^&\r\n]*)
|
    (?:
      (?:\[\$(?<conjunction>and|or)\](?:\[(?<groupIndex>\d+)\])?)
      | (?:\[(?<memberOf>[\p{L}\p{N}.\-_ ]+)\]\[(?<groupIndex>\d+)\])
    )?
    (?:\[(?<path>[\p{L}\p{N}.\-_ ]+?)\])+
    (?i)(?:\[\$(?<operator>eq|ge|gt|gte|le|lt|lte|ne|endswith|startswith|regex|notbetween|notcontains|notin|notnull|between|contains|in|null)\])?
    =(?<value>[^&\r\n]*)
)";

    /// <summary>
    /// Generates a compiled regex for the default filter pattern using source generation.
    /// </summary>
    /// <returns>A compiled <see cref="Regex"/> instance optimized for the default filter pattern.</returns>
    [GeneratedRegex(DefaultFilterPattern, RegExConstants.RegExOptions | RegexOptions.IgnoreCase, RegExConstants.MatchTimeoutMilliseconds, RegExConstants.Culture)]
    private static partial Regex GetDefaultFilterRegex();

    /// <summary>
    /// Gets the default instance of the <see cref="FilterPatternProvider"/> class.
    /// </summary>
    public static FilterPatternProvider Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPatternProvider"/> class with the default filter pattern.
    /// </summary>
    public FilterPatternProvider()
        : base(DefaultFilterPattern) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPatternProvider"/> class with the specified regular
    /// expression pattern and group names for parsing filter components.
    /// </summary>
    /// <remarks>This constructor allows you to specify a regular expression pattern and customize the names
    /// of the groups used to parse different components of a filter expression. If no group names are provided, default
    /// group names will be used.</remarks>
    /// <param name="pattern">A regular expression pattern used to parse filter expressions. The pattern must define named groups for the
    /// filter components, such as path, value, operator, and others.</param>
    /// <param name="pathGroupName">The name of the regular expression group that captures the path component of the filter. Defaults to <see
    /// cref="DefaultPathGroupName"/>.</param>
    /// <param name="valueGroupName">The name of the regular expression group that captures the value component of the filter. Defaults to <see
    /// cref="PatternProviderBase.DefaultValueGroupName"/>.</param>
    /// <param name="operatorGroupName">The name of the regular expression group that captures the operator component of the filter. Defaults to <see
    /// cref="DefaultOperatorGroupName"/>.</param>
    /// <param name="typeGroupName">The name of the regular expression group that captures the type component of the filter. Defaults to <see
    /// cref="DefaultTypeGroupName"/>.</param>
    /// <param name="conjunctionGroupName">The name of the regular expression group that captures the conjunction (e.g., AND/OR) component of the filter.
    /// Defaults to <see cref="DefaultConjunctionGroupName"/>.</param>
    /// <param name="groupIndexGroupName">The name of the regular expression group that captures the group index component of the filter. Defaults to <see
    /// cref="DefaultGroupIndexGroupName"/>.</param>
    /// <param name="memberOfGroupName">The name of the regular expression group that captures the "member of" component of the filter. Defaults to <see
    /// cref="DefaultMemberOfGroupName"/>.</param>
    public FilterPatternProvider([StringSyntax(StringSyntaxAttribute.Regex)] string pattern, string pathGroupName = DefaultPathGroupName, string valueGroupName = DefaultValueGroupName, string operatorGroupName = DefaultOperatorGroupName, string typeGroupName = DefaultTypeGroupName, string conjunctionGroupName = DefaultConjunctionGroupName, string groupIndexGroupName = DefaultGroupIndexGroupName, string memberOfGroupName = DefaultMemberOfGroupName)
        : base(pattern, valueGroupName)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(pathGroupName);
      ArgumentException.ThrowIfNullOrWhiteSpace(operatorGroupName);
      ArgumentException.ThrowIfNullOrWhiteSpace(typeGroupName);
      ArgumentException.ThrowIfNullOrWhiteSpace(conjunctionGroupName);
      ArgumentException.ThrowIfNullOrWhiteSpace(groupIndexGroupName);
      ArgumentException.ThrowIfNullOrWhiteSpace(memberOfGroupName);

      ConjunctionGroupName = conjunctionGroupName;
      GroupIndexGroupName = groupIndexGroupName;
      MemberOfGroupName = memberOfGroupName;
      OperatorGroupName = operatorGroupName;
      PathGroupName = pathGroupName;
      TypeGroupName = typeGroupName;
    }

    /// <summary>
    /// Gets the name of the group used to capture conjunctions (e.g., "and", "or") in the filter pattern.
    /// </summary>
    public string ConjunctionGroupName
    {
      get;
      init;
    } = DefaultConjunctionGroupName;

    /// <summary>
    /// Gets the name of the group used to capture group indices in the filter pattern.
    /// </summary>
    public string GroupIndexGroupName
    {
      get;
      init;
    } = DefaultGroupIndexGroupName;

    /// <summary>
    /// Gets the name of the group used to capture "memberOf" values in the filter pattern.
    /// </summary>
    public string MemberOfGroupName
    {
      get;
      init;
    } = DefaultMemberOfGroupName;

    /// <summary>
    /// Gets the name of the group used to capture operators (e.g., "eq", "ne", "gt") in the filter pattern.
    /// </summary>
    public string OperatorGroupName
    {
      get;
      init;
    } = DefaultOperatorGroupName;

    /// <summary>
    /// Gets the name of the group used to capture paths in the filter pattern.
    /// </summary>
    public string PathGroupName
    {
      get;
      init;
    } = DefaultPathGroupName;

    /// <summary>
    /// Gets the name of the group used to capture types (e.g., "group") in the filter pattern.
    /// </summary>
    public string TypeGroupName
    {
      get;
      init;
    } = DefaultTypeGroupName;

    /// <summary>
    /// Gets the regular expression used to match patterns based on the specified configuration.
    /// </summary>
    /// <remarks>The regular expression uses source generation for optimal performance when using the default pattern,
    /// otherwise falls back to runtime compilation.</remarks>
    public override Regex RegularExpression =>
      Pattern == DefaultFilterPattern
        ? GetDefaultFilterRegex()
        : base.RegularExpression;
  }
}
