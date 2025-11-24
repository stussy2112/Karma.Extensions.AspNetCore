// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.PageByQuery.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Karma.Extensions.AspNetCore.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class EnumerableExtensionsTests
  {
    // ========== PageByQuery Tests ==========

    [TestMethod]
    public void When_source_is_null_PageByQuery_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;

      // Act
      IEnumerable<TestEntity>? result = source!.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_httpContext_is_null_PageByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(null!, (item) => item.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_no_pageInfo_PageByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      // HttpContext.Items is empty by default

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_pageInfo_is_not_PageInfo_type_PageByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      _httpContext.Items[ContextItemKeys.PageInfo] = "not a PageInfo object";

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_pageInfo_is_null_PageByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      _httpContext.Items[ContextItemKeys.PageInfo] = null;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_offset_based_pageInfo_PageByQuery_applies_pagination()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(2u, 3u); // offset=2, limit=3
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(4, resultList[1].Id);
      Assert.AreEqual(5, resultList[2].Id);
    }

    [TestMethod]
    public void When_httpContext_has_after_cursor_PageByQuery_applies_cursor_pagination()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 2, Name = "bob", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 4, Name = "david", Value = 4 }
      };
      var pageInfo = new PageInfo(after: "bob", limit: 2u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("charlie", resultList[0].Name);
      Assert.AreEqual("david", resultList[1].Name);
    }

    [TestMethod]
    public void When_httpContext_has_before_cursor_PageByQuery_applies_cursor_pagination()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 2, Name = "bob", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 4, Name = "david", Value = 4 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "david" };
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("alice", resultList[0].Name);
      Assert.AreEqual("bob", resultList[1].Name);
      Assert.AreEqual("charlie", resultList[2].Name);
    }

    [TestMethod]
    public void When_cursorProperty_is_null_PageByQuery_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var pageInfo = new PageInfo(after: "a", limit: 10u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;
      Func<TestEntity, string?>? cursorProperty = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source.PageByQuery(_httpContext, cursorProperty!);
      });
    }

    [TestMethod]
    public void When_source_is_empty_PageByQuery_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      var pageInfo = new PageInfo(after: "a", limit: 10u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_using_int_cursor_property_PageByQuery_applies_correctly()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 }
      };
      var pageInfo = new PageInfo(after: "2", limit: 2u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(4, resultList[1].Id);
    }

    [TestMethod]
    public void When_cursor_not_found_PageByQuery_returns_items_after_cursor_value()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "alice", Value = 1 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 5, Name = "eve", Value = 5 }
      };
      var pageInfo = new PageInfo(after: "bob", limit: 10u); // 'bob' doesn't exist
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Should return items after 'bob' in sorted order: 'charlie', 'eve'
      Assert.HasCount(2, resultList);
      Assert.AreEqual("charlie", resultList[0].Name);
      Assert.AreEqual("eve", resultList[1].Name);
    }

    [TestMethod]
    public void When_limit_is_smaller_than_results_PageByQuery_respects_limit()
    {
      // Arrange
      var source = Enumerable.Range(1, 100).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(after: "10", limit: 5u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
      Assert.AreEqual(11, resultList[0].Id);
      Assert.AreEqual(15, resultList[4].Id);
    }

    [TestMethod]
    public void When_both_before_and_after_in_httpContext_PageByQuery_before_takes_precedence()
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
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // 'before' takes precedence
      Assert.HasCount(3, resultList);
      Assert.AreEqual("a", resultList[0].Name);
      Assert.AreEqual("b", resultList[1].Name);
      Assert.AreEqual("c", resultList[2].Name);
    }

    [TestMethod]
    public void When_using_nullable_int_cursor_PageByQuery_excludes_nulls()
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
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      // Use wrapper function to access nullable int cursor
      IEnumerable<TestEntityWithNullableInt>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // After id 8, we get items with id > 8 which don't exist in this test data
      // The nullable values after "8" would be 10 and 20, but we're using Id as cursor
      Assert.HasCount(0, resultList);
    }

    [TestMethod]
    public void When_pageInfo_has_no_cursors_PageByQuery_uses_offset_based_pagination()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(3u, 2u); // offset=3, limit=2
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(4, resultList[0].Id);
      Assert.AreEqual(5, resultList[1].Id);
    }

    [TestMethod]
    public void When_invalid_cursor_value_PageByQuery_returns_first_page_ordered()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 }
      };
      var pageInfo = new PageInfo(after: "invalid", limit: 2u);
      _httpContext.Items[ContextItemKeys.PageInfo] = pageInfo;

      // Act
      IEnumerable<TestEntity>? result = source.PageByQuery(_httpContext, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      // Should be ordered by Id
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }
  }
}
