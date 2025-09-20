// -----------------------------------------------------------------------
// <copyright file="SortsQueryStringParser.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents a strategy for parsing a string into a <see cref="SortInfo" />.
  /// </summary>
  internal sealed partial class SortsQueryStringParser : IParseStrategy<IEnumerable<SortInfo>>
  {
    private const string ValueGroupName = "value";

    // NOTE sort={type}.{field} // NOSONAR
    [GeneratedRegex(@"(?<=^|\?|&)sort=(?<value>[\p{L}\p{N}.\-_ ]+[^&\r\n]*)", RegExConstants.RegExOptions, RegExConstants.Culture)]
    private static partial Regex SortInfoRegEx();

    /// <inheritdoc />
    public string ParameterKey
    {
      get;
    } = QueryParameterNames.SortInfo;

    /// <inheritdoc />
    public IEnumerable<SortInfo> Parse(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
      {
        yield break;
      }

      // Only unescape if needed
      string query = input.Contains('%', StringComparison.OrdinalIgnoreCase) ? Uri.UnescapeDataString(input) : input;

      MatchCollection matches = SortInfoRegEx().Matches(query);

      if (matches.Count == 0)
      {
        yield break;
      }

      var seen = new HashSet<SortInfo>();
      foreach (Match match in matches)
      {
        string groupValue = match.Groups[ValueGroupName].Value;
        if (string.IsNullOrWhiteSpace(groupValue))
        {
          continue;
        }

        foreach (string field in groupValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
          SortInfo sortInfo = field;
          if (seen.Add(sortInfo)) // Only yield if not already seen
          {
            yield return sortInfo;
          }
        }
      }
    }

    public bool TryParse(string input, out IEnumerable<SortInfo>? parsed)
    {
      parsed = null;

      if (string.IsNullOrWhiteSpace(input))
      {
        return false;
      }

      parsed = Parse(input);

      return parsed.Any();
    }

    /// <inheritdoc />
    object? IParseStrategy.Parse(string input) => Parse(input);

    bool IParseStrategy.TryParse(string input, out object? parsed)
    {
      bool result = TryParse(input, out IEnumerable<SortInfo>? sortInfos);
      parsed = sortInfos;
      return result;
    }
  }
}