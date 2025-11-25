// -----------------------------------------------------------------------
// <copyright file="FilterQueryStringParser.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides functionality to parse query string parameters into a collection of <see cref="FilterInfo"/> objects.
  /// </summary>
  /// <remarks>This class implements the <see cref="IParseStrategy{T}"/> interface to handle query string
  /// parsing for filter-related information. The <see cref="ParameterKey"/> property specifies the query string key
  /// associated with this parser. NOTE: Each condition filter MUST have it's own, unique name.</remarks>
  internal sealed class FilterQueryStringParser : IParseStrategy<FilterInfoCollection>
  {
    private const string DefaultCollectionName = "root";

    private const string DefaultGroupIndex = "0";
    private const string GroupTypeName = "group";
    private static readonly Dictionary<string, Operator> _operatorMappings = new(StringComparer.OrdinalIgnoreCase)
    {
      { "eq", Operator.EqualTo },
      { "ge", Operator.GreaterThanOrEqualTo },
      { "gte", Operator.GreaterThanOrEqualTo },
      { "le", Operator.LessThanOrEqualTo },
      { "lte", Operator.LessThanOrEqualTo },
      { "ne", Operator.NotEqualTo },
      { "gt", Operator.GreaterThan },
      { "lt", Operator.LessThan },
      { "null", Operator.IsNull },
      { "notnull", Operator.IsNotNull },
      { "contains", Operator.Contains },
      { "notcontains", Operator.NotContains },
      { "in", Operator.In },
      { "notin", Operator.NotIn },
      { "between", Operator.Between },
      { "notbetween", Operator.NotBetween },
      { "startswith", Operator.StartsWith },
      { "endswith", Operator.EndsWith },
      { "regex", Operator.Regex }
    };
    private readonly FilterPatternProvider _patternProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterQueryStringParser"/> class.
    /// </summary>
    /// <param name="patternProvider">An optional <see cref="FilterPatternProvider"/> instance used to provide filter patterns.  If not specified, the
    /// default pattern provider is used.</param>
    public FilterQueryStringParser(FilterPatternProvider? patternProvider = null) =>
      _patternProvider = patternProvider ?? FilterPatternProvider.Default;

    /// <inheritdoc />
    public string ParameterKey
    {
      get;
    } = QueryParameterNames.Filter;

    /// <inheritdoc />
    public FilterInfoCollection Parse(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return new FilterInfoCollection(DefaultCollectionName);
      }

      // Only unescape if needed
      string query = input.Contains('%', StringComparison.OrdinalIgnoreCase) ? Unescape(input) : input;
      MatchCollection filterMatches;

      try
      {
        filterMatches = _patternProvider.RegularExpression.Matches(query);
      }
      catch (RegexMatchTimeoutException)
      {
        return new FilterInfoCollection(DefaultCollectionName);
      }

      IReadOnlyList<IFilterInfo> rootGroups = BuildFilterHierarchy(filterMatches);

      if (rootGroups.Count == 0)
      {
        return new FilterInfoCollection(DefaultCollectionName);
      }

      // If only one root group and it's a FilterInfoCollection, return it directly
      return rootGroups.Count == 1 && rootGroups[0] is FilterInfoCollection singleGroup
        ? singleGroup
        // Otherwise, wrap the filters in a root collection
        : new FilterInfoCollection(DefaultCollectionName, rootGroups);
    }

    /// <summary>
    /// Attempts to parse the specified input string into a collection of filters.
    /// </summary>
    /// <param name="input">The input string to parse. Cannot be null, empty, or consist only of whitespace.</param>
    /// <param name="parsed">When this method returns, contains the parsed collection of filters if the parsing was successful;  otherwise,
    /// <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the input string was successfully parsed into a non-empty collection of filters; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool TryParse(string input, [NotNullWhen(true)] out FilterInfoCollection? parsed)
    {
      parsed = null;

      if (string.IsNullOrWhiteSpace(input))
      {
        return false; // Return false if the input is null, empty or whitespace
      }

      parsed = Parse(input); // Initialize the filters collection

      if (parsed.Count == 0)
      {
        parsed = null;
        return false; // Return false if no filters were added
      }

      return true; // Return true if filters were added
    }

    private IReadOnlyList<IFilterInfo> BuildFilterHierarchy(MatchCollection filterMatches)
    {
      if (filterMatches is null || filterMatches.Count == 0)
      {
        return [];
      }

      // Step 1: Group matches by path
      Dictionary<string, List<Match>> matchesByPath = filterMatches
        .OrderBy((m) => m.Groups.GetGroupCollectionValue(_patternProvider.GroupIndexGroupName, DefaultGroupIndex))
        .CreateGroupDictionary(GetPathValue);

      // Step 2: Build all filters/filter collections
      Dictionary<string, IFilterInfo> allFilters = CreateFilters(matchesByPath);

      // Step 3: Build children lookup
      IDictionary<string, List<IFilterInfo>> childrenByGroup = allFilters.Values
        .Where((f) => !string.IsNullOrWhiteSpace(f.MemberOf))
        .CreateGroupDictionary((fi) => fi.MemberOf!);

      foreach (string groupName in childrenByGroup.Keys.Where((gn) => !allFilters.ContainsKey(gn)))
      {
        // Step 4: Add FilterInfoCollections for the instances that are members of a group and the group wasn't defined
        // Ensure there is a collection for every group (memberOf) name
        Conjunction conjunction = groupName.Contains($"-{Conjunction.Or}-", StringComparison.OrdinalIgnoreCase) ? Conjunction.Or : Conjunction.And;
        allFilters.Add(groupName, new FilterInfoCollection(groupName, conjunction));
      }

      // Step 5: Find root groups (not a member of any other group)
      var allChildNames = new HashSet<string>(childrenByGroup.Values.SelectMany((kvp) => kvp.Select((f) => f.Name)), StringComparer.OrdinalIgnoreCase);
      var rootCandidates = allFilters.Values
        .Where((f) => string.IsNullOrWhiteSpace(f.MemberOf) || !allChildNames.Contains(f.Name))
        .ToList();

      return [.. rootCandidates.Select((fi) => PopulateFilterInfoCollection(fi, childrenByGroup))];
    }

    object? IParseStrategy.Parse(string input) => Parse(input);

    bool IParseStrategy.TryParse(string input, [NotNullWhen(true)] out object? parsed)
    {
      bool result = TryParse(input, out FilterInfoCollection? collection);
      parsed = collection;
      return result;
    }

    private IFilterInfo? CreateFilter(Match filterDefinition, uint index)
    {
      if (filterDefinition is null || !filterDefinition.Success)
      {
        return default;
      }

      // Determine the type of filter to create
      string filterType = filterDefinition.Groups[_patternProvider.TypeGroupName].Value;
      return GroupTypeName.Equals(filterType, StringComparison.OrdinalIgnoreCase)
        ? CreateFilterInfoCollection(filterDefinition)
        : CreateFilterInfo(filterDefinition, index);
    }

    private FilterInfo? CreateFilterInfo(Match filterDefinition, uint index)
    {
      if (filterDefinition is null || !filterDefinition.Success)
      {
        return default;
      }

      // Path of the FilterInfo is concatenation of all the properties
      string path = GetPathValue(filterDefinition);

      // Name of the IFilterInfo is the path
      string name = $"{path}-{index}";

      string memberOf = GetMemberOfName(filterDefinition, path);

      Operator @operator = ParseOperator(filterDefinition);

      object[] values = GetFilterValues(filterDefinition, @operator);

      return new FilterInfo(memberOf, name, path, @operator, values);
    }

    private FilterInfoCollection? CreateFilterInfoCollection(Match filterDefinition)
    {
      if (filterDefinition is null || !filterDefinition.Success)
      {
        return default;
      }

      string name = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.PathGroupName);
      Conjunction conjunction = Enum.TryParse(filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.ConjunctionGroupName), true, out Conjunction result) ? result : Conjunction.And;
      string memberOf = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.MemberOfGroupName);

      return new FilterInfoCollection(memberOf, name, conjunction);
    }

    /// <summary>
    /// Creates a dictionary of filter information objects from the specified matches grouped by path.
    /// </summary>
    /// <param name="matchesByPath">A dictionary containing regex matches grouped by their path values.</param>
    /// <returns>A dictionary mapping filter names to their corresponding <see cref="IFilterInfo"/> objects.</returns>
    private Dictionary<string, IFilterInfo> CreateFilters(Dictionary<string, List<Match>> matchesByPath)
    {
      var allFilters = new Dictionary<string, IFilterInfo>(StringComparer.Ordinal);

      foreach (List<Match> matchList in matchesByPath.Values)
      {
        uint index = 0;
        foreach (Match match in matchList)
        {
          if (CreateFilter(match, index++) is { } filter && !allFilters.ContainsKey(filter.Name))
          {
            allFilters.Add(filter.Name, filter);
          }
        }
      }

      return allFilters;
    }

    private object[] GetFilterValues(Match filterDefinition, Operator @operator)
    {
      // Value of the FilterInfo is the <value> group value
      string allValues = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.ValueGroupName);

      if (@operator is Operator.In or Operator.NotIn or Operator.Between or Operator.NotBetween)
      {
        return allValues.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      }

      return !string.IsNullOrWhiteSpace(allValues) ? [allValues] : [];
    }

    private string GetMemberOfName(Match filterDefinition, string path)
    {
      // The filter is a member of a group IF
      // ...it explicitly defines a <memberOf>
      string memberOf = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.MemberOfGroupName);
      if (string.IsNullOrEmpty(memberOf))
      {
        // ...it explicitly defines a <conjunction> without defining a <memberOf>
        string conjunction = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.ConjunctionGroupName);
        if (!string.IsNullOrWhiteSpace(conjunction))
        {
          memberOf = $"{path}-{conjunction}-group";
        }
      }

      return memberOf;
    }

    private string GetPathValue(Match filterDefinition) =>
      string.Join(".", filterDefinition.Groups[_patternProvider.PathGroupName].Captures.Select((c) => c.Value));

    private static IFilterInfo PopulateFilterInfoCollection(IFilterInfo filterInfo, IDictionary<string, List<IFilterInfo>> childrenByGroup)
    {
      if (filterInfo is FilterInfoCollection fic && childrenByGroup.TryGetValue(fic.Name, out List<IFilterInfo>? children))
      {
        IEnumerable<IFilterInfo> nestedChildren = children.Select((fi) => PopulateFilterInfoCollection(fi, childrenByGroup));
        // Need to find the correct conjunction to use
        return new FilterInfoCollection(fic.MemberOf, fic.Name, fic.Conjunction, nestedChildren);
      }

      return filterInfo;
    }

    private Operator ParseOperator(Match filterDefinition)
    {
      // Operator of the FilterInfo is the <operator> group value
      string valueToConvert = filterDefinition.Groups.GetGroupCollectionValue(_patternProvider.OperatorGroupName);

      if (Enum.TryParse(valueToConvert, true, out Operator op) || _operatorMappings.TryGetValue(valueToConvert, out op))
      {
        return op;
      }

      return Operator.EqualTo;
    }

    /// <summary>
    /// High-performance URL decoding that only allocates when necessary.
    /// </summary>
    /// <param name="input">The input string to decode.</param>
    /// <returns>The decoded string if decoding was needed; otherwise, the original string.</returns>
    private static string Unescape(string input)
    {
      ReadOnlySpan<char> span = input.AsSpan();

      // Fast scan for URL-encoded characters
      for (int i = 0; i < span.Length - 2; i++)
      {
        if (span[i] == '%' && char.IsAsciiHexDigit(span[i + 1]) && char.IsAsciiHexDigit(span[i + 2]))
        {
          // Only decode if we found actual URL-encoded content
          return Uri.UnescapeDataString(input);
        }
      }

      // No encoding found, return original string (no allocation)
      return input;
    }
  }
}