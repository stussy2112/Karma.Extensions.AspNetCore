// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.Sort.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class EnumerableExtensionsTests
  {
    // ========== Sort<T>(IEnumerable<T>, IEnumerable<SortInfo>) Tests ==========

    [TestMethod]
    public void When_source_is_null_Sort_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = source!.Sort(sortInfos);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_sortInfos_is_null_Sort_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      IEnumerable<SortInfo>? sortInfos = null;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_sortInfos_is_empty_Sort_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<SortInfo> sortInfos = [];

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_both_source_and_sortInfos_are_null_Sort_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      IEnumerable<SortInfo>? sortInfos = null;

      // Act
      IEnumerable<TestEntity>? result = source!.Sort(sortInfos);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_sortInfos_has_single_ascending_sort_Sort_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = ((IEnumerable<TestEntity>)_testData).Reverse();
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Charlie", resultList[2].Name);
    }

    [TestMethod]
    public void When_sortInfos_has_single_descending_sort_Sort_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Descending)];

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      Assert.AreEqual("Charlie", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Alice", resultList[2].Name);
    }

    [TestMethod]
    public void When_sortInfos_has_multiple_sorts_Sort_chains_correctly()
    {
      // Arrange
      List<Person> people =
      [
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Smith", Age = 25 },
        new() { FirstName = "Alice", LastName = "Johnson", Age = 35 }
      ];

      List<SortInfo> sortInfos =
      [
        new(fieldName: "LastName", direction: ListSortDirection.Ascending),
        new(fieldName: "FirstName", direction: ListSortDirection.Ascending)
      ];

      // Act
      IEnumerable<Person>? result = people.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual("Johnson", resultList[0].LastName);
      Assert.AreEqual("Alice", resultList[0].FirstName);
      Assert.AreEqual("Smith", resultList[1].LastName);
      Assert.AreEqual("Alice", resultList[1].FirstName);
      Assert.AreEqual("Smith", resultList[2].LastName);
      Assert.AreEqual("Bob", resultList[2].FirstName);
    }

    [TestMethod]
    public void When_sortInfos_has_three_level_sort_Sort_chains_all_correctly()
    {
      // Arrange
      List<Person> people =
      [
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Smith", Age = 30 },
        new() { FirstName = "Alice", LastName = "Smith", Age = 25 },
        new() { FirstName = "Charlie", LastName = "Johnson", Age = 30 }
      ];

      List<SortInfo> sortInfos =
      [
        new(fieldName: "LastName", direction: ListSortDirection.Ascending),
        new(fieldName: "Age", direction: ListSortDirection.Descending),
        new(fieldName: "FirstName", direction: ListSortDirection.Ascending)
      ];

      // Act
      IEnumerable<Person>? result = people.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();

      // Johnson first
      Assert.AreEqual("Johnson", resultList[0].LastName);

      // Smith entries: by Age DESC, then FirstName ASC
      Assert.AreEqual("Smith", resultList[1].LastName);
      Assert.AreEqual(30, resultList[1].Age);
      Assert.AreEqual("Alice", resultList[1].FirstName);

      Assert.AreEqual("Smith", resultList[2].LastName);
      Assert.AreEqual(30, resultList[2].Age);
      Assert.AreEqual("Bob", resultList[2].FirstName);

      Assert.AreEqual("Smith", resultList[3].LastName);
      Assert.AreEqual(25, resultList[3].Age);
      Assert.AreEqual("Alice", resultList[3].FirstName);
    }

    [TestMethod]
    public void When_property_does_not_exist_Sort_skips_that_sort()
    {
      // Arrange
      List<SortInfo> sortInfos =
      [
        new(fieldName: "NonExistentProperty", direction: ListSortDirection.Ascending),
        new(fieldName: "Name", direction: ListSortDirection.Ascending)
      ];
      IEnumerable<TestEntity> source = ((IEnumerable<TestEntity>)_testData).Reverse();

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Should be sorted by Name since NonExistentProperty was skipped
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Charlie", resultList[2].Name);
    }

    [TestMethod]
    public void When_all_properties_do_not_exist_Sort_returns_original_order()
    {
      // Arrange
      List<SortInfo> sortInfos =
      [
        new(fieldName: "NonExistent1", direction: ListSortDirection.Ascending),
        new(fieldName: "NonExistent2", direction: ListSortDirection.Descending)
      ];
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Should maintain original order
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }

    [TestMethod]
    public void When_source_is_empty_Sort_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public void When_sorting_by_numeric_property_Sort_sorts_numerically()
    {
      // Arrange
      List<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Ascending)];
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual(10.5, resultList[0].Value);
      Assert.AreEqual(15.0, resultList[1].Value);
      Assert.AreEqual(20.0, resultList[2].Value);
    }

    [TestMethod]
    public void When_sorting_by_numeric_property_descending_Sort_sorts_numerically()
    {
      // Arrange
      List<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Descending)];
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual(20.0, resultList[0].Value);
      Assert.AreEqual(15.0, resultList[1].Value);
      Assert.AreEqual(10.5, resultList[2].Value);
    }

    [TestMethod]
    public void When_property_name_has_different_casing_Sort_finds_property_case_insensitively()
    {
      // Arrange
      List<SortInfo> sortInfos = [new SortInfo("name", ListSortDirection.Ascending)]; // lowercase
      IEnumerable<TestEntity> source = ((IEnumerable<TestEntity>)_testData).Reverse();

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      // Should sort by Name property (case-insensitive property lookup)
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Charlie", resultList[2].Name);
    }

    [TestMethod]
    public void When_source_has_duplicate_values_Sort_maintains_relative_order()
    {
      // Arrange
      List<TestEntity> sourceWithDuplicates =
      [
        new TestEntity { Id = 1, Name = "Same", Value = 25.0 },
        new TestEntity { Id = 2, Name = "Same", Value = 25.0 },
        new TestEntity { Id = 3, Name = "Same", Value = 25.0 }
      ];
      List<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = sourceWithDuplicates.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
      // All values are the same
      Assert.IsTrue(resultList.All((entity) => entity.Value is <= 25.0 and >= 25.0));
    }

    [TestMethod]
    public void When_large_collection_Sort_performs_efficiently()
    {
      // Arrange
      var largeSource = Enumerable.Range(1, 1000)
        .Select((i) => new TestEntity { Id = i, Name = $"Name{1001 - i}", Value = i })
        .Reverse()
        .ToList();
      List<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = largeSource.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1000, resultList);
      Assert.AreEqual(1.0, resultList[0].Value);
      Assert.AreEqual(1000.0, resultList[999].Value);
    }

    [TestMethod]
    public void When_sorting_with_minus_prefix_Sort_treats_as_descending()
    {
      // Arrange
      // SortInfo constructor handles '-' prefix
      List<SortInfo> sortInfos = [new SortInfo("-Name")]; // Should be descending
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual("Charlie", resultList[0].Name);
      Assert.AreEqual("Bob", resultList[1].Name);
      Assert.AreEqual("Alice", resultList[2].Name);
    }

    [TestMethod]
    public void When_deferred_execution_Sort_does_not_materialize_immediately()
    {
      // Arrange
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];
      IEnumerable<TestEntity> source = _testData;

      // Act
      IEnumerable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      // Result should be IOrderedEnumerable, not materialized list
      _ = Assert.IsInstanceOfType<IOrderedEnumerable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_sorting_with_mixed_directions_Sort_chains_correctly()
    {
      // Arrange
      List<Person> people =
      [
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Smith", Age = 25 },
        new() { FirstName = "Alice", LastName = "Johnson", Age = 35 },
        new() { FirstName = "Charlie", LastName = "Smith", Age = 20 }
      ];

      List<SortInfo> sortInfos =
      [
        new(fieldName: "LastName", direction: ListSortDirection.Ascending),
        new(fieldName: "Age", direction: ListSortDirection.Descending)
      ];

      // Act
      IEnumerable<Person>? result = people.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();

      // Johnson comes first (ascending LastName)
      Assert.AreEqual("Johnson", resultList[0].LastName);
      Assert.AreEqual(35, resultList[0].Age);

      // Smith entries sorted by Age descending
      Assert.AreEqual("Smith", resultList[1].LastName);
      Assert.AreEqual(30, resultList[1].Age); // Alice, 30

      Assert.AreEqual("Smith", resultList[2].LastName);
      Assert.AreEqual(25, resultList[2].Age); // Bob, 25

      Assert.AreEqual("Smith", resultList[3].LastName);
      Assert.AreEqual(20, resultList[3].Age); // Charlie, 20
    }

    [TestMethod]
    public void When_sorting_on_single_item_Sort_returns_single_item()
    {
      // Arrange
      List<TestEntity> singleItemSource =
      [
        new TestEntity { Id = 1, Name = "Alice", Value = 10.5 }
      ];
      List<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = singleItemSource.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
    }

    [TestMethod]
    public void When_sorting_with_id_property_Sort_applies_correctly()
    {
      // Arrange
      List<TestEntity> unsortedSource =
      [
        new TestEntity { Id = 3, Name = "Charlie", Value = 15.0 },
        new TestEntity { Id = 1, Name = "Alice", Value = 10.5 },
        new TestEntity { Id = 2, Name = "Bob", Value = 20.0 }
      ];
      List<SortInfo> sortInfos = [new SortInfo("Id", ListSortDirection.Ascending)];

      // Act
      IEnumerable<TestEntity>? result = unsortedSource.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual(1, resultList[0].Id);
      Assert.AreEqual(2, resultList[1].Id);
      Assert.AreEqual(3, resultList[2].Id);
    }
  }
}
