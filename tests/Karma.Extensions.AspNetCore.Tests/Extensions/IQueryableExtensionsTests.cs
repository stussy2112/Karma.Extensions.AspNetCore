// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Karma.Extensions.AspNetCore.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public partial class IQueryableExtensionsTests
  {
    public TestContext TestContext { get; set; } = null!;

    // ========== Integration Tests: Chaining All Four Methods (parameter-Apply pattern) ==========

    [TestMethod]
    public void When_Chaining_FilterInfoCollection_Apply_Sort_And_Page_All_Operations_Apply_Using_ParamApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();

      FilterInfoCollection filters = new("test",
      [
        new FilterInfo("category", "Category", Operator.EqualTo, "A")
      ]);

      IEnumerable<SortInfo> sortInfos = [new SortInfo("Value", ListSortDirection.Descending)];
      PageInfo pageInfo = new(offset: 1, limit: 2);

      // Act
      IQueryable<TestEntity> filtered = filters.Apply(source) ?? source;
      IQueryable<TestEntity> sorted = filtered.Sort(sortInfos) ?? filtered;
      IQueryable<TestEntity> paged = sorted.Page(pageInfo) ?? sorted;
      var result = paged.ToList();

      // Assert
      Assert.HasCount(2, result);

      // Verify filtering (Category = "A" yields Id: 1, 3, 6, 9 with values 10, 30, 60, 90)
      Assert.IsTrue(result.All(e => e.Category == "A"));

      // Verify sorting (descending by Value: 90, 60, 30, 10)
      // Verify pagination (skip 1, take 2 = 60, 30)
      Assert.AreEqual(60, result[0].Value);
      Assert.AreEqual(30, result[1].Value);
    }

    [TestMethod]
    public void When_Chaining_All_Methods_With_PageNumber_PageSize_Returns_Correct_Results_Using_ParamApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();

      FilterInfoCollection filters = new("test",
      [
        new FilterInfo("value", "Value", Operator.LessThanOrEqualTo, 80)
      ]);

      IEnumerable<SortInfo> sortInfos = [new SortInfo("Id", ListSortDirection.Descending)];
      int pageNumber = 2;
      int pageSize = 3;

      // Act
      IQueryable<TestEntity> filtered = filters.Apply(source) ?? source;
      IQueryable<TestEntity> sorted = filtered.Sort(sortInfos) ?? filtered;
      IQueryable<TestEntity> paged = sorted.Page(pageNumber, pageSize) ?? sorted;
      var result = paged.ToList();

      // Assert
      // Filtering: Value <= 80 yields 8 items (Id: 1-8)
      // Sorting: Descending by Id (8, 7, 6, 5, 4, 3, 2, 1)
      // Pagination: Page 2, size 3 = skip 3, take 3 = (5, 4, 3)
      Assert.HasCount(3, result);
      Assert.AreEqual(5, result[0].Id);
      Assert.AreEqual(4, result[1].Id);
      Assert.AreEqual(3, result[2].Id);
    }

    [TestMethod]
    public void When_All_Methods_Return_IQueryable_Until_Materialized_Using_ParamApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", [new FilterInfo("value", "Value", Operator.GreaterThan, 40)]);
      IEnumerable<SortInfo> sortInfos = [new SortInfo("Name")];
      PageInfo pageInfo = new(offset: 0, limit: 3);

      // Act
      IQueryable<TestEntity> filtered = filters.Apply(source) ?? source;
      IQueryable<TestEntity> sorted = filtered.Sort(sortInfos) ?? filtered;
      IQueryable<TestEntity> query = sorted.Page(pageInfo) ?? sorted;

      // Assert - Verify it's still IQueryable (deferred execution)
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(query);

      // Materialize and verify
      var result = query.ToList();
      Assert.HasCount(3, result);
      Assert.IsTrue(result.All(e => e.Value > 40));
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Complex_Filters_And_Pagination_Using_ParamApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();

      FilterInfoCollection filters = new("test", Conjunction.Or,
      [
        new FilterInfo("value", "Value", Operator.LessThan, 30),
        new FilterInfo("value", "Value", Operator.GreaterThan, 70)
      ]);

      PageInfo pageInfo = new(offset: 2, limit: 3);

      // Act
      IQueryable<TestEntity> filtered = filters.Apply(source) ?? source;
      IQueryable<TestEntity> paged = filtered.Page(pageInfo) ?? filtered;
      var result = paged.ToList();

      // Assert
      // Filtering: (Value < 30 OR Value > 70) yields 5 items (Id: 1, 2, 8, 9, 10)
      // Pagination: skip 2, take 3 = (Id: 8, 9, 10)
      Assert.HasCount(3, result);
      Assert.AreEqual(8, result[0].Id);
      Assert.AreEqual(9, result[1].Id);
      Assert.AreEqual(10, result[2].Id);
    }

    private static List<Product> GetTestProducts() =>
    [
      new() { Id = 1, Name = "Gaming Laptop", Price = 1200.00m, Category = "Electronics", IsActive = true, Description = "High-end gaming laptop", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 10, Status = ProductStatus.Active },
      new() { Id = 2, Name = "Wireless Mouse", Price = 25.00m, Category = "Electronics", IsActive = false, Description = null, CreatedDate = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Unspecified), Stock = null, Status = ProductStatus.Discontinued },
      new() { Id = 3, Name = "Mechanical Keyboard", Price = 75.00m, Category = "Electronics", IsActive = true, Description = "Mechanical keyboard", CreatedDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), Stock = 5, Status = ProductStatus.Active },
      new() { Id = 4, Name = "Office Desk", Price = 300.00m, Category = "Furniture", IsActive = true, Description = "Ergonomic desk", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 3, Status = ProductStatus.Active },
      new() { Id = 5, Name = "Office Chair", Price = 200.00m, Category = "Furniture", IsActive = true, Description = "Ergonomic chair", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 7, Status = ProductStatus.Active },
      new() { Id = 6, Name = "Gaming Mouse", Price = 45.00m, Category = "Electronics", IsActive = false, Description = "RGB gaming mouse", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 15, Status = ProductStatus.OutOfStock },
      new() { Id = 7, Name = "Monitor", Price = 400.00m, Category = "Electronics", IsActive = true, Description = null, CreatedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Unspecified), Stock = null, Status = ProductStatus.Active },
      new() { Id = 8, Name = "Premium Chair", Price = 1500.00m, Category = "Furniture", IsActive = true, Description = "Executive chair", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 2, Status = ProductStatus.Active },
      new() { Id = 9, Name = "Lamp", Price = 50.00m, Category = "Lighting", IsActive = true, Description = "LED desk lamp", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 20, Status = ProductStatus.Active },
      new() { Id = 10, Name = "Laptop", Price = 800.00m, Category = "Electronics", IsActive = true, Description = "Business laptop", CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), Stock = 8, Status = ProductStatus.Active }
    ];

    [ExcludeFromCodeCoverage]
    public sealed class TestDbContext : DbContext
    {
      public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
      {
      }

      public DbSet<Product> Products { get; set; } = null!;
    }

    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("Id={Id,nq}; Name={Name,nq}; Price={Price,nq}")]
    public sealed class Product
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public decimal Price { get; init; }
      public string? Category { get; init; }
      public bool IsActive { get; init; }
      public string? Description { get; init; }
      public DateTime CreatedDate { get; init; }
      public int? Stock { get; init; }
      public ProductStatus Status { get; init; }
    }

    public enum ProductStatus
    {
      Active,
      Discontinued,
      OutOfStock
    }
  }
}
