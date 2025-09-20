// -----------------------------------------------------------------------
// <copyright file="IParseStrategy.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// When implemented, represents a strategy for parsing a querystring into <see cref="object"/>.
  /// </summary>
  public interface IParseStrategy
  {
    /// <summary>
    ///  Gets the key used to identify the query parameter for filtering.
    /// </summary>
    string ParameterKey
    {
      get;
    }

    /// <summary>
    /// Parses the specified input string and converts it into an object representation.
    /// </summary>
    /// <param name="input">The input string to parse. Cannot be null or empty.</param>
    /// <returns>An object representing the parsed data, or <see langword="null"/> if the input cannot be parsed.</returns>
    object? Parse(string input);

    /// <summary>
    /// Attempts to parse the specified input string and convert it to an object.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="parsed">When this method returns, contains the parsed object if the conversion succeeded, or <see langword="null"/> if
    /// the conversion failed.</param>
    /// <returns><see langword="true"/> if the input string was successfully parsed; otherwise, <see langword="false"/>.</returns>
    bool TryParse(string input, out object? parsed);
  }
}
