// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Apply.CursorPaging.Extras.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class IQueryableExtensionsTests
  {
    private sealed record Item
    {
      public int? Id { get; init; }

      public string? Name { get; init; }
    }

    [TestMethod]
    public void When_BothBeforeAndAfter_BeforeTakesPrecedence()
    {
      // Arrange
      var items = Enumerable.Range(1, 6).Select(i => new Item { Id = i }).ToList();
      IQueryable<Item> source = items.AsQueryable();

      // Both cursors present - before should take precedence
      PageInfo pageInfo = new PageInfo(after: "2", limit: 3) with { Before = "5" };

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      // Before = 5 -> select items with Id < 5 -> 1,2,3,4 ; OrderByDescending -> 4,3,2 ; Take 3 -> 4,3,2
      int[] expected = [4, 3, 2];
      CollectionAssert.AreEqual(expected, result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void When_StringCursor_ComparisonUsesStringCompare()
    {
      // Arrange
      List<Item> items =
      [
        new Item { Name = "apple" },
        new Item { Name = "banana" },
        new Item { Name = "cherry" },
        new Item { Name = "date" },
        new Item { Name = "elderberry" }
      ];

      IQueryable<Item> source = items.AsQueryable();
      // after "banana" -> expect names that compare greater than "banana"
      PageInfo pageInfo = new (after: "banana", limit: 2);

      // Act
      var result = pageInfo.Apply(source, (i) => i.Name)!.ToList();

      // Assert
      string[] expected = ["cherry", "date"];
      CollectionAssert.AreEqual(expected, result.Select(x => x.Name).ToArray());
    }

    [TestMethod]
    public void When_NullableIntCursor_AfterFiltersCorrectly()
    {
      // Arrange
      List<Item> items =
      [
        new Item { Id = null },
        new Item { Id = 1 },
        new Item { Id = 2 },
        new Item { Id = 3 }
      ];

      IQueryable<Item> source = items.AsQueryable();
      PageInfo pageInfo = new ("1", limit: 10);

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      // after 1 -> items with Id > 1 -> 2,3
      CollectionAssert.AreEqual(new int?[] { 2, 3 }, result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void When_LimitIsOne_TakesSingleItem()
    {
      // Arrange
      var items = Enumerable.Range(1, 5).Select(i => new Item { Id = i }).ToList();
      IQueryable<Item> source = items.AsQueryable();
      PageInfo pageInfo = new (after: "2", limit: 1);

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      CollectionAssert.AreEqual(new int?[] { 3 }, result.Select(x => x.Id).ToArray());
    }
  }
}
