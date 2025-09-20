// -----------------------------------------------------------------------
// <copyright file="PageInfo.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Karma.Extensions.AspNetCore
{
  /// <summary>
  /// Represents page information for a set of data.
  /// </summary>
  public sealed record PageInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PageInfo"/> class with pagination details.
    /// </summary>
    /// <remarks>If <paramref name="offset"/> is less than 0, it will be set to 0. If <paramref name="limit"/>
    /// is less than 1, it will be set to <see cref="uint.MaxValue"/>.</remarks>
    /// <param name="after">The cursor indicating the position after which to retrieve items. Defaults to an empty string.</param>
    /// <param name="before">The cursor indicating the position before which to retrieve items. Defaults to an empty string.</param>
    /// <param name="offset">The zero-based index of the first item to retrieve. Must be 0 or greater. Defaults to 0.</param>
    /// <param name="limit">The maximum number of items to retrieve. Must be 1 or greater. Defaults to <see cref="uint.MaxValue"/>.</param>
    public PageInfo(string after = "", string before = "", uint offset = 0, uint limit = uint.MaxValue) =>
      (After, Before, Offset, Limit) = (after, before, offset, limit < 1 ? uint.MaxValue : limit);

    /// <summary>
    /// Instantiates a new <see cref="PageInfo"/> instance.
    /// </summary>
    /// <param name="after">The cursor/identifier of the item that is the start of the "page" of items.</param>
    /// <param name="limit">The limit of items to be queried.</param>
    public PageInfo(string after, uint limit = uint.MaxValue)
      : this(after, string.Empty, 1, limit: limit)
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="PageInfo"/> instance.
    /// </summary>
    /// <param name="offset">The page of the items.</param>
    /// <param name="limit">The limit of items to be queried.</param>
    public PageInfo(uint offset, uint limit = uint.MaxValue)
      : this(string.Empty, string.Empty, offset, limit)
    {
    }

    /// <summary>
    /// The cursor/identifier of the item that is the start of the "page" of items.
    /// </summary>
    public string After
    {
      get;
    }

    /// <summary>
    /// The cursor/identifier of the item that is the end of the "page" of items.
    /// </summary>
    public string Before
    {
      get;
    }

    /// <summary>
    /// The limit of items to be queried
    /// </summary>
    public uint Limit
    {
      get;
    } = uint.MaxValue;

    /// <summary>
    /// The page of the items.
    /// </summary>
    public uint Offset
    {
      get;
    } = 0;

  }
}
