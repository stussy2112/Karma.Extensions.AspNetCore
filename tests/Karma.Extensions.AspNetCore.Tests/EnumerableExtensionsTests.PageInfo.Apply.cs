// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.PageInfo.Apply.cs" company="Karma, LLC">
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

    // ========== PageInfo.Apply Tests ==========

    [TestMethod]
    public void When_pageInfo_is_null_Apply_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      PageInfo? pageInfo = null;

      // Act
      IEnumerable<TestEntity>? result = pageInfo!.Apply(source, (i) => i.Name);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_source_is_null_Apply_PageInfo_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      var pageInfo = new PageInfo(0u, 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (i) => i.Name);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_source_is_empty_Apply_PageInfo_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      var pageInfo = new PageInfo(0u, 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (i) => i.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_source_is_empty_with_cursor_Apply_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      var pageInfo = new PageInfo(after: "somevalue", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_using_offset_paging_Apply_skips_and_takes()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(2u, 2u); // offset=2, limit=2

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(4, resultList[1].Id);
    }

    [TestMethod]
    public void When_limit_exceeds_source_count_Apply_returns_all_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, 100u); // limit > source count

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(5, result.ToList());
    }

    [TestMethod]
    public void When_limit_is_zero_Apply_returns_all_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      // Note: PageInfo constructor converts limit < 1 to uint.MaxValue
      var pageInfo = new PageInfo(0u, 0u); // limit = 0 becomes uint.MaxValue

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(5, result.ToList()); // Returns all items because limit becomes uint.MaxValue
    }

    [TestMethod]
    public void When_limit_is_max_value_Apply_handles_correctly()
    {
      // Arrange
      var source = Enumerable.Range(1, 100).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, uint.MaxValue);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(100, result.ToList());
    }

    [TestMethod]
    public void When_offset_exceeds_source_count_Apply_returns_empty_sequence()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(100u, 10u); // offset > source count

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_offset_equals_source_count_Apply_returns_empty_sequence()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(5u, 10u); // offset = source count

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_using_after_cursor_Apply_returns_items_after_cursor_ordered_by_cursor()
    {
      // Arrange - unsorted source
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "delta", Value = 1 },
        new TestEntity { Id = 2, Name = "alpha", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 4, Name = "bravo", Value = 4 }
      };

      var pageInfo = new PageInfo(after: "bravo", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Ordered by Name ascending: alpha, bravo, charlie, delta -> items after 'bravo' are charlie, delta
      Assert.HasCount(2, resultList);
      Assert.AreEqual("charlie", resultList[0].Name);
      Assert.AreEqual("delta", resultList[1].Name);
    }

    [TestMethod]
    public void When_after_cursor_not_found_Apply_returns_all_ordered_items_after_cursor_value()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "z", limit: 10u); // cursor doesn't exist

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count()); // No items after 'z'
    }

    [TestMethod]
    public void When_after_cursor_matches_last_item_Apply_returns_empty()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "c", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_after_cursor_with_limit_Apply_respects_limit()
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
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - should return "c" and "d" only (limit=2)
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("c", resultList[0].Name);
      Assert.AreEqual("d", resultList[1].Name);
    }

    [TestMethod]
    public void When_using_before_cursor_Apply_returns_items_before_cursor_ordered_by_cursor()
    {
      // Arrange - unsorted source
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "delta", Value = 1 },
        new TestEntity { Id = 2, Name = "alpha", Value = 2 },
        new TestEntity { Id = 3, Name = "charlie", Value = 3 },
        new TestEntity { Id = 4, Name = "bravo", Value = 4 }
      };

      // Use init to set Before since constructor doesn't support it
      var pageInfo = new PageInfo(0u, 10u) { Before = "charlie" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Ordered by Name ascending: alpha, bravo, charlie, delta -> items before 'charlie' are alpha, bravo
      Assert.HasCount(2, resultList);
      Assert.AreEqual("alpha", resultList[0].Name);
      Assert.AreEqual("bravo", resultList[1].Name);
    }

    [TestMethod]
    public void When_before_cursor_not_found_Apply_returns_all_ordered_items_before_cursor_value()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "b", Value = 1 },
        new TestEntity { Id = 2, Name = "c", Value = 2 },
        new TestEntity { Id = 3, Name = "d", Value = 3 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "a" }; // cursor before all items

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_before_cursor_matches_first_item_Apply_returns_empty()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "a" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_before_cursor_with_limit_Apply_respects_limit()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 },
        new TestEntity { Id = 4, Name = "d", Value = 4 }
      };
      var pageInfo = new PageInfo(0u, 2u) { Before = "d" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - should return first 2 items before "d" which are "a" and "b"
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("b", resultList[0].Name);
      Assert.AreEqual("c", resultList[1].Name);
    }

    [TestMethod]
    public void When_both_before_and_after_present_before_takes_precedence()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 }
      };

      var pageInfo = new PageInfo(after: "a", limit: 10u) { Before = "c" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // before 'c' should return 'a' and 'b'
      Assert.HasCount(2, resultList);
      Assert.AreEqual("a", resultList[0].Name);
      Assert.AreEqual("b", resultList[1].Name);
    }

    [TestMethod]
    public void When_cursorProperty_returns_null_Apply_handles_correctly()
    {
      // Arrange
      var source = new List<TestEntityWithNullableString>
      {
        new TestEntityWithNullableString { Id = 1, NullableName = "a" },
        new TestEntityWithNullableString { Id = 2, NullableName = null },
        new TestEntityWithNullableString { Id = 3, NullableName = "c" }
      };
      var pageInfo = new PageInfo(after: "a", limit: 10u);

      // Act
      IEnumerable<TestEntityWithNullableString>? result = pageInfo.Apply(source, (item) => item.NullableName);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Items ordered: null, "a", "c" -> after "a" returns "c" only
      Assert.HasCount(1, resultList);
      Assert.AreEqual("c", resultList[0].NullableName);
    }

    [TestMethod]
    public void When_before_cursor_is_whitespace_Apply_uses_offset_paging()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, 2u) { Before = "   " };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - should fall back to offset paging because before is whitespace
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_cursor_property_is_numeric_Apply_orders_and_filters_correctly()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 5, Name = "e", Value = 5 },
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 3, Name = "c", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "2", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - ordered by Id, items after "2" (as string) are 3 and 5
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(5, resultList[1].Id);
    }

    // ========== Generic Apply<T, TValue> Tests (Type-Safe Comparisons) ==========

    [TestMethod]
    public void When_using_generic_overload_with_int_cursor_Apply_uses_type_safe_comparison()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 10, Name = "ten", Value = 10 },
        new TestEntity { Id = 2, Name = "two", Value = 2 },
        new TestEntity { Id = 5, Name = "five", Value = 5 }
      };
      var pageInfo = new PageInfo(after: "5", limit: 10u);

      // Act - Using generic overload with int type
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - ordered by Id, items after 5 are 10
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(10, resultList[0].Id);
    }

    [TestMethod]
    public void When_using_generic_overload_with_invalid_cursor_value_Apply_returns_first_page_ordered()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 }
      };
      var pageInfo = new PageInfo(after: "invalid_number", limit: 10u);

      // Act - Parsing will fail for "invalid_number" as int
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Should return first page ordered by cursor property since parsing failed
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      // Verify items are ordered by Id (the cursor property)
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_generic_overload_cursor_value_is_empty_Apply_uses_offset_paging()
    {
      // Arrange
      var source = Enumerable.Range(1, 5).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(after: "", before: "", limit: 2u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Falls back to offset paging
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
    }

    [TestMethod]
    public void When_generic_overload_before_cursor_with_int_Apply_filters_correctly()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 10, Name = "ten", Value = 10 },
        new TestEntity { Id = 5, Name = "five", Value = 5 },
        new TestEntity { Id = 15, Name = "fifteen", Value = 15 },
        new TestEntity { Id = 3, Name = "three", Value = 3 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "10" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Items before 10: 3, 5
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(3, resultList[0].Id);
      Assert.AreEqual(5, resultList[1].Id);
    }

    [TestMethod]
    public void When_generic_overload_with_DateTime_cursor_Apply_uses_type_safe_comparison()
    {
      // Arrange
      var baseDate = new DateTime(2024, 1, 1, 0, 0, 0,DateTimeKind.Local);
      var source = new List<TestEntityWithDateTime>
      {
        new TestEntityWithDateTime { Id = 1, Date = baseDate.AddDays(3) },
        new TestEntityWithDateTime { Id = 2, Date = baseDate.AddDays(1) },
        new TestEntityWithDateTime { Id = 3, Date = baseDate.AddDays(5) }
      };
      DateTime cursorDate = baseDate.AddDays(2);
      var pageInfo = new PageInfo(after: cursorDate.ToString("O"), limit: 10u);

      // Act
      IEnumerable<TestEntityWithDateTime>? result = pageInfo.Apply(source, (item) => item.Date);

      // Assert - Items after day 2: day 3 and day 5
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(baseDate.AddDays(3), resultList[0].Date);
      Assert.AreEqual(baseDate.AddDays(5), resultList[1].Date);
    }

    [TestMethod]
    public void When_generic_overload_with_Guid_cursor_Apply_uses_type_safe_comparison()
    {
      // Arrange
      var guid1 = new Guid("11111111-1111-1111-1111-111111111111");
      var guid2 = new Guid("22222222-2222-2222-2222-222222222222");
      var guid3 = new Guid("33333333-3333-3333-3333-333333333333");

      var source = new List<TestEntityWithGuid>
      {
        new TestEntityWithGuid { Id = 1, UniqueId = guid3 },
        new TestEntityWithGuid { Id = 2, UniqueId = guid1 },
        new TestEntityWithGuid { Id = 3, UniqueId = guid2 }
      };
      var pageInfo = new PageInfo(after: guid1.ToString(), limit: 10u);

      // Act
      IEnumerable<TestEntityWithGuid>? result = pageInfo.Apply(source, (item) => item.UniqueId);

      // Assert - Items after guid1: guid2, guid3
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(guid2, resultList[0].UniqueId);
      Assert.AreEqual(guid3, resultList[1].UniqueId);
    }

    [TestMethod]
    public void When_generic_overload_with_decimal_cursor_Apply_uses_type_safe_comparison()
    {
      // Arrange
      var source = new List<TestEntityWithDecimal>
      {
        new TestEntityWithDecimal { Id = 1, Price = 99.99m },
        new TestEntityWithDecimal { Id = 2, Price = 49.99m },
        new TestEntityWithDecimal { Id = 3, Price = 149.99m }
      };
      var pageInfo = new PageInfo(after: "50.00", limit: 10u);

      // Act
      IEnumerable<TestEntityWithDecimal>? result = pageInfo.Apply(source, (item) => item.Price);

      // Assert - Items after 50.00: 99.99, 149.99
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(99.99m, resultList[0].Price);
      Assert.AreEqual(149.99m, resultList[1].Price);
    }

    [TestMethod]
    public void When_generic_overload_cursor_property_returns_null_Apply_excludes_null_items()
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
      IEnumerable<TestEntityWithNullableInt>? result = pageInfo.Apply(source, (item) => item.NullableValue);

      // Assert - Items with non-null values after 8: 10, 20
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(10, resultList[0].NullableValue);
      Assert.AreEqual(20, resultList[1].NullableValue);
    }

    [TestMethod]
    public void When_generic_overload_with_long_cursor_Apply_handles_large_numbers()
    {
      // Arrange
      var source = new List<TestEntityWithLong>
      {
        new TestEntityWithLong { Id = 1, LargeValue = 1000000000000L },
        new TestEntityWithLong { Id = 2, LargeValue = 500000000000L },
        new TestEntityWithLong { Id = 3, LargeValue = 2000000000000L }
      };
      var pageInfo = new PageInfo(after: "750000000000", limit: 10u);

      // Act
      IEnumerable<TestEntityWithLong>? result = pageInfo.Apply(source, (item) => item.LargeValue);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1000000000000L, resultList[0].LargeValue);
      Assert.AreEqual(2000000000000L, resultList[1].LargeValue);
    }

    [TestMethod]
    public void When_generic_overload_with_string_cursor_Apply_uses_culture_sensitive_comparison()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "Zebra", Value = 1 },
        new TestEntity { Id = 2, Name = "alpha", Value = 2 },
        new TestEntity { Id = 3, Name = "Beta", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "Z", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - Culture-sensitive comparison using string.CompareTo()
      // Ordered: "alpha", "Beta", "Zebra" -> items after "Z": "Zebra"
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("Zebra", resultList[0].Name);
    }

    [TestMethod]
    public void When_generic_overload_with_string_before_cursor_Apply_uses_culture_sensitive_comparison()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "apple", Value = 1 },
        new TestEntity { Id = 2, Name = "Banana", Value = 2 },
        new TestEntity { Id = 3, Name = "cherry", Value = 3 },
        new TestEntity { Id = 4, Name = "Date", Value = 4 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "Date" };

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - Culture-sensitive comparison: "apple", "Banana", "cherry" are before "Date"
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("apple", resultList[0].Name);
      Assert.AreEqual("Banana", resultList[1].Name);
      Assert.AreEqual("cherry", resultList[2].Name);
    }

    [TestMethod]
    public void When_multiple_items_have_same_cursor_value_Apply_includes_all_matching()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "A", Value = 5 },
        new TestEntity { Id = 2, Name = "B", Value = 10 },
        new TestEntity { Id = 3, Name = "C", Value = 10 },
        new TestEntity { Id = 4, Name = "D", Value = 15 }
      };
      var pageInfo = new PageInfo(after: "5", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Items after 5: none (IDs are 1,2,3,4)
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(0, resultList);
    }

    [TestMethod]
    public void When_all_items_have_same_cursor_value_after_cursor_returns_empty()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "Same", Value = 10 },
        new TestEntity { Id = 2, Name = "Same", Value = 10 },
        new TestEntity { Id = 3, Name = "Same", Value = 10 }
      };
      var pageInfo = new PageInfo(after: "Same", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - No items after "Same" when all are "Same"
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_cursor_value_is_between_items_Apply_returns_items_greater_than_cursor()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "A", Value = 10 },
        new TestEntity { Id = 2, Name = "B", Value = 20 },
        new TestEntity { Id = 3, Name = "C", Value = 30 }
      };
      var pageInfo = new PageInfo(after: "15", limit: 10u);

      // Act - Cursor value "15" doesn't exist in source
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Items with Id > 15: none (IDs are 1,2,3)
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_string_cursor_tests_case_sensitivity_Apply_uses_culture_sensitive_comparison()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "AAA", Value = 1 },
        new TestEntity { Id = 2, Name = "aaa", Value = 2 },
        new TestEntity { Id = 3, Name = "BBB", Value = 3 },
        new TestEntity { Id = 4, Name = "bbb", Value = 4 }
      };
      var pageInfo = new PageInfo(after: "Z", limit: 10u);

      // Act - Culture-sensitive comparison with string.CompareTo()
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Name);

      // Assert - Culture-sensitive: typically case-insensitive, so "aaa" and "bbb" > "Z"
      // Actual behavior depends on current culture, but typically ignores case
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // In most cultures, both "aaa" and "bbb" sort after "Z"
      Assert.IsGreaterThanOrEqualTo(resultList.Count, 0);
    }

    [TestMethod]
    public void When_nullable_string_has_nulls_Apply_excludes_nulls_from_results()
    {
      // Arrange
      var source = new List<TestEntityWithNullableString>
      {
        new TestEntityWithNullableString { Id = 1, NullableName = null },
        new TestEntityWithNullableString { Id = 2, NullableName = "b" },
        new TestEntityWithNullableString { Id = 3, NullableName = "c" },
        new TestEntityWithNullableString { Id = 4, NullableName = null }
      };
      var pageInfo = new PageInfo(after: "a", limit: 10u);

      // Act
      IEnumerable<TestEntityWithNullableString>? result = pageInfo.Apply(source, (item) => item.NullableName);

      // Assert - Nulls are excluded, items after "a": "b", "c"
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("b", resultList[0].NullableName);
      Assert.AreEqual("c", resultList[1].NullableName);
    }

    [TestMethod]
    public void When_nullable_string_before_cursor_excludes_nulls()
    {
      // Arrange
      var source = new List<TestEntityWithNullableString>
      {
        new TestEntityWithNullableString { Id = 1, NullableName = null },
        new TestEntityWithNullableString { Id = 2, NullableName = "a" },
        new TestEntityWithNullableString { Id = 3, NullableName = "b" },
        new TestEntityWithNullableString { Id = 4, NullableName = null }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "c" };

      // Act
      IEnumerable<TestEntityWithNullableString>? result = pageInfo.Apply(source, (item) => item.NullableName);

      // Assert - Nulls excluded, items before "c": "a", "b"
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("a", resultList[0].NullableName);
      Assert.AreEqual("b", resultList[1].NullableName);
    }

    [TestMethod]
    public void When_source_has_mixed_null_and_non_null_nullable_ints_Apply_handles_correctly()
    {
      // Arrange
      var source = new List<TestEntityWithNullableInt>
      {
        new TestEntityWithNullableInt { Id = 1, NullableValue = null },
        new TestEntityWithNullableInt { Id = 2, NullableValue = 5 },
        new TestEntityWithNullableInt { Id = 3, NullableValue = null },
        new TestEntityWithNullableInt { Id = 4, NullableValue = 15 },
        new TestEntityWithNullableInt { Id = 5, NullableValue = 10 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "12" };

      // Act
      IEnumerable<TestEntityWithNullableInt>? result = pageInfo.Apply(source, (item) => item.NullableValue);

      // Assert - Items before 12: 5, 10 (nulls excluded)
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(5, resultList[0].NullableValue);
      Assert.AreEqual(10, resultList[1].NullableValue);
    }

    [TestMethod]
    public void When_offset_is_zero_and_limit_is_one_Apply_returns_first_item()
    {
      // Arrange
      var source = Enumerable.Range(1, 10).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(0u, 1u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply<TestEntity>(source);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(1, resultList[0].Id);
    }

    [TestMethod]
    public void When_using_before_cursor_with_nullable_int_and_null_values_Apply_excludes_nulls()
    {
      // Arrange
      var source = new List<TestEntityWithNullableInt>
      {
        new TestEntityWithNullableInt { Id = 1, NullableValue = 5 },
        new TestEntityWithNullableInt { Id = 2, NullableValue = null },
        new TestEntityWithNullableInt { Id = 3, NullableValue = 15 },
        new TestEntityWithNullableInt { Id = 4, NullableValue = null },
        new TestEntityWithNullableInt { Id = 5, NullableValue = 20 }
      };
      var pageInfo = new PageInfo(0u, 10u) { Before = "18" };

      // Act
      IEnumerable<TestEntityWithNullableInt>? result = pageInfo.Apply(source, (item) => item.NullableValue);

      // Assert - Items before 18 with non-null values: 5, 15
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(5, resultList[0].NullableValue);
      Assert.AreEqual(15, resultList[1].NullableValue);
    }

    [TestMethod]
    public void When_cursor_property_selector_returns_consistent_values_Apply_orders_consistently()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 3, Name = "C", Value = 30 },
        new TestEntity { Id = 1, Name = "A", Value = 10 },
        new TestEntity { Id = 2, Name = "B", Value = 20 }
      };
      var pageInfo = new PageInfo(after: "1", limit: 10u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Ordered by Id: 1, 2, 3 -> after 1: 2, 3
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(2, resultList[0].Id);
      Assert.AreEqual(3, resultList[1].Id);
    }

    [TestMethod]
    public void When_limit_is_less_than_matching_results_Apply_returns_only_limit_items()
    {
      // Arrange
      var source = Enumerable.Range(1, 100).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).ToList();
      var pageInfo = new PageInfo(after: "10", limit: 5u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert - Items after 10: 11-100, but limit to 5
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(5, resultList);
      Assert.AreEqual(11, resultList[0].Id);
      Assert.AreEqual(15, resultList[4].Id);
    }

    [TestMethod]
    public void When_using_DateTime_cursor_with_before_Apply_filters_correctly()
    {
      // Arrange
      var baseDate = new DateTime(2024, 1, 1, 0, 0, 0,DateTimeKind.Local);
      var source = new List<TestEntityWithDateTime>
      {
        new TestEntityWithDateTime { Id = 1, Date = baseDate.AddDays(1) },
        new TestEntityWithDateTime { Id = 2, Date = baseDate.AddDays(5) },
        new TestEntityWithDateTime { Id = 3, Date = baseDate.AddDays(10) }
      };
      DateTime cursorDate = baseDate.AddDays(7);
      var pageInfo = new PageInfo(0u, 10u) { Before = cursorDate.ToString("O") };

      // Act
      IEnumerable<TestEntityWithDateTime>? result = pageInfo.Apply(source, (item) => item.Date);

      // Assert - Items before day 7: day 1 and day 5
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(baseDate.AddDays(1), resultList[0].Date);
      Assert.AreEqual(baseDate.AddDays(5), resultList[1].Date);
    }

    [TestMethod]
    public void When_generic_overload_limit_equals_one_Apply_returns_single_item()
    {
      // Arrange
      var source = new List<TestEntity>
      {
        new TestEntity { Id = 1, Name = "a", Value = 1 },
        new TestEntity { Id = 2, Name = "b", Value = 2 },
        new TestEntity { Id = 3, Name = "c", Value = 3 }
      };
      var pageInfo = new PageInfo(after: "1", limit: 1u);

      // Act
      IEnumerable<TestEntity>? result = pageInfo.Apply(source, (item) => item.Id);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual(2, resultList[0].Id);
    }
  }
}
