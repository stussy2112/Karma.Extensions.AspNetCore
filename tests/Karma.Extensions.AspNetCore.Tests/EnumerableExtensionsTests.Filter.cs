// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.Filter.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class EnumerableExtensionsTests
  {
    // ========== Filter Tests ==========

    [TestMethod]
    public void When_source_is_null_Filter_returns_null()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      var filters = new FilterInfoCollection("test");

      // Act
      IEnumerable<TestEntity>? result = source!.Filter(filters);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_filters_is_null_Filter_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      FilterInfoCollection? filters = null;

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_filters_is_empty_Filter_returns_original_source()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      var filters = new FilterInfoCollection("empty", []);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_filters_has_single_condition_Filter_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "Bob")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(1, resultList);
      Assert.AreEqual("Bob", resultList[0].Name);
      Assert.AreEqual(2, resultList[0].Id);
    }

    [TestMethod]
    public void When_filters_has_multiple_conditions_with_and_Filter_applies_all()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("ValueFilter", nameof(TestEntity.Value), Operator.GreaterThan, 10.0),
        new FilterInfo("IdFilter", nameof(TestEntity.Id), Operator.LessThan, 3)
      ];
      var filters = new FilterInfoCollection("test", Conjunction.And, filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual(1, resultList[0].Id); // Alice with Value=10.5 and Id=1
      Assert.AreEqual(2, resultList[1].Id); // Bob with Value=20.0 and Id=2
    }

    [TestMethod]
    public void When_filters_has_multiple_conditions_with_or_Filter_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "Alice"),
        new FilterInfo("NameFilter2", nameof(TestEntity.Name), Operator.EqualTo, "Charlie")
      ];
      var filters = new FilterInfoCollection("test", Conjunction.Or, filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Charlie", resultList[1].Name);
    }

    [TestMethod]
    public void When_no_items_match_filter_Filter_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "NonExistent")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.IsEmpty(resultList);
    }

    [TestMethod]
    public void When_source_is_empty_Filter_returns_empty_sequence()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("NameFilter", nameof(TestEntity.Name), Operator.EqualTo, "Bob")
      ];
      var filters = new FilterInfoCollection("test", filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(0, resultList);
    }

    [TestMethod]
    public void When_filtering_with_numeric_operators_Filter_applies_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("ValueFilter", nameof(TestEntity.Value), Operator.GreaterThanOrEqualTo, 15.0)
      ];
      var filters = new FilterInfoCollection("test", filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("Bob", resultList[0].Name);
      Assert.AreEqual("Charlie", resultList[1].Name);
    }

    [TestMethod]
    public void When_all_items_match_filter_Filter_returns_all_items()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> filterInfos =
      [
        new FilterInfo("ValueFilter", nameof(TestEntity.Value), Operator.GreaterThan, 0)
      ];
      var filters = new FilterInfoCollection("test", filterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(3, resultList);
    }

    [TestMethod]
    public void When_filter_uses_complex_nested_conditions_Filter_evaluates_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      List<IFilterInfo> innerFilterInfos =
      [
        new FilterInfo("ValueFilter1", nameof(TestEntity.Value), Operator.GreaterThan, 10.0),
        new FilterInfo("ValueFilter2", nameof(TestEntity.Value), Operator.LessThan, 20.0)
      ];
      var innerFilters = new FilterInfoCollection("inner", Conjunction.And, innerFilterInfos);

      List<IFilterInfo> outerFilterInfos =
      [
        innerFilters
      ];
      var filters = new FilterInfoCollection("outer", Conjunction.And, outerFilterInfos);

      // Act
      IEnumerable<TestEntity>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(2, resultList);
      Assert.AreEqual("Alice", resultList[0].Name);
      Assert.AreEqual("Charlie", resultList[1].Name);
    }
  }
}
