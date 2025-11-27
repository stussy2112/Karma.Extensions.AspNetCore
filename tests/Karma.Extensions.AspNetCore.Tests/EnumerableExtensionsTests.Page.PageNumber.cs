// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.Page.PageNumber.cs" company="Karma, LLC">
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
    // ========== Page<T>(IEnumerable<T>?, int pageNumber, int pageSize) Tests ==========

    [TestMethod]
    public void When_source_is_null_Page_with_pageNumber_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      int pageNumber = 1;
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_pageSize_is_zero_Page_with_pageNumber_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      int pageNumber = 1;
      int pageSize = 0;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_pageSize_is_negative_Page_with_pageNumber_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      int pageNumber = 1;
      int pageSize = -5;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_pageNumber_is_zero_Page_with_pageNumber_defaults_to_page_one()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 0;
      int pageSize = 3;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }

    [TestMethod]
    public void When_pageNumber_is_negative_Page_with_pageNumber_defaults_to_page_one()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = -3;
      int pageSize = 2;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_first_page_requested_Page_with_pageNumber_returns_first_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = 3;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }

    [TestMethod]
    public void When_middle_page_requested_Page_with_pageNumber_returns_correct_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 3;
      int pageSize = 3;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(7, resultList[0].Id); // (3-1)*3 + 1 = 7
      Assert.AreEqual(8, resultList[1].Id);
      Assert.AreEqual(9, resultList[2].Id);
    }

    [TestMethod]
    public void When_last_page_requested_Page_with_pageNumber_returns_remaining_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 4;
      int pageSize = 3;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList); // Only 1 item left
      Assert.AreEqual(10, resultList[0].Id);
    }

    [TestMethod]
    public void When_page_beyond_available_items_Page_with_pageNumber_returns_empty()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 10;
      int pageSize = 5;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_pageSize_equals_one_Page_with_pageNumber_returns_single_item()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 5;
      int pageSize = 1;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(5, resultList[0].Id);
    }

    [TestMethod]
    public void When_pageSize_larger_than_source_Page_with_pageNumber_returns_all_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = 100;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(5, resultList[4].Id);
    }

    [TestMethod]
    public void When_source_is_empty_Page_with_pageNumber_returns_empty()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      int pageNumber = 1;
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_large_page_number_requested_Page_with_pageNumber_calculates_offset_correctly()
    {
      // Arrange
      var source = Enumerable.Range(1, 1000).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 50;
      int pageSize = 20;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(20, resultList);
      int expectedFirstId = ((pageNumber - 1) * pageSize) + 1;
      Assert.AreEqual(expectedFirstId, resultList[0].Id); // (50-1)*20+1 = 981
      Assert.AreEqual(expectedFirstId + 19, resultList[19].Id); // 1000
    }

    [TestMethod]
    public void When_page_boundary_exact_Page_with_pageNumber_returns_exact_page()
    {
      // Arrange - 30 items, page size 10 = exactly 3 pages
      var source = Enumerable.Range(1, 30).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 3;
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(10, resultList);
      Assert.AreEqual(21, resultList[0].Id);
      Assert.AreEqual(30, resultList[9].Id);
    }

    [TestMethod]
    public void When_both_source_and_pageSize_zero_Page_with_pageNumber_returns_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      int pageNumber = 1;
      int pageSize = 0;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_pagination_at_page_two_Page_with_pageNumber_skips_first_page()
    {
      // Arrange
      var source = Enumerable.Range(1, 20).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 2;
      int pageSize = 5;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
      Assert.AreEqual(6, resultList[0].Id);
      Assert.AreEqual(7, resultList[1].Id);
      Assert.AreEqual(8, resultList[2].Id);
      Assert.AreEqual(9, resultList[3].Id);
      Assert.AreEqual(10, resultList[4].Id);
    }

    [TestMethod]
    public void When_using_with_queryable_Page_with_pageNumber_works_correctly()
    {
      // Arrange
      IQueryable<TestEntity> source = Enumerable.Range(1, 50).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).AsQueryable();
      int pageNumber = 3;
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(10, resultList);
      Assert.AreEqual(21, resultList[0].Id);
      Assert.AreEqual(30, resultList[9].Id);
    }

    [TestMethod]
    public void When_deferred_execution_needed_Page_with_pageNumber_supports_lazy_evaluation()
    {
      // Arrange
      int accessCount = 0;
      IEnumerable<TestEntity> source = Enumerable.Range(1, 100).Select((i) =>
      {
        accessCount++;
        return new TestEntity { Id = i, Name = $"N{i}", Value = i };
      });

      int pageNumber = 2;
      int pageSize = 5;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert before enumeration
      Assert.AreEqual(0, accessCount); // Should not have accessed source yet

      // Enumerate
      var resultList = result!.ToList();

      // Assert after enumeration
      Assert.IsGreaterThan(0, accessCount); // Should have accessed items
      Assert.HasCount(5, resultList);
      Assert.AreEqual(6, resultList[0].Id);
    }

    [TestMethod]
    public void When_pageSize_is_max_int_Page_with_pageNumber_handles_gracefully()
    {
      // Arrange
      var source = Enumerable.Range(1, 100).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = int.MaxValue;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(100, resultList);
    }

    [TestMethod]
    public void When_arithmetic_overflow_would_occur_Page_with_pageNumber_throws_OverflowException()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = int.MaxValue;
      int pageSize = 2;

      // Act & Assert
      _ = Assert.ThrowsExactly<OverflowException>(() =>
      {
        _ = source.Page(pageNumber, pageSize).ToList();
      });
    }

    [TestMethod]
    public void When_page_calculation_near_overflow_boundary_Page_with_pageNumber_works_correctly()
    {
      // Arrange
      var source = Enumerable.Range(1, 100).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = int.MaxValue / 100; // Large but safe page number
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert - Should return empty since offset exceeds source
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_multiple_pages_enumerated_sequentially_Page_with_pageNumber_returns_correct_data()
    {
      // Arrange
      var source = Enumerable.Range(1, 15).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageSize = 5;

      // Act - Get all pages
      var page1 = source.Page(1, pageSize).ToList();
      var page2 = source.Page(2, pageSize).ToList();
      var page3 = source.Page(3, pageSize).ToList();

      // Assert
      Assert.HasCount(5, page1);
      Assert.AreEqual(1, page1[0].Id);
      Assert.AreEqual(5, page1[4].Id);

      Assert.HasCount(5, page2);
      Assert.AreEqual(6, page2[0].Id);
      Assert.AreEqual(10, page2[4].Id);

      Assert.HasCount(5, page3);
      Assert.AreEqual(11, page3[0].Id);
      Assert.AreEqual(15, page3[4].Id);
    }

    [TestMethod]
    public void When_paging_with_complex_objects_Page_with_pageNumber_maintains_object_integrity()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "Alice", Value = 10.5 },
        new TestEntity { Id = 2, Name = "Bob", Value = 20.0 },
        new TestEntity { Id = 3, Name = "Charlie", Value = 15.0 },
        new TestEntity { Id = 4, Name = "David", Value = 25.5 },
        new TestEntity { Id = 5, Name = "Eve", Value = 30.0 }
      };
      int pageNumber = 2;
      int pageSize = 2;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual("Charlie", resultList[0].Name);
      Assert.AreEqual(15.0, resultList[0].Value);
      Assert.AreEqual(4, resultList[1].Id);
      Assert.AreEqual("David", resultList[1].Name);
      Assert.AreEqual(25.5, resultList[1].Value);
    }

    [TestMethod]
    public void When_source_contains_nulls_Page_with_pageNumber_handles_correctly()
    {
      // Arrange
      var source = new List<TestEntityWithNullableString>
      {
        new TestEntityWithNullableString { Id = 1, NullableName = "Alice" },
        new TestEntityWithNullableString { Id = 2, NullableName = null },
        new TestEntityWithNullableString { Id = 3, NullableName = "Bob" },
        new TestEntityWithNullableString { Id = 4, NullableName = null },
        new TestEntityWithNullableString { Id = 5, NullableName = "Charlie" }
      };
      int pageNumber = 2;
      int pageSize = 2;

      // Act
      IEnumerable<TestEntityWithNullableString>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual("Bob", resultList[0].NullableName);
      Assert.AreEqual(4, resultList[1].Id);
      Assert.IsNull(resultList[1].NullableName);
    }

    [TestMethod]
    public void When_combining_with_linq_methods_Page_with_pageNumber_integrates_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = Enumerable.Range(1, 100)
        .Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i })
        .Where((e) => e.Id % 2 == 0); // Even IDs only

      int pageNumber = 2;
      int pageSize = 5;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
      Assert.AreEqual(12, resultList[0].Id); // 6th even number
      Assert.AreEqual(14, resultList[1].Id);
      Assert.AreEqual(16, resultList[2].Id);
      Assert.AreEqual(18, resultList[3].Id);
      Assert.AreEqual(20, resultList[4].Id);
    }

    [TestMethod]
    public void When_page_one_with_size_one_Page_with_pageNumber_returns_first_item_only()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = 1;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(1, resultList[0].Id);
    }

    [TestMethod]
    public void When_pageSize_equals_source_count_Page_with_pageNumber_returns_all_on_page_one()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = 10;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(10, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(10, resultList[9].Id);
    }

    [TestMethod]
    public void When_very_large_pageSize_Page_with_pageNumber_returns_all_remaining_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 50).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      int pageNumber = 1;
      int pageSize = 1000000;

      // Act
      IEnumerable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(50, resultList);
    }
  }
}
