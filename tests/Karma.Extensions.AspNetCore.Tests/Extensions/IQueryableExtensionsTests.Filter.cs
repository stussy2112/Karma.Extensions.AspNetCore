// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.Filter.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Unit tests for <see cref="EnumerableExtensions"/> IQueryable extension methods:
  /// FilterInfoCollection.Apply, Page, Page(pageNumber, pageSize), and Sort.
  /// </summary>
  public partial class IQueryableExtensionsTests
  {
    [TestMethod]
    public void When_source_is_null_Filter_IQueryable_returns_null_Using_IQueryableFilter()
    {
      // Arrange
      IQueryable<Product>? source = null;
      FilterInfoCollection filters = new("TestFilter", Conjunction.And, [new FilterInfo("Name", nameof(Product.Name), Operator.EqualTo, "Test")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_filters_is_null_Filter_IQueryable_returns_original_source_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection? filters = null;

      // Act
      IQueryable<Product>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_filters_is_empty_Filter_IQueryable_returns_original_source_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("EmptyFilters", Conjunction.And, []);

      // Act
      IQueryable<Product>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_filters_count_is_zero_Filter_IQueryable_returns_original_source_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("ZeroFilters", Conjunction.And);

      // Act
      IQueryable<Product>? result = source.Filter(filters);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_single_EqualTo_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(6, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category == "Electronics"));
    }

    [TestMethod]
    public void When_single_NotEqualTo_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("NotElectronics", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.NotEqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category != "Electronics"));
    }

    [TestMethod]
    public void When_GreaterThan_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("PriceFilter", Conjunction.And, [new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 100.00m)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(6, resultList);
      Assert.IsTrue(resultList.All((p) => p.Price > 100.00m));
    }

    [TestMethod]
    public void When_LessThan_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("LowPrice", Conjunction.And, [new FilterInfo("Price", nameof(Product.Price), Operator.LessThan, 50.00m)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.Price < 50.00m));
    }

    [TestMethod]
    public void When_Between_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("PriceRange", Conjunction.And, [new FilterInfo("Price", nameof(Product.Price), Operator.Between, 50.00m, 500.00m)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, resultList);
      Assert.IsTrue(resultList.All((p) => p.Price is > 50.00m and < 500.00m));
    }

    [TestMethod]
    public void When_NotBetween_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("OutsidePriceRange", Conjunction.And, [new FilterInfo("Price", nameof(Product.Price), Operator.NotBetween, 50.00m, 500.00m)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(5, resultList);
      Assert.IsTrue(resultList.All((p) => p.Price is <= 50.00m or >= 500.00m));
    }

    [TestMethod]
    public void When_Contains_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("NameContains", Conjunction.And, [new FilterInfo("Name", nameof(Product.Name), Operator.Contains, "Laptop")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.Name!.Contains("Laptop", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void When_NotContains_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("NoGaming", Conjunction.And, [new FilterInfo("Name", nameof(Product.Name), Operator.NotContains, "Gaming")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(8, resultList);
      Assert.IsTrue(resultList.All((p) => !p.Name!.Contains("Gaming", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void When_StartsWith_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("StartsWithGaming", Conjunction.And, [new FilterInfo("Name", nameof(Product.Name), Operator.StartsWith, "Gaming")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.Name!.StartsWith("Gaming")));
    }

    [TestMethod]
    public void When_EndsWith_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("EndsWithMouse", Conjunction.And, [new FilterInfo("Name", nameof(Product.Name), Operator.EndsWith, "Mouse")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.Name!.EndsWith("Mouse")));
    }

    [TestMethod]
    public void When_In_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryIn", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.In, "Electronics", "Lighting")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(7, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category is "Electronics" or "Lighting"));
    }

    [TestMethod]
    public void When_NotIn_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryNotIn", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.NotIn, "Electronics", "Lighting")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category == "Furniture"));
    }

    [TestMethod]
    public void When_IsNull_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("DescriptionNull", Conjunction.And, [new FilterInfo("Description", nameof(Product.Description), Operator.IsNull)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.Description is null));
    }

    [TestMethod]
    public void When_IsNotNull_filter_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("DescriptionNotNull", Conjunction.And, [new FilterInfo("Description", nameof(Product.Description), Operator.IsNotNull)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(8, resultList);
      Assert.IsTrue(resultList.All((p) => p.Description is not null));
    }

    [TestMethod]
    public void When_multiple_filters_with_And_conjunction_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("MultipleAnd", Conjunction.And,
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics"),
        new FilterInfo("IsActive", nameof(Product.IsActive), Operator.EqualTo, true),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 30.00m)
      ]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category == "Electronics" && p.IsActive && p.Price > 30.00m));
    }

    [TestMethod]
    public void When_multiple_filters_with_Or_conjunction_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("MultipleOr", Conjunction.Or,
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Furniture"),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 1000.00m)
      ]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category == "Furniture" || p.Price > 1000.00m));
    }

    [TestMethod]
    public void When_nested_filter_collections_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange - (Category=Electronics OR Price>1000) AND IsActive=true
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;

      var orFilters = new FilterInfoCollection("OrGroup", Conjunction.Or,
      [
        new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics"),
        new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, 1000.00m)
      ]);

      FilterInfoCollection filters = new("NestedFilters", Conjunction.And,
      [
        orFilters,
        new FilterInfo("IsActive", nameof(Product.IsActive), Operator.EqualTo, true)
      ]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(5, resultList);
      Assert.IsTrue(resultList.All((p) => (p.Category == "Electronics" || p.Price > 1000.00m) && p.IsActive));
    }

    [TestMethod]
    public void When_filter_with_type_conversion_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("PriceFromString", Conjunction.And, [new FilterInfo("Price", nameof(Product.Price), Operator.GreaterThan, "100.00")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(6, resultList);
      Assert.IsTrue(resultList.All((p) => p.Price > 100.00m));
    }

    [TestMethod]
    public void When_filter_with_DateTime_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      var compareDate = new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfoCollection filters = new("DateFilter", Conjunction.And, [new FilterInfo("CreatedDate", nameof(Product.CreatedDate), Operator.GreaterThan, compareDate)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, resultList);
      Assert.IsTrue(resultList.All((p) => p.CreatedDate > compareDate));
    }

    [TestMethod]
    public void When_filter_with_nullable_value_type_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("StockFilter", Conjunction.And, [new FilterInfo("Stock", nameof(Product.Stock), Operator.GreaterThan, 5)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(5, resultList);
      Assert.IsTrue(resultList.All((p) => p.Stock > 5));
    }

    [TestMethod]
    public void When_filter_with_enum_Filter_IQueryable_returns_filtered_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("StatusFilter", Conjunction.And, [new FilterInfo("Status", nameof(Product.Status), Operator.EqualTo, ProductStatus.Active)]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(8, resultList);
      Assert.IsTrue(resultList.All((p) => p.Status == ProductStatus.Active));
    }

    [TestMethod]
    public void When_filter_result_is_chained_with_OrderBy_Filter_IQueryable_works_correctly_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var orderedResults = result!.OrderBy((p) => p.Price).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(6, orderedResults);
      Assert.AreEqual("Wireless Mouse", orderedResults[0].Name);
      Assert.AreEqual("Gaming Laptop", orderedResults[5].Name);
    }

    [TestMethod]
    public void When_filter_result_is_chained_with_Skip_Take_Filter_IQueryable_works_correctly_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var pagedResults = result!.OrderBy((p) => p.Id).Skip(2).Take(2).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, pagedResults);
      Assert.AreEqual(3, pagedResults[0].Id);
      Assert.AreEqual(6, pagedResults[1].Id);
    }

    [TestMethod]
    public void When_filter_result_is_used_with_Count_Filter_IQueryable_works_correctly_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      int count = result!.Count();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(6, count);
    }

    [TestMethod]
    public void When_filter_result_is_used_with_Any_Filter_IQueryable_works_correctly_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      bool hasResults = result!.Any();

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(hasResults);
    }

    [TestMethod]
    public void When_filter_result_is_used_with_FirstOrDefault_Filter_IQueryable_works_correctly_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      Product? firstProduct = result!.FirstOrDefault();

      // Assert
      Assert.IsNotNull(result);
      Assert.IsNotNull(firstProduct);
      Assert.AreEqual("Gaming Laptop", firstProduct.Name);
    }

    [TestMethod]
    public void When_no_results_match_filter_Filter_IQueryable_returns_empty_sequence_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("NoMatch", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "NonExistentCategory")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, resultList);
    }

    [TestMethod]
    public void When_filter_with_invalid_property_path_Filter_IQueryable_returns_all_results_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filters = new("InvalidProperty", Conjunction.And, [new FilterInfo("Invalid", "NonExistentProperty", Operator.EqualTo, "Test")]);

      // Act
      IQueryable<Product>? result = source.Filter(filters);
      var resultList = result!.ToList();

      // Assert - Invalid property returns all results (TrueExpression)
      Assert.IsNotNull(result);
      Assert.HasCount(10, resultList);
    }

    [TestMethod]
    public void When_multiple_Filter_calls_are_chained_Filter_IQueryable_applies_all_filters_Using_IQueryableFilter()
    {
      // Arrange
      using TestDbContext context = CreateDbContext();
      IQueryable<Product> source = context.Products;
      FilterInfoCollection filter1 = new("CategoryFilter", Conjunction.And, [new FilterInfo("Category", nameof(Product.Category), Operator.EqualTo, "Electronics")]);
      FilterInfoCollection filter2 = new("ActiveFilter", Conjunction.And, [new FilterInfo("IsActive", nameof(Product.IsActive), Operator.EqualTo, true)]);

      // Act
      IQueryable<Product>? result = source.Filter(filter1).Filter(filter2);
      var resultList = result!.ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, resultList);
      Assert.IsTrue(resultList.All((p) => p.Category == "Electronics" && p.IsActive));
    }

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

    // ========== FilterInfoCollection.Apply<T>(IQueryable<T>) Tests ==========

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Null_Source_Returns_Null_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity>? source = null;
      FilterInfoCollection filters = new("test", [new FilterInfo("name", "Name", Operator.EqualTo, "Item 1")]);

      // Act
      IQueryable<TestEntity>? result = filters.Apply(source);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Null_Filters_Returns_Source_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection? filters = null;

      // Act
      IQueryable<TestEntity>? result = filters.Apply(source);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Empty_Filters_Returns_Source_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test");

      // Act
      IQueryable<TestEntity>? result = filters.Apply(source);

      // Assert
      Assert.AreSame(source, result);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Valid_Filter_Returns_IQueryable_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", [new FilterInfo("name", "Name", Operator.EqualTo, "Item 1")]);

      // Act
      IQueryable<TestEntity>? result = filters.Apply(source);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(result);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_Filters_By_EqualTo_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", [new FilterInfo("name", "Name", Operator.EqualTo, "Item 5")]);

      // Act
      var result = filters.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(1, result);
      Assert.AreEqual("Item 5", result[0].Name);
      Assert.AreEqual(5, result[0].Id);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_Filters_By_GreaterThan_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", [new FilterInfo("value", "Value", Operator.GreaterThan, 70)]);

      // Act
      var result = filters.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(3, result);
      Assert.IsTrue(result.All(e => e.Value > 70));
      Assert.AreEqual(80, result[0].Value);
      Assert.AreEqual(90, result[1].Value);
      Assert.AreEqual(100, result[2].Value);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_Multiple_Filters_Uses_AND_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test",
      [
        new FilterInfo("category", "Category", Operator.EqualTo, "A"),
        new FilterInfo("value", "Value", Operator.GreaterThanOrEqualTo, 60)
      ]);

      // Act
      var result = filters.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(2, result);
      Assert.IsTrue(result.All(e => e.Category == "A" && e.Value >= 60));
      Assert.AreEqual(6, result[0].Id);
      Assert.AreEqual(9, result[1].Id);
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_With_OR_Conjunction_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", Conjunction.Or,
      [
        new FilterInfo("name", "Name", Operator.EqualTo, "Item 1"),
        new FilterInfo("name", "Name", Operator.EqualTo, "Item 10")
      ]);

      // Act
      var result = filters.Apply(source)!.ToList();

      // Assert
      Assert.HasCount(2, result);
      Assert.IsTrue(result.Any(e => e.Name == "Item 1"));
      Assert.IsTrue(result.Any(e => e.Name == "Item 10"));
    }

    [TestMethod]
    public void When_FilterInfoCollection_Apply_Returns_Deferred_Execution_Using_FilterInfoCollectionApply()
    {
      // Arrange
      IQueryable<TestEntity> source = CreateTestData().AsQueryable();
      FilterInfoCollection filters = new("test", [new FilterInfo("value", "Value", Operator.LessThan, 50)]);

      // Act
      IQueryable<TestEntity>? query = filters.Apply(source);

      // Assert - Verify it's still IQueryable (not materialized)
      Assert.IsNotNull(query);
      _ = Assert.IsInstanceOfType<IQueryable<TestEntity>>(query);

      // Now materialize and verify
      var result = query!.ToList();
      Assert.HasCount(4, result);
      Assert.IsTrue(result.All(e => e.Value < 50));
    }
  }
}
