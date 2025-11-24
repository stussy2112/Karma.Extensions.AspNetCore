// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.Page.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class EnumerableExtensionsTests
  {
    // ========== Page<T>(IEnumerable<T>, PageInfo) Tests ==========

    [TestMethod]
    public void When_source_is_null_Page_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      var pageInfo = new PageInfo(0u, 10u);

      // Act
      IEnumerable<TestEntity>? result = source!.Page(pageInfo);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_pageInfo_is_null_Page_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      PageInfo? pageInfo = null;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_both_source_and_pageInfo_are_null_Page_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      PageInfo? pageInfo = null;

      // Act
      IEnumerable<TestEntity>? result = source!.Page(pageInfo);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_pageInfo_has_offset_and_limit_Page_returns_paginated_subset()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(2u, 3u); // offset=2, limit=3

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(4, resultList[1].Id);
      Assert.AreEqual(5, resultList[2].Id);
    }

    [TestMethod]
    public void When_offset_is_zero_Page_starts_from_beginning()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, 2u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_limit_exceeds_available_items_Page_returns_remaining_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, 100u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
    }

    [TestMethod]
    public void When_offset_exceeds_source_count_Page_returns_empty_sequence()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(100u, 10u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_source_is_empty_Page_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      var pageInfo = new PageInfo(0u, 10u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_limit_is_one_Page_returns_single_item()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(5u, 1u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(6, resultList[0].Id);
    }

    // ========== Page<T, TValue>(IEnumerable<T>, PageInfo, Func<T, TValue?>) Tests - Reference Type ==========

    [TestMethod]
    public void When_cursor_property_is_null_Page_with_cursor_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var pageInfo = new PageInfo(after: "a", limit: 10u);
      Func<TestEntity, string?>? cursorProperty = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source.Page(pageInfo, cursorProperty!);
      });
    }

    [TestMethod]
    public void When_source_is_null_Page_with_cursor_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      var pageInfo = new PageInfo(after: "a", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = source!.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_pageInfo_is_null_Page_with_cursor_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      PageInfo? pageInfo = null;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_using_after_cursor_Page_returns_items_after_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 2, Name = "bob", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "bob", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("charlie", resultList[0].Name);
    }

    [TestMethod]
    public void When_using_before_cursor_Page_returns_items_before_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 2, Name = "bob", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "charlie" };

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("alice", resultList[0].Name);
      Assert.AreEqual("bob", resultList[1].Name);
    }

    [TestMethod]
    public void When_cursor_limit_is_specified_Page_respects_limit()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 },
        new TestEntity { Id = 5, Name = "e", Value = 5 }
      };
      var pageInfo = new PageInfo(after: "b", limit: 2u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("c", resultList[0].Name);
      Assert.AreEqual("d", resultList[1].Name);
    }

    [TestMethod]
    public void When_cursor_not_found_Page_returns_items_after_cursor_value()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "c", Value = 2 },
        new TestEntity { Id = 3, Name = "e", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "b", limit: 10u); // 'b' doesn't exist

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Should return items after 'b' in sorted order: 'c', 'e'
      Assert.HasCount(2, resultList);
      Assert.AreEqual("c", resultList[0].Name);
      Assert.AreEqual("e", resultList[1].Name);
    }

    [TestMethod]
    public void When_no_cursor_provided_Page_returns_first_page_ordered_by_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 2, Name = "bob", Value = 2 }
      };
      var pageInfo = new PageInfo(after: "x", limit: 2u) { After = "" };

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      // Should be ordered by Name and limited to 2
      Assert.AreEqual("alice", resultList[0].Name);
      Assert.AreEqual("bob", resultList[1].Name);
    }

    // ========== Page<T, TValue>(IEnumerable<T>, PageInfo, Func<T, TValue?>) Tests - Struct Type ==========

    [TestMethod]
    public void When_cursor_property_is_null_for_struct_Page_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var pageInfo = new PageInfo(after: "5", limit: 10u);
      Func<TestEntity, int?>? cursorProperty = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source.Page(pageInfo, cursorProperty!);
      });
    }

    [TestMethod]
    public void When_source_is_null_for_struct_cursor_Page_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      var pageInfo = new PageInfo(after: "5", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = source!.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_pageInfo_is_null_for_struct_cursor_Page_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      PageInfo? pageInfo = null;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_using_after_cursor_with_int_Page_returns_items_after_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 }
      };
      var pageInfo = new PageInfo(after: "2", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(4, resultList[1].Id);
    }

    [TestMethod]
    public void When_using_before_cursor_with_int_Page_returns_items_before_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "3" };

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_struct_cursor_has_limit_Page_respects_limit()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(after: "3", limit: 3u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(4, resultList[0].Id);
      Assert.AreEqual(5, resultList[1].Id);
      Assert.AreEqual(6, resultList[2].Id);
    }

    [TestMethod]
    public void When_struct_cursor_invalid_Page_returns_first_page_ordered()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 }
      };
      var pageInfo = new PageInfo(after: "invalid", limit: 2u);

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      // Should be ordered by Id
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_nullable_int_cursor_with_nulls_Page_excludes_nulls()
    {
      // Arrange
      var source = new List<TestEntityWithNullableInt>
      {
        new TestEntityWithNullableInt { Id = 1, NullableValue = 10 },
        new TestEntityWithNullableInt { Id = 2, NullableValue = null },
        new TestEntityWithNullableInt { Id = 3, NullableValue = 20 },
        new TestEntityWithNullableInt { Id = 4, NullableValue = 5 }
      };
      var pageInfo = new PageInfo(after: "8", limit: 10u);

      // Act
      IEnumerable<TestEntityWithNullableInt>? result = source.Page(pageInfo, (item) => item.NullableValue);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(10, resultList[0].NullableValue);
      Assert.AreEqual(20, resultList[1].NullableValue);
    }

    [TestMethod]
    public void When_both_before_and_after_provided_Page_before_takes_precedence()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 }
      };
      var pageInfo = new PageInfo(after: "a", limit: 10u) { Before = "d" };

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageInfo, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // 'before' takes precedence, so should return items before 'd'
      Assert.HasCount(3, resultList);
      Assert.AreEqual("a", resultList[0].Name);
      Assert.AreEqual("b", resultList[1].Name);
      Assert.AreEqual("c", resultList[2].Name);
    }
  }
}
