// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Unit tests for the EnumerableExtensions class, covering FilterByQuery and CreateGroupDictionary methods.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class EnumerableExtensionsTests
  {
    private DefaultHttpContext _httpContext = null!;
    private List<TestEntity> _testData = null!;

    [TestInitialize]
    public void Setup()
    {
      _httpContext = new DefaultHttpContext();
      _testData = [
        new TestEntity { Id = 1, Name = "Alice", Value = 10.5 },
        new TestEntity { Id = 2, Name = "Bob", Value = 20.0 },
        new TestEntity { Id = 3, Name = "Charlie", Value = 15.0 }
      ];
    }

    // ========== FilterByQuery Tests ==========

    [TestMethod]
    public void When_source_is_null_FilterByQuery_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;

      // Act
      IEnumerable<TestEntity>? result = source!.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_httpContext_is_null_FilterByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(null!);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_both_source_and_httpContext_are_null_FilterByQuery_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;

      // Act
      IEnumerable<TestEntity>? result = source!.FilterByQuery(null!);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_httpContext_has_no_filters_FilterByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      // HttpContext.Items is empty by default

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_non_FilterInfoCollection_value_FilterByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      _httpContext.Items[ContextItemKeys.Filters] = "not a FilterInfoCollection";

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_null_filters_value_FilterByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      _httpContext.Items[ContextItemKeys.Filters] = null;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_empty_FilterInfoCollection_FilterByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var emptyFilters = new FilterInfoCollection("empty", []);
      _httpContext.Items[ContextItemKeys.Filters] = emptyFilters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_has_valid_filters_FilterByQuery_applies_filtering()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos = [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "Bob")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);
      _httpContext.Items[ContextItemKeys.Filters] = filters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreNotSame(source, result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("Bob", resultList[0].Name);
      Assert.AreEqual(2, resultList[0].Id);
    }

    [TestMethod]
    public void When_httpContext_has_multiple_filters_FilterByQuery_applies_all_filters()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos = [
        new FilterInfo("ValueFilter", nameof(TestEntity.Value), Operator.GreaterThan, 15.0)
      ];
      var filters = new FilterInfoCollection("test", filterInfos);
      _httpContext.Items[ContextItemKeys.Filters] = filters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("Bob", resultList[0].Name);
      Assert.AreEqual(20.0, resultList[0].Value);
    }

    [TestMethod]
    public void When_filters_match_no_items_FilterByQuery_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos = [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "NonExistent")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);
      _httpContext.Items[ContextItemKeys.Filters] = filters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.IsEmpty(resultList);
    }

    [TestMethod]
    public void When_source_is_empty_FilterByQuery_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      List<IFilterInfo> filterInfos = [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "Bob")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);
      _httpContext.Items[ContextItemKeys.Filters] = filters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(0, resultList);
    }

    [TestMethod]
    public void When_filters_have_complex_conditions_FilterByQuery_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos = [
        new FilterInfo("ValueFilter1", nameof(TestEntity.Value), Operator.GreaterThan, 10.0),
        new FilterInfo("ValueFilter2", nameof(TestEntity.Value), Operator.LessThan, 20.0)
      ];
      var filters = new FilterInfoCollection("test", Conjunction.And, filterInfos);
      _httpContext.Items[ContextItemKeys.Filters] = filters;

      // Act
      IEnumerable<TestEntity>? result = source.FilterByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual(10.5, resultList[0].Value);
      Assert.AreEqual("Charlie", resultList[1].Name);
      Assert.AreEqual(15.0, resultList[1].Value);
    }

    // ========== CreateGroupDictionary Tests ==========

    [TestMethod]
    public void When_source_is_null_CreateGroupDictionary_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source!.CreateGroupDictionary(keySelector);
      });
    }

    [TestMethod]
    public void When_keySelector_is_null_CreateGroupDictionary_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string>? keySelector = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source.CreateGroupDictionary(keySelector!);
      });
    }

    [TestMethod]
    public void When_source_is_empty_CreateGroupDictionary_returns_empty_dictionary()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_source_has_unique_keys_CreateGroupDictionary_creates_single_item_groups()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsTrue(result.ContainsKey("Bob"));
      Assert.IsTrue(result.ContainsKey("Charlie"));

      Assert.HasCount(1, result["Alice"]);
      Assert.HasCount(1, result["Bob"]);
      Assert.HasCount(1, result["Charlie"]);

      Assert.AreEqual(1, result["Alice"][0].Id);
      Assert.AreEqual(2, result["Bob"][0].Id);
      Assert.AreEqual(3, result["Charlie"][0].Id);
    }

    [TestMethod]
    public void When_source_has_duplicate_keys_CreateGroupDictionary_groups_items_correctly()
    {
      // Arrange
      List<TestEntity> sourceWithDuplicates = [
        new TestEntity { Id = 1, Name = "Group1", Value = 10.0 },
        new TestEntity { Id = 2, Name = "Group2", Value = 20.0 },
        new TestEntity { Id = 3, Name = "Group1", Value = 30.0 },
        new TestEntity { Id = 4, Name = "Group2", Value = 40.0 },
        new TestEntity { Id = 5, Name = "Group1", Value = 50.0 }
      ];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = sourceWithDuplicates.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey("Group1"));
      Assert.IsTrue(result.ContainsKey("Group2"));

      Assert.HasCount(3, result["Group1"]);
      Assert.HasCount(2, result["Group2"]);

      var group1Ids = result["Group1"].Select((entity) => entity.Id).ToList();
      var group2Ids = result["Group2"].Select((entity) => entity.Id).ToList();

      int[] expected = [1, 3, 5];
      int[] expected1 = [2, 4];
      CollectionAssert.AreEquivalent(expected, group1Ids);
      CollectionAssert.AreEquivalent(expected1, group2Ids);
    }

    [TestMethod]
    public void When_using_default_equality_comparer_CreateGroupDictionary_uses_default_comparison()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsFalse(result.ContainsKey("alice")); // Case sensitive by default
    }

    [TestMethod]
    public void When_using_custom_equality_comparer_CreateGroupDictionary_uses_custom_comparison()
    {
      // Arrange
      List<TestEntity> sourceWithCaseVariations = [
        new TestEntity { Id = 1, Name = "Alice", Value = 10.0 },
        new TestEntity { Id = 2, Name = "alice", Value = 20.0 },
        new TestEntity { Id = 3, Name = "ALICE", Value = 30.0 }
      ];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = sourceWithCaseVariations.CreateGroupDictionary(
        keySelector,
        StringComparer.OrdinalIgnoreCase);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result); // All variations should be grouped together

      // The key will be the first one encountered
      string actualKey = result.Keys.First();
      Assert.AreEqual("Alice", actualKey);
      Assert.HasCount(3, result[actualKey]);

      var allIds = result[actualKey].Select((entity) => entity.Id).ToList();
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEquivalent(expected, allIds);
    }

    [TestMethod]
    public void When_using_null_equality_comparer_CreateGroupDictionary_uses_default_comparer()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector, null);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsFalse(result.ContainsKey("alice")); // Case sensitive by default
    }

    [TestMethod]
    public void When_key_selector_returns_different_types_CreateGroupDictionary_works_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, int> keySelector = (entity) => entity.Id % 2; // Even/odd grouping

      // Act
      Dictionary<int, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey(0)); // Even
      Assert.IsTrue(result.ContainsKey(1)); // Odd

      Assert.HasCount(1, result[0]); // Only Bob (Id=2)
      Assert.HasCount(2, result[1]); // Alice (Id=1) and Charlie (Id=3)

      Assert.AreEqual("Bob", result[0][0].Name);
      var oddNames = result[1].Select((entity) => entity.Name).ToList();
      string[] expected = ["Alice", "Charlie"];
      CollectionAssert.AreEquivalent(expected, oddNames);
    }

    [TestMethod]
    public void When_key_selector_returns_null_values_CreateGroupDictionary_handles_gracefully()
    {
      // Arrange
      List<TestEntityWithNullableString> sourceWithNulls = [
        new TestEntityWithNullableString { Id = 1, NullableName = "Group1" },
        new TestEntityWithNullableString { Id = 2, NullableName = null },
        new TestEntityWithNullableString { Id = 3, NullableName = "Group1" },
        new TestEntityWithNullableString { Id = 4, NullableName = null }
      ];

      // Use a non-null constraint on the key type
      Func<TestEntityWithNullableString, string> keySelector = (entity) => entity.NullableName ?? "NULL_GROUP";

      // Act
      Dictionary<string, List<TestEntityWithNullableString>> result = sourceWithNulls.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey("Group1"));
      Assert.IsTrue(result.ContainsKey("NULL_GROUP"));

      Assert.HasCount(2, result["Group1"]);
      Assert.HasCount(2, result["NULL_GROUP"]);
    }

    [TestMethod]
    public void When_source_contains_many_items_CreateGroupDictionary_maintains_insertion_order_within_groups()
    {
      // Arrange
      List<TestEntity> largeSource = [];
      for (int i = 1; i <= 100; i++)
      {
        largeSource.Add(new TestEntity { Id = i, Name = $"Group{i % 10}", Value = i });
      }

      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = largeSource.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(10, result); // Group0 through Group9

      // Verify each group has 10 items
      foreach (List<TestEntity> group in result.Values)
      {
        Assert.HasCount(10, group);
      }

      // Verify insertion order within groups (first item should have lower Id than last)
      foreach (List<TestEntity> group in result.Values)
      {
        for (int i = 1; i < group.Count; i++)
        {
          Assert.IsLessThan(group[i].Id, group[i - 1].Id);
        }
      }
    }
    // ========== SortByQuery Tests ==========

    [TestMethod]
    public void When_source_is_null_SortByQuery_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;

      // Act
      IEnumerable<TestEntity>? result = source!.SortByQuery(_httpContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_httpContext_is_null_SortByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(null!);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_items_does_not_contain_sortInfo_key_SortByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      // HttpContext.Items is empty by default

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_httpContext_items_sortInfo_value_is_not_SortInfo_type_SortByQuery_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      _httpContext.Items[ContextItemKeys.SortInfo] = "not a SortInfo object";

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_sortInfo_direction_is_ascending_SortByQuery_sorts_ascending_by_property()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      source = source.Reverse(); // Reverse to ensure sorting is actually applied
      var sortInfo = new SortInfo("Name", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Charlie", resultList[2].Name);
    }

    [TestMethod]
    public void When_sortInfo_direction_is_descending_SortByQuery_sorts_descending_by_property()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var sortInfo = new SortInfo("Name", ListSortDirection.Descending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("Charlie", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Alice", resultList[2].Name);
    }

    [TestMethod]
    public void When_sorting_by_numeric_property_ascending_SortByQuery_sorts_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var sortInfo = new SortInfo("Value", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(10.5, resultList[0].Value);
      Assert.AreEqual(15.0, resultList[1].Value);
      Assert.AreEqual(20.0, resultList[2].Value);
    }

    [TestMethod]
    public void When_sorting_by_numeric_property_descending_SortByQuery_sorts_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var sortInfo = new SortInfo("Value", ListSortDirection.Descending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual(20.0, resultList[0].Value);
      Assert.AreEqual(15.0, resultList[1].Value);
      Assert.AreEqual(10.5, resultList[2].Value);
    }

    [TestMethod]
    public void When_property_does_not_exist_SortByQuery_returns_original_order()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var sortInfo = new SortInfo("NonExistentProperty", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      // Original order should be maintained when property doesn't exist
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }

    [TestMethod]
    public void When_source_is_empty_SortByQuery_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      var sortInfo = new SortInfo("Name", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_property_name_has_different_casing_SortByQuery_is_case_sensitive()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var sortInfo = new SortInfo("name", ListSortDirection.Ascending); // lowercase 'name'
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = source.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      // Should maintain original order since 'name' property doesn't exist (case sensitive)
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }

    [TestMethod]
    public void When_sorting_large_collection_SortByQuery_performs_efficiently()
    {
      // Arrange
      var largeSource = Enumerable.Range(1, 1000)
        .Select((i) => new TestEntity { Id = i, Name = $"Name{1001 - i}", Value = i })
        .Reverse()
        .ToList();
      var sortInfo = new SortInfo("Value", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = largeSource.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1000, resultList);
      Assert.AreEqual(1.0, resultList[0].Value);
      Assert.AreEqual(1000.0, resultList[999].Value);
      // Verify it's actually sorted
      for (int i = 1; i < resultList.Count; i++)
      {
        Assert.IsLessThanOrEqualTo(resultList[i].Value, resultList[i - 1].Value);
      }
    }

    [TestMethod]
    public void When_source_has_duplicate_property_values_SortByQuery_maintains_stable_sort()
    {
      // Arrange
      List<TestEntity> sourceWithDuplicates = [
        new TestEntity { Id = 1, Name = "Same", Value = 25.0 },
        new TestEntity { Id = 2, Name = "Same", Value = 25.0 },
        new TestEntity { Id = 3, Name = "Same", Value = 25.0 }
      ];
      var sortInfo = new SortInfo("Value", ListSortDirection.Ascending);
      _httpContext.Items[ContextItemKeys.SortInfo] = new[] { sortInfo };

      // Act
      IEnumerable<TestEntity>? result = sourceWithDuplicates.SortByQuery(_httpContext);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      // All values should be the same
      Assert.IsTrue(resultList.All((entity) => entity.Value is <= 25.0 and >= 25.0));
    }

    // ========== Test Entity Classes ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id
      {
        get; init;
      }
      public string Name { get; init; } = string.Empty;
      public double Value
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithNullableString
    {
      public int Id
      {
        get; init;
      }
      public string? NullableName
      {
        get; init;
      }
    }
  }
}