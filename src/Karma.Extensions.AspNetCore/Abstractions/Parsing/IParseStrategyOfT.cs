// -----------------------------------------------------------------------
// <copyright file="IParseStrategyOfT.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Defines a strategy for parsing input data into an instance of <typeparamref name="T"/>.
  /// </summary>
  /// <remarks>This interface provides methods for parsing input data, such as strings or key-value pairs, into
  /// strongly-typed objects. Implementations of this interface should define the specific parsing logic for the target
  /// type <typeparamref name="T"/>.</remarks>
  /// <typeparam name="T">The type of the object that the input data will be parsed into.</typeparam>
  public interface IParseStrategy<T> : IParseStrategy
  {
    /// <summary>
    /// Parses the specified input string and converts it to an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="input">The string representation of the value to parse. Cannot be null or empty.</param>
    /// <returns>An instance of type <typeparamref name="T"/> if the parsing is successful; otherwise, <see langword="null"/>.</returns>
    new T? Parse(string input);

    /// <summary>
    /// Attempts to parse the specified input string into an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="parsed">When this method returns, contains the parsed value of type <typeparamref name="T"/> if the parsing succeeds; 
    /// otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns>A nullable <see langword="bool"/> indicating the result of the parsing operation:  <see langword="true"/> if
    /// parsing succeeds, <see langword="false"/> if parsing fails, or <see langword="null"/>  if the parsing result is
    /// indeterminate.</returns>
    bool TryParse(string input, out T? parsed);
  }
}
