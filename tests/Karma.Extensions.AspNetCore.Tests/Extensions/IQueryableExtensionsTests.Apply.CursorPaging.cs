// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Apply.CursorPaging.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class IQueryableExtensionsTests
  {
    private sealed record CursorItem
    {
      public int? Id { get; init; }
      public DateTime? Ts { get; init; }
    }

    [TestMethod]
    public void When_SourceIsNull_ReturnsNull()
    {
      // Arrange
      IQueryable<CursorItem>? source = null;
      var pageInfo = new PageInfo(0, 10);

      // Act
      IQueryable<CursorItem>? result = pageInfo.Apply(source, (i) => i.Id);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_PageInfoIsNull_ReturnsSourceUnchanged()
    {
      // Arrange
      IQueryable<CursorItem> source = new List<CursorItem> { new CursorItem { Id = 1 } }.AsQueryable();
      PageInfo? pageInfo = null;

      // Act
      IQueryable<CursorItem> result = pageInfo.Apply(source, (i) => i.Id);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_NoCursors_OrdersAscendingAndTakesLimit()
    {
      // Arrange
      var items = new List<CursorItem>
      {
        new CursorItem { Id = 5 },
        new CursorItem { Id = 2 },
        new CursorItem { Id = 4 },
        new CursorItem { Id = 1 },
        new CursorItem { Id = 3 }
      };

      IQueryable<CursorItem> source = items.AsQueryable();
      var pageInfo = new PageInfo(0, 3);

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, result.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public void When_AfterCursor_FiltersGreaterThanAndOrdersAscending()
    {
      // Arrange
      var items = new List<CursorItem>
      {
        new CursorItem { Id = 1 },
        new CursorItem { Id = 2 },
        new CursorItem { Id = 3 },
        new CursorItem { Id = 4 },
        new CursorItem { Id = 5 }
      };

      IQueryable<CursorItem> source = items.AsQueryable();
      var pageInfo = new PageInfo(after: "2", limit: 2); // after 2 -> expect 3,4

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      int[] expected = [3, 4];
      CollectionAssert.AreEqual(expected, result.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public void When_BeforeCursor_FiltersLessThanAndOrdersDescending()
    {
      // Arrange
      var items = new List<CursorItem>
      {
        new CursorItem { Id = 1 },
        new CursorItem { Id = 2 },
        new CursorItem { Id = 3 },
        new CursorItem { Id = 4 },
        new CursorItem { Id = 5 }
      };

      IQueryable<CursorItem> source = items.AsQueryable();
      // Create PageInfo with only Before set (use default ctor and init properties)
      var pageInfo = new PageInfo() { Before = "4", Limit = 2 };

      // Act
      var result = pageInfo.Apply(source, (i) => i.Id)!.ToList();

      // Assert
      // Expect items with Id < 4 -> 1,2,3 ; OrderByDescending -> 3,2 ; Take 2 -> 3,2
      int[] expected = [3, 2];
      CollectionAssert.AreEqual(expected, result.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public void When_DateTimeCursor_After_WorksCorrectly()
    {
      // Arrange
      var baseTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
      var items = new List<CursorItem>
      {
        new CursorItem { Ts = baseTime.AddDays(-2) },
        new CursorItem { Ts = baseTime.AddDays(-1) },
        new CursorItem { Ts = baseTime },
        new CursorItem { Ts = baseTime.AddDays(1) },
        new CursorItem { Ts = baseTime.AddDays(2) }
      };

      IQueryable<CursorItem> source = items.AsQueryable();
      // after baseTime -> expect items with Ts > baseTime -> 1 and 2 days
      var pageInfo = new PageInfo(after: baseTime.ToString("o"), limit: 2);

      // Act
      var result = pageInfo.Apply(source, (i) => i.Ts)!.ToList();

      // Assert
      Assert.HasCount(2, result);
      Assert.IsTrue(result.All(x => x.Ts > baseTime));
      CollectionAssert.AreEqual(new[] { baseTime.AddDays(1), baseTime.AddDays(2) }, result.Select(x => x.Ts).ToArray());
    }
  }
}
