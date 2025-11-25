// -----------------------------------------------------------------------
// <copyright file="PageInfoQueryStringParser.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Provides functionality to parse query string parameters into a <see cref="PageInfo"/> object.
  /// </summary>
  /// <remarks>This class is designed to extract pagination-related information, such as "after", "before",
  /// "limit", and "offset", from a query string. It implements the <see cref="IParseStrategy{T}"/> interface for
  /// parsing query strings into <see cref="PageInfo"/> instances.</remarks>
  internal sealed partial class PageInfoQueryStringParser : IParseStrategy<PageInfo>
  {
    private readonly PageInfoPatternProvider _patternProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfoQueryStringParser"/> class.
    /// </summary>
    /// <param name="patternProvider">An optional <see cref="PageInfoPatternProvider"/> instance used to define the patterns for parsing query
    /// strings. If not provided, a default instance of <see cref="PageInfoPatternProvider"/> is used.</param>
    public PageInfoQueryStringParser(PageInfoPatternProvider? patternProvider = null) =>
      _patternProvider = patternProvider ?? new PageInfoPatternProvider();

    /// <inheritdoc />
    public string ParameterKey
    {
      get;
    } = QueryParameterNames.Page;

    /// <inheritdoc />
    public PageInfo Parse(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        return new PageInfo();
      }

      return ParseInternal(input) ?? new PageInfo();
    }

    public bool TryParse(string input, [NotNullWhen(true)] out PageInfo? parsed)
    {
      parsed = null;

      if (string.IsNullOrWhiteSpace(input))
      {
        return false;
      }

      parsed = ParseInternal(input);
      return parsed is not null;
    }

    private PageInfo CreatePageInfo(Dictionary<string, List<Match>> matchesByPropertyName)
    {
      string after = string.Empty;
      string before = string.Empty;
      uint limit = uint.MaxValue;
      uint offset = 0;

      foreach ((string key, IReadOnlyCollection<Match> val) in matchesByPropertyName)
      {
        string propValue = val.Select((m) => m.Groups.GetGroupCollectionValue(_patternProvider.ValueGroupName))
          .LastOrDefault(string.Empty);

        (after, before, limit, offset) = key.ToUpperInvariant() switch
        {
          "AFTER" or "CURSOR" => (propValue, before, limit, offset),
          "BEFORE" => (after, propValue, limit, offset),
          "LIMIT" => (after, before, uint.TryParse(propValue, out uint parsedLimit) ? parsedLimit : uint.MaxValue, offset),
          "OFFSET" => (after, before, limit, uint.TryParse(propValue, out uint parsedOffset) ? parsedOffset : 0),
          _ => (after, before, limit, offset)
        };
      }

      return new PageInfo(after, before, offset, limit);
    }

    private PageInfo? ParseInternal(string input)
    {
      // Only unescape if needed
      string query = input.Contains('%', StringComparison.OrdinalIgnoreCase) ? Uri.UnescapeDataString(input) : input;
      MatchCollection matches;

      try
      {
        matches = _patternProvider.RegularExpression.Matches(query);
      }
      catch (RegexMatchTimeoutException)
      {
        return null;
      }

      if (matches.Count == 0)
      {
        return null;
      }

      // Group by property name
      Dictionary<string, List<Match>> matchesByPropertyName = matches.CreateGroupDictionary((m) => m.Groups.GetGroupCollectionValue(_patternProvider.PropertyGroupName));

      return CreatePageInfo(matchesByPropertyName);
    }

    /// <inheritdoc />
    object? IParseStrategy.Parse(string input) => Parse(input);

    /// <inheritdoc />
    bool IParseStrategy.TryParse(string input, [NotNullWhen(true)] out object? parsed)
    {
      bool result = TryParse(input, out PageInfo? pageInfo);
      parsed = pageInfo;
      return result;
    }
  }
}