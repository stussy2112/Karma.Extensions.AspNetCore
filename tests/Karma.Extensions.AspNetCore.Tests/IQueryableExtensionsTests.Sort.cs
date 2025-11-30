// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Sort.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class IQueryableExtensionsTests
  {
    // ========== IQueryable<T>.Sort(IEnumerable<SortInfo>) Tests ==========

    [TestMethod]
    public void When_IQueryable_Sort_With_Null_Source_Returns_Null_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name")];

      // Act
      IQueryable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_Null_SortInfos_Returns_Source_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo>? sortInfos = null;

      // Act
      IQueryable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_Empty_SortInfos_Returns_Source_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [];

      // Act
      IQueryable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_Valid_Sort_Returns_IOrderedQueryable_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name")];

      // Act
      IQueryable<TestEntity>? result = source.Sort(sortInfos);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IOrderedQueryable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_IQueryable_Sort_Ascending_By_Name_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      var result = source.Sort(sortInfos)!.ToList();

      // Assert
      Assert.HasCount(10, result);
      Assert.AreEqual("Item 1", result[0].Name);
      Assert.AreEqual("Item 10", result[1].Name);
      Assert.AreEqual("Item 2", result[2].Name);
    }

    [TestMethod]
    public void When_IQueryable_Sort_Descending_By_Value_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Descending)];

      // Act
      var result = source.Sort(sortInfos)!.ToList();

      // Assert
      Assert.HasCount(10, result);
      Assert.AreEqual(100, result[0].Value);
      Assert.AreEqual(90, result[1].Value);
      Assert.AreEqual(80, result[2].Value);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_Multiple_SortInfos_Applies_ThenBy_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = new[]
      {
        new TestEntity { Id = 1, Name = "B", Category = "Cat1", Value = 100 },
        new TestEntity { Id = 2, Name = "A", Category = "Cat1", Value = 200 },
        new TestEntity { Id = 3, Name = "C", Category = "Cat1", Value = 50 },
        new TestEntity { Id = 4, Name = "B", Category = "Cat2", Value = 150 },
        new TestEntity { Id = 5, Name = "A", Category = "Cat2", Value = 250 }
      }.AsQueryable();

      IEnumerable<SortInfo> sortInfos =
      [
        new SortInfo("Category", ListSortDirection.Ascending),
        new SortInfo("Name", ListSortDirection.Descending)
      ];

      // Act
      var result = source.Sort(sortInfos)!.ToList();

      // Assert
      Assert.HasCount(5, result);
      Assert.AreEqual("Cat1", result[0].Category);
      Assert.AreEqual("C", result[0].Name);
      Assert.AreEqual("Cat1", result[1].Category);
      Assert.AreEqual("B", result[1].Name);
      Assert.AreEqual("Cat1", result[2].Category);
      Assert.AreEqual("A", result[2].Name);
      Assert.AreEqual("Cat2", result[3].Category);
      Assert.AreEqual("B", result[3].Name);
      Assert.AreEqual("Cat2", result[4].Category);
      Assert.AreEqual("A", result[4].Name);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_NonExistent_Property_Skips_Sort_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("NonExistentProperty")];

      // Act
      IQueryable<TestEntity>? result = source.Sort(sortInfos);

      // Assert - Should return source unchanged
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Sort_With_Mixed_Valid_And_Invalid_Properties_Applies_Valid_Only_Using_IQueryableSort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos =
      [
        new SortInfo("NonExistent"),
        new SortInfo("Value", ListSortDirection.Ascending)
      ];

      // Act
      var result = source.Sort(sortInfos)!.ToList();

      // Assert - Should apply only the valid sort (Value)
      Assert.HasCount(10, result);
      Assert.AreEqual(10, result[0].Value);
      Assert.AreEqual(20, result[1].Value);
      Assert.AreEqual(30, result[2].Value);
    }
  }
}
