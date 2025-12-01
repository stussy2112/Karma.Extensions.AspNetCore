// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Apply.OffsetPaging.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class IQueryableExtensionsTests
  {
    private sealed record Entity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
    }

    [TestMethod]
    public void When_Offset_SourceIsNull_ReturnsNull()
    {
      IQueryable<Entity>? source = null;
      var pageInfo = new PageInfo(0, 10);

      IQueryable<Entity>? result = pageInfo.Apply(source);

      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_Offset_PageInfoIsNull_ReturnsSourceUnchanged()
    {
      var list = new List<Entity> { new Entity { Id = 1 } };
      IQueryable<Entity> source = list.AsQueryable();
      PageInfo? pageInfo = null;

      IQueryable<Entity> result = pageInfo.Apply(source);

      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void OffsetAndLimit_AppliesSkipAndTake()
    {
      var items = Enumerable.Range(1, 10).Select(i => new Entity { Id = i }).ToList();
      IQueryable<Entity> source = items.AsQueryable();

      // offset = 2 (zero-based) -> skip first two items (1,2) -> start at 3; take 3 -> 3,4,5
      var pageInfo = new PageInfo(offset: 2u, limit: 3u);

      var result = pageInfo.Apply(source)!.ToList();

      int[] expected = [3, 4, 5];
      CollectionAssert.AreEqual(expected, result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void OffsetBeyondCount_ReturnsEmpty()
    {
      var items = Enumerable.Range(1, 5).Select(i => new Entity { Id = i }).ToList();
      IQueryable<Entity> source = items.AsQueryable();

      var pageInfo = new PageInfo(offset: 100u, limit: 10u);

      var result = pageInfo.Apply(source)!.ToList();

      Assert.IsEmpty(result);
    }

    [TestMethod]
    public void LimitLarge_IsCappedToIntMaxValue_ButDoesNotAffectSmallSequences()
    {
      var items = Enumerable.Range(1, 50).Select(i => new Entity { Id = i }).ToList();
      IQueryable<Entity> source = items.AsQueryable();

      // Use a very large limit (uint.MaxValue) which would be capped to int.MaxValue internally
      var pageInfo = new PageInfo(offset: 10u, limit: uint.MaxValue);

      var result = pageInfo.Apply(source)!.ToList();

      // Should return elements from 11..50
      CollectionAssert.AreEqual(Enumerable.Range(11, 40).ToArray(), result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void NegativeOffsetHandledViaPageInfoConstructors_DefaultsToZero()
    {
      // PageInfo uses uint for offset; negative offset cannot be represented. This test verifies behavior when offset is zero.
      var items = Enumerable.Range(1, 5).Select(i => new Entity { Id = i }).ToList();
      IQueryable<Entity> source = items.AsQueryable();

      var pageInfo = new PageInfo(offset: 0u, limit: 2u);
      var result = pageInfo.Apply(source)!.ToList();

      int[] expected = [1, 2];
      CollectionAssert.AreEqual(expected, result.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void MultipleSequentialPages_WillReturnDistinctSlices()
    {
      var items = Enumerable.Range(1, 9).Select(i => new Entity { Id = i }).ToList();
      IQueryable<Entity> source = items.AsQueryable();

      var page1 = new PageInfo(offset: 0u, limit: 3u);
      var page2 = new PageInfo(offset: 3u, limit: 3u);
      var page3 = new PageInfo(offset: 6u, limit: 3u);

      int[] r1 = [.. page1.Apply(source)!.Select(x => x.Id)];
      int[] r2 = [.. page2.Apply(source)!.Select(x => x.Id)];
      int[] r3 = [.. page3.Apply(source)!.Select(x => x.Id)];

      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, r1);
      expected = [4, 5, 6];
      CollectionAssert.AreEqual(expected, r2);
      expected = [7, 8, 9];
      CollectionAssert.AreEqual(expected, r3);
    }
  }
}
