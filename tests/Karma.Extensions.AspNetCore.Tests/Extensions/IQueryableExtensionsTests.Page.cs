// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Page.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Unit tests for <see cref="EnumerableExtensions"/> IQueryable Apply methods for PageInfo and SortInfo.
  /// </summary>
  public partial class IQueryableExtensionsTests
  {
    // ========== PageInfo.Apply<T>(IQueryable<T>) Tests ==========

    [TestMethod]
    public void When_PageInfo_Apply_With_Null_Source_Returns_Null()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      PageInfo pageInfo = new(offset: 0, limit: 10);

      // Act
      IQueryable<TestEntity>? result = pageInfo.Apply(source);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_PageInfo_Apply_With_Null_PageInfo_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo? pageInfo = null;

      // Act
      IQueryable<TestEntity>? result = pageInfo.Apply(source);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_PageInfo_Apply_With_Valid_Pagination_Returns_IQueryable()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 2, limit: 3);

      // Act
      IQueryable<TestEntity>? result = pageInfo.Apply(source);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_PageInfo_Apply_Paginates_Correctly()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 2, limit: 3);

      // Act
      var result = pageInfo.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.AreEqual(3, result[0].Id);
      Assert.AreEqual(4, result[1].Id);
      Assert.AreEqual(5, result[2].Id);
    }

    [TestMethod]
    public void When_PageInfo_Apply_With_Offset_Zero_Returns_First_Page()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 0, limit: 3);

      // Act
      var result = pageInfo.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.AreEqual(1, result[0].Id);
      Assert.AreEqual(2, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
    }

    [TestMethod]
    public void When_PageInfo_Apply_With_Limit_Larger_Than_Source_Returns_All_Remaining()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 8, limit: 100);

      // Act
      var result = pageInfo.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(2, result);
      Assert.AreEqual(9, result[0].Id);
      Assert.AreEqual(10, result[1].Id);
    }

    [TestMethod]
    public void When_PageInfo_Apply_With_Offset_Beyond_Source_Returns_Empty()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 100, limit: 10);

      // Act
      var result = pageInfo.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(0, result);
    }

    // ========== IEnumerable<SortInfo>.Apply<T>(IQueryable<T>) Tests ==========

    [TestMethod]
    public void When_SortInfo_Apply_With_Null_Source_Returns_Null()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name")];

      // Act
      IQueryable<TestEntity>? result = sortInfos.Apply(source);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_Null_SortInfos_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo>? sortInfos = null;

      // Act
      IQueryable<TestEntity>? result = sortInfos.Apply(source);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_Empty_SortInfos_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [];

      // Act
      IQueryable<TestEntity>? result = sortInfos.Apply(source);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_Valid_Sort_Returns_IOrderedQueryable()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name")];

      // Act
      IQueryable<TestEntity>? result = sortInfos.Apply(source);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IOrderedQueryable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_SortInfo_Apply_Sorts_Ascending_By_Name()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Ascending)];

      // Act
      var result = sortInfos.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(10, result);
      Assert.AreEqual("Item 1", result[0].Name);
      Assert.AreEqual("Item 10", result[1].Name);
      Assert.AreEqual("Item 2", result[2].Name);
    }

    [TestMethod]
    public void When_SortInfo_Apply_Sorts_Descending_By_Name()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name", ListSortDirection.Descending)];

      // Act
      var result = sortInfos.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(10, result);
      Assert.AreEqual("Item 9", result[0].Name);
      Assert.AreEqual("Item 8", result[1].Name);
      Assert.AreEqual("Item 7", result[2].Name);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_Multiple_Sorts_Applies_ThenBy()
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
        new SortInfo("Name", ListSortDirection.Ascending)
      ];

      // Act
      var result = sortInfos.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(5, result);
      Assert.AreEqual("Cat1", result[0].Category);
      Assert.AreEqual("A", result[0].Name);
      Assert.AreEqual("Cat1", result[1].Category);
      Assert.AreEqual("B", result[1].Name);
      Assert.AreEqual("Cat1", result[2].Category);
      Assert.AreEqual("C", result[2].Name);
      Assert.AreEqual("Cat2", result[3].Category);
      Assert.AreEqual("A", result[3].Name);
      Assert.AreEqual("Cat2", result[4].Category);
      Assert.AreEqual("B", result[4].Name);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_NonExistent_Property_Skips_Sort()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos =
      [
        new SortInfo("NonExistentProperty", ListSortDirection.Ascending)
      ];

      // Act
      IQueryable<TestEntity>? result = sortInfos.Apply(source);

      // Assert - Should return source unchanged when property doesn't exist
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_SortInfo_Apply_With_Mixed_Valid_And_Invalid_Sorts_Applies_Valid_Only()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      IEnumerable<SortInfo> sortInfos =
      [
        new SortInfo("NonExistent", ListSortDirection.Ascending),
        new SortInfo("Name", ListSortDirection.Ascending)
      ];

      // Act
      var result = sortInfos.Apply(source)!.ToList();

      // Assert - Should apply only the valid sort (Name)
      Assert.HasCount(10, result);
      Assert.AreEqual("Item 1", result[0].Name);
    }

    // ========== Integration Tests: Chaining All Operations ==========

    // Note: Chaining and deferred-execution tests consolidated into the primary test file
    // to avoid overlapping duplicate test coverage. See IQueryableExtensionsTests.cs for canonical tests.

    // ========== IQueryable<T>.Page(PageInfo) Tests ==========

    [TestMethod]
    public void When_using_with_queryable_Page_with_pageNumber_works_correctly()
    {
      // Arrange
      IQueryable<TestEntity> source = Enumerable.Range(1, 50).Select((i) => new TestEntity { Id = i, Name = $"N{i}", Value = i }).AsQueryable();
      int pageNumber = 3;
      int pageSize = 10;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.HasCount(10, resultList);
      Assert.AreEqual(21, resultList[0].Id);
      Assert.AreEqual(30, resultList[9].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_Null_Source_Returns_Null()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      PageInfo pageInfo = new(offset: 0, limit: 5);

      // Act
      IQueryable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_Null_PageInfo_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo? pageInfo = null;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_Valid_PageInfo_Returns_IQueryable()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 0, limit: 5);

      // Act
      IQueryable<TestEntity>? result = source.Page(pageInfo);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_IQueryable_Page_Paginates_First_Page()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 0, limit: 3);

      // Act
      var result = source.Page(pageInfo)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.AreEqual(1, result[0].Id);
      Assert.AreEqual(2, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_Paginates_Second_Page()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 3, limit: 3);

      // Act
      var result = source.Page(pageInfo)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.AreEqual(4, result[0].Id);
      Assert.AreEqual(5, result[1].Id);
      Assert.AreEqual(6, result[2].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_Offset_Beyond_Source_Returns_Empty()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 100, limit: 10);

      // Act
      var result = source.Page(pageInfo)!.ToList();

      // Assert
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_Limit_Larger_Than_Remaining_Returns_All_Remaining()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      PageInfo pageInfo = new(offset: 7, limit: 100);

      // Act
      var result = source.Page(pageInfo)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.AreEqual(8, result[0].Id);
      Assert.AreEqual(9, result[1].Id);
      Assert.AreEqual(10, result[2].Id);
    }

    // ========== IQueryable<T>.Page(int, int) Tests ==========

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Null_Source_Returns_Null()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      int pageNumber = 1;
      int pageSize = 5;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Zero_PageSize_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 1;
      int pageSize = 0;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Negative_PageSize_Returns_Source()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 1;
      int pageSize = -5;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Zero_PageNumber_Defaults_To_One()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 0;
      int pageSize = 3;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert - Should return first page (page 1)
      Assert.HasCount(3, result);
      Assert.AreEqual(1, result[0].Id);
      Assert.AreEqual(2, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Negative_PageNumber_Defaults_To_One()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = -5;
      int pageSize = 3;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert - Should return first page (page 1)
      Assert.HasCount(3, result);
      Assert.AreEqual(1, result[0].Id);
      Assert.AreEqual(2, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_First_Page()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 1;
      int pageSize = 4;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert
      Assert.HasCount(4, result);
      Assert.AreEqual(1, result[0].Id);
      Assert.AreEqual(2, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
      Assert.AreEqual(4, result[3].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Second_Page()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 2;
      int pageSize = 4;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert
      Assert.HasCount(4, result);
      Assert.AreEqual(5, result[0].Id);
      Assert.AreEqual(6, result[1].Id);
      Assert.AreEqual(7, result[2].Id);
      Assert.AreEqual(8, result[3].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Third_Page_Partial()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 3;
      int pageSize = 4;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert
      Assert.HasCount(2, result);
      Assert.AreEqual(9, result[0].Id);
      Assert.AreEqual(10, result[1].Id);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Beyond_Data_Returns_Empty()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 10;
      int pageSize = 5;

      // Act
      var result = source.Page(pageNumber, pageSize)!.ToList();

      // Assert
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_IQueryable_Page_With_PageNumber_And_PageSize_Returns_IQueryable()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      int pageNumber = 1;
      int pageSize = 5;

      // Act
      IQueryable<TestEntity>? result = source.Page(pageNumber, pageSize);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(result);
    }
  }
}
