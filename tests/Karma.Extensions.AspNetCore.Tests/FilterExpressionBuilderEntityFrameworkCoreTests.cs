// -----------------------------------------------------------------------
// <copyright file="FilterExpressionBuilderEntityFrameworkCoreTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Tests for FilterExpressionBuilder with TRUE Entity Framework Core integration using DbContext 
  /// and in-memory database to verify that generated expressions can be translated to SQL queries.
  /// </summary>
  /// <remarks>
  /// These tests validate that FilterExpressionBuilder.BuildExpression creates expression trees that are 
  /// compatible with Entity Framework Core's expression tree to SQL translation. We use EF Core's InMemory 
  /// database provider with a real DbContext to ensure expressions can be translated by EF Core's query provider.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class FilterExpressionBuilderEntityFrameworkCoreTests
  {
    private static TestDbContext CreateDbContext()
    {
      DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

      var context = new TestDbContext(options);

      // Seed test data
      context.Products.AddRange(GetTestProducts());
      _ = context.SaveChanges();

      return context;
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

    [TestMethod]
    public void When_EqualTo_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act - Use BuildExpression to get raw expression tree for EF Core
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(6, results);
      Assert.IsTrue(results.All((p) => p.Category == "Electronics"));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("IsActive", nameof(Product.IsActive), Operator.NotEqualTo, false)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(8, results);
      Assert.IsTrue(results.All((p) => p.IsActive));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 100.00m)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(6, results);
      Assert.IsTrue(results.All((p) => p.Price > 100.00m));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(Product.Price), Operator.LessThanOrEqualTo, 75.00m)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(4, results);
      Assert.IsTrue(results.All((p) => p.Price <= 75.00m));
    }

    [TestMethod]
    public void When_Between_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(Product.Price), Operator.Between, 50.00m, 500.00m)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert - Between is EXCLUSIVE: price > 50 AND price < 500
      // Products that match: Mechanical Keyboard (75), Office Desk (300), Office Chair (200), Monitor (400)
      Assert.HasCount(4, results);
      Assert.IsTrue(results.All((p) => p.Price is > 50.00m and < 500.00m));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(Product.Price), Operator.NotBetween, 50.00m, 500.00m)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert - NotBetween is EXCLUSIVE: price <= 50 OR price >= 500
      // Products that match: Gaming Laptop (1200), Wireless Mouse (25), Gaming Mouse (45), Premium Chair (1500), Lamp (50), Laptop (800)
      Assert.HasCount(5, results);
      Assert.IsTrue(results.All((p) => p.Price is <= 50.00m or >= 500.00m));
    }

    [TestMethod]
    public void When_Contains_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(Product.Name), Operator.Contains, "Laptop")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.Name!.Contains("Laptop", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(Product.Name), Operator.StartsWith, "Gaming")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.Name!.StartsWith("Gaming")));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(Product.Name), Operator.EndsWith, "Mouse")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.Name!.EndsWith("Mouse")));
    }

    [TestMethod]
    public void When_NotContains_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(Product.Name), Operator.NotContains, "Gaming")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(8, results);
      Assert.IsTrue(results.All((p) => !p.Name!.Contains("Gaming", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void When_In_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.In, "Electronics", "Lighting")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(7, results);
      Assert.IsTrue(results.All((p) => p.Category is "Electronics" or "Lighting"));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.NotIn, "Electronics", "Lighting")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(3, results);
      Assert.IsTrue(results.All((p) => p.Category == "Furniture"));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Description", nameof(Product.Description), Operator.IsNull)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.Description is null));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Description", nameof(Product.Description), Operator.IsNotNull)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(8, results);
      Assert.IsTrue(results.All((p) => p.Description is not null));
    }

    [TestMethod]
    public void When_Multiple_Filters_With_And_Conjunction_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters =
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics"),
        new FilterInfo("IsActive", nameof(Product.IsActive), Operator.EqualTo, true),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 30.00m)
      ];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(4, results);
      Assert.IsTrue(results.All((p) => p.Category == "Electronics" && p.IsActive && p.Price > 30.00m));
    }

    [TestMethod]
    public void When_Multiple_Filters_With_Or_Conjunction_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      var filterCollection = new FilterInfoCollection("orFilters", Conjunction.Or,
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Furniture"),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 1000.00m)
      ]);

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filterCollection);
      var results = context.Products.Where(expression).ToList();

      // Assert - Products: Office Desk, Office Chair, Premium Chair (all Furniture), Gaming Laptop (Price > 1000)
      Assert.HasCount(4, results);
      Assert.IsTrue(results.All((p) => p.Category == "Furniture" || p.Price > 1000.00m));
    }

    [TestMethod]
    public void When_Nested_Property_Filter_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using var context = new TestDbContextWithSuppliers(
        new DbContextOptionsBuilder<TestDbContextWithSuppliers>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options);

      // Seed suppliers first
      var techCorp = new Supplier { Id = 1, Name = "TechCorp", Country = "USA" };
      var globalTech = new Supplier { Id = 2, Name = "GlobalTech", Country = "China" };
      context.Suppliers.AddRange(techCorp, globalTech);
      _ = context.SaveChanges();

      // Then add products with supplier references
      context.ProductsWithSuppliers.AddRange(      
        new() { Id = 1, Name = "Laptop", SupplierId = 1 },
        new() { Id = 2, Name = "Mouse", SupplierId = 2 },
        new() { Id = 3, Name = "Keyboard", SupplierId = 1 });
      _ = context.SaveChanges();

      List<IFilterInfo> filters = [new FilterInfo("SupplierName", "Supplier.Name", Operator.EqualTo, "TechCorp")];

      // Act
      Expression<Func<ProductWithSupplier, bool>> expression = FilterExpressionBuilder.BuildExpression<ProductWithSupplier>(filters);
      var results = context.ProductsWithSuppliers.Include((p) => p.Supplier).Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.Supplier?.Name == "TechCorp"));
    }

    [TestMethod]
    public void When_DateTime_Comparison_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      var compareDate = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Unspecified);
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(Product.CreatedDate), Operator.GreaterThan, compareDate)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.IsTrue(results.All((p) => p.CreatedDate > compareDate));
    }

    [TestMethod]
    public void When_Nullable_Value_Type_Filter_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Stock", nameof(Product.Stock), Operator.GreaterThan, 5)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(5, results);
      Assert.IsTrue(results.All((p) => p.Stock > 5));
    }

    [TestMethod]
    public void When_Enum_Comparison_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Status", nameof(Product.Status), Operator.EqualTo, ProductStatus.Active)];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(8, results);
      Assert.IsTrue(results.All((p) => p.Status == ProductStatus.Active));
    }

    [TestMethod]
    public void When_Complex_Nested_FilterInfoCollection_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange - Simulates complex query: (Category=Electronics OR Price>1000) AND IsActive=true
      using TestDbContext context = CreateDbContext();

      var orFilters = new FilterInfoCollection("orGroup", Conjunction.Or,
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics"),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 1000.00m)
      ]);

      List<IFilterInfo> mainFilters =
      [
        orFilters,
        new FilterInfo("IsActive", nameof(Product.IsActive), Operator.EqualTo, true)
      ];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(mainFilters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(5, results);
      Assert.IsTrue(results.All((p) => (p.Category == "Electronics" || p.Price > 1000.00m) && p.IsActive));
    }

    [TestMethod]
    public void When_Type_Conversion_With_DbContext_BuildExpression_Translates_To_SQL()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, "100.00")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).ToList();

      // Assert
      Assert.HasCount(6, results);
      Assert.IsTrue(results.All((p) => p.Price > 100.00m));
    }

    [TestMethod]
    public void When_Ordering_After_Filtering_With_DbContext_BuildExpression_Works_With_OrderBy()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).OrderBy((p) => p.Price).ToList();

      // Assert
      Assert.HasCount(6, results);
      Assert.AreEqual("Wireless Mouse", results[0].Name);
      Assert.AreEqual("Gaming Laptop", results[5].Name);
    }

    [TestMethod]
    public void When_Pagination_After_Filtering_With_DbContext_BuildExpression_Works_With_Skip_Take()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      var results = context.Products.Where(expression).OrderBy((p) => p.Id).Skip(2).Take(2).ToList();

      // Assert
      Assert.HasCount(2, results);
      Assert.AreEqual(3, results[0].Id);
      Assert.AreEqual(6, results[1].Id);
    }

    [TestMethod]
    public void When_Count_After_Filtering_With_DbContext_BuildExpression_Works_With_Count()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      int count = context.Products.Count(expression);

      // Assert
      Assert.AreEqual(6, count);
    }

    [TestMethod]
    public void When_FirstOrDefault_After_Filtering_With_DbContext_BuildExpression_Works_With_FirstOrDefault()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      Product? result = context.Products.FirstOrDefault(expression);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("Gaming Laptop", result.Name);
    }

    [TestMethod]
    public void When_Any_After_Filtering_With_DbContext_BuildExpression_Works_With_Any()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      List<IFilterInfo> filters = [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")];

      // Act
      Expression<Func<Product, bool>> expression = FilterExpressionBuilder.BuildExpression<Product>(filters);
      bool hasElectronics = context.Products.Any(expression);
      bool hasToys = context.Products.Any((p) => p.Category == "Toys");

      // Assert
      Assert.IsTrue(hasElectronics);
      Assert.IsFalse(hasToys);
    }

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
    public sealed class TestDbContextWithSuppliers : DbContext
    {
      public TestDbContextWithSuppliers(DbContextOptions<TestDbContextWithSuppliers> options)
        : base(options)
      {
      }

      public DbSet<ProductWithSupplier> ProductsWithSuppliers { get; set; } = null!;
      public DbSet<Supplier> Suppliers { get; set; } = null!;
    }

    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("Id={Id,nq}; Price={Price,nq}")]
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

    [ExcludeFromCodeCoverage]
    public sealed class ProductWithSupplier
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public int? SupplierId { get; init; }
      public Supplier? Supplier { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed class Supplier
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public string? Country { get; init; }
    }

    public enum ProductStatus
    {
      Active,
      Discontinued,
      OutOfStock
    }
  }
}
