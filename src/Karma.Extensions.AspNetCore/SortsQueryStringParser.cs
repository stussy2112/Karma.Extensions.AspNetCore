// -----------------------------------------------------------------------
// <copyright file="SortsQueryStringParser.cs" company="Karma, LLC">
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
  /// Represents a strategy for parsing a string into a <see cref="SortInfo" />.
  /// </summary>
  internal sealed partial class SortsQueryStringParser : IParseStrategy<IEnumerable<SortInfo>>
  {
    private const string ValueGroupName = "value";

    // NOTE: sort={type}.{field} // NOSONAR
    [GeneratedRegex(@"(?<=^|\?|&)sort=(?<value>[\p{L}\p{N}.\-_ ]+[^&\r\n]*)", RegExConstants.RegExOptions, RegExConstants.Culture)]
    private static partial Regex SortInfoRegEx();

    /// <inheritdoc />
    public string ParameterKey
    {
      get;
    } = QueryParameterNames.Sort;

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

      HashSet<string> seen = new (StringComparer.Ordinal);
      foreach (Match match in matches)
      {
        string groupValue = match.Groups[ValueGroupName].Value;
        if (string.IsNullOrWhiteSpace(groupValue))
        {
          continue;
        }

        foreach (string field in groupValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
          SortInfo? sortInfo;
          try
          {
            // TRICKY: The `SortInfo` constructor will validate the field name ...
            sortInfo = field; // Implicit conversion from string to SortInfo
          }
          catch (ArgumentException)
          {
            // ... and throw an `ArgumentException` if invalid
            // Skip invalid field names (e.g., just "-" or empty after trimming "-")
            continue;
          }

          // Only yield if successfully created and not already seen
          if (seen.Add(sortInfo.FieldName))
          {
            yield return sortInfo;
          }
        }
      }
    }

    public bool TryParse(string input, [NotNullWhen(true)] out IEnumerable<SortInfo>? parsed)
    {
      parsed = null;

      if (string.IsNullOrWhiteSpace(input))
      {
        return false;
      }

      parsed = Parse(input);
      parsed = parsed.Any() ? parsed : null;
      return parsed is not null;
    }

    /// <inheritdoc />
    object? IParseStrategy.Parse(string input) => Parse(input);

    bool IParseStrategy.TryParse(string input, [NotNullWhen(true)] out object? parsed)
    {
      bool result = TryParse(input, out IEnumerable<SortInfo>? sortInfos);
      parsed = sortInfos;
      return result;
    }
  }
}