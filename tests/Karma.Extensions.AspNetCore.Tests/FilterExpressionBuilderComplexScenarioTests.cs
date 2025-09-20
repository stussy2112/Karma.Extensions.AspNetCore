// -----------------------------------------------------------------------
// <copyright file="FilterExpressionBuilderComplexScenarioTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Complex scenario unit tests for FilterExpressionBuilder focusing on nested collections and advanced cases.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class FilterExpressionBuilderComplexScenarioTests
  {

    [TestMethod]
    public void When_Filter_Path_Has_Trailing_Dots_BuildLambda_Returns_True()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("TrailingDots", $"{nameof(TestEntity.Name)}...", Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
    }

    [TestMethod]
    public void When_Filter_Path_Has_Leading_Dots_BuildLambda_Returns_True()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("LeadingDots", $"...{nameof(TestEntity.Name)}", Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
    }

    [TestMethod]
    public void When_Guid_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      var testGuid = Guid.NewGuid();
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, testGuid.ToString())];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(lambda(new TestEntity { UniqueId = Guid.NewGuid() }));
      Assert.IsFalse(lambda(new TestEntity { UniqueId = null }));
    }

    [TestMethod]
    public void When_Guid_Comparison_BuildLambda_Converts_Correctly()
    {
      // Arrange
      var testGuid = Guid.NewGuid();
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, testGuid)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(lambda(new TestEntity { UniqueId = Guid.NewGuid() }));
      Assert.IsFalse(lambda(new TestEntity { UniqueId = null }));
    }

    [TestMethod]
    public void When_Decimal_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Price", nameof(TestEntity.Price), Operator.EqualTo, "99.99")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Price = 99.99m }));
      Assert.IsFalse(lambda(new TestEntity { Price = 100.00m }));
    }

    [TestMethod]
    public void When_Float_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Rating", nameof(TestEntity.Rating), Operator.EqualTo, "4.5")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Rating = 4.5f }));
      Assert.IsFalse(lambda(new TestEntity { Rating = 3.5f }));
    }

    [TestMethod]
    public void When_List_Contains_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange
      var entity = new EntityWithMultipleCollections { IntegerList = [1, 2, 3, 4, 5] };
      List<IFilterInfo> filters = [new FilterInfo("IntegerList", nameof(EntityWithMultipleCollections.IntegerList), Operator.Contains, 3)];
      Func<EntityWithMultipleCollections, bool> lambda = FilterExpressionBuilder.BuildLambda<EntityWithMultipleCollections>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entity));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { IntegerList = [1, 2, 4, 5] }));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { IntegerList = null }));
    }

    [TestMethod]
    public void When_HashSet_Contains_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange
      var entity = new EntityWithMultipleCollections { StringSet = ["foo", "bar", "baz"] };
      List<IFilterInfo> filters = [new FilterInfo("StringSet", nameof(EntityWithMultipleCollections.StringSet), Operator.Contains, "bar")];
      Func<EntityWithMultipleCollections, bool> lambda = FilterExpressionBuilder.BuildLambda<EntityWithMultipleCollections>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entity));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { StringSet = ["foo", "baz"] }));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { StringSet = null }));
    }

    [TestMethod]
    public void When_IEnumerable_Contains_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange
      var entity = new EntityWithMultipleCollections { DoubleEnumerable = [1.1, 2.2, 3.3] };
      List<IFilterInfo> filters = [new FilterInfo("DoubleEnumerable", nameof(EntityWithMultipleCollections.DoubleEnumerable), Operator.Contains, 2.2)];
      Func<EntityWithMultipleCollections, bool> lambda = FilterExpressionBuilder.BuildLambda<EntityWithMultipleCollections>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entity));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { DoubleEnumerable = [1.1, 3.3] }));
      Assert.IsFalse(lambda(new EntityWithMultipleCollections { DoubleEnumerable = null }));
    }

    [TestMethod]
    public void When_Nullable_Value_Type_Compared_To_NonNullable_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      var entity = new EntityWithNullableValueTypes { DecimalValue = 100.50m };
      List<IFilterInfo> filters = [new FilterInfo("DecimalValue", nameof(EntityWithNullableValueTypes.DecimalValue), Operator.EqualTo, 100.50m)];
      Func<EntityWithNullableValueTypes, bool> lambda = FilterExpressionBuilder.BuildLambda<EntityWithNullableValueTypes>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entity));
      Assert.IsFalse(lambda(new EntityWithNullableValueTypes { DecimalValue = null }));
      Assert.IsFalse(lambda(new EntityWithNullableValueTypes { DecimalValue = 99.99m }));
    }

    [TestMethod]
    public void When_Nullable_Value_Type_Compared_To_Null_BuildLambda_Evaluates_Correctly()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("BooleanValue", nameof(EntityWithNullableValueTypes.BooleanValue), Operator.EqualTo, (bool?)null!)];
      Func<EntityWithNullableValueTypes, bool> lambda = FilterExpressionBuilder.BuildLambda<EntityWithNullableValueTypes>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new EntityWithNullableValueTypes { BooleanValue = null }));
      Assert.IsFalse(lambda(new EntityWithNullableValueTypes { BooleanValue = true }));
      Assert.IsFalse(lambda(new EntityWithNullableValueTypes { BooleanValue = false }));
    }

    [TestMethod]
    public void When_FilterInfo_Collection_With_Complex_Cache_Key_BuildLambda_Caches_Correctly()
    {
      // Arrange
      List<IFilterInfo> nestedFilters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.StartsWith, "test"),
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, 10.0, 20.0)
      ];
      var nestedCollection = new FilterInfoCollection("memberOf", "complex", Conjunction.Or, nestedFilters);

      List<IFilterInfo> mainFilters = [
        nestedCollection,
        new FilterInfo("Id", nameof(TestEntity.Id), Operator.In, 1, 2, 3, 4, 5)
      ];

      // Act - Build lambda multiple times to test caching
      Func<TestEntity, bool> lambda1 = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);
      Func<TestEntity, bool> lambda2 = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);

      // Assert - Both should work identically
      var testEntity = new TestEntity { Name = "testing", Value = 15.0, Id = 3 };
      Assert.IsTrue(lambda1(testEntity));
      Assert.IsTrue(lambda2(testEntity));
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id
      {
        get; init;
      }
      public string? Name
      {
        get; init;
      }
      public double Value
      {
        get; init;
      }
      public string? Description
      {
        get; init;
      }
      public int? NullableInt
      {
        get; init;
      }
      public bool IsActive
      {
        get; init;
      }
      public DateTime CreatedDate
      {
        get; init;
      }
      public Guid? UniqueId
      {
        get; init;
      }
      public decimal Price
      {
        get; init;
      }
      public float Rating
      {
        get; init;
      }
      public Collection<string>? Tags
      {
        get; init;
      }
      public NestedEntity? Nested
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public string? Code
      {
        get; init;
      }
      public int? Count
      {
        get; init;
      }
      public DeepNestedEntity? Deep
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record DeepNestedEntity
    {
      public string? Value
      {
        get; init;
      }
      public int Level
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record EntityWithMultipleCollections
    {
      public int Id
      {
        get; init;
      }

      public Collection<int>? IntegerList
      {
        get; init;
      }

      public HashSet<string>? StringSet
      {
        get; init;
      }

      public int[]? IntArray
      {
        get; init;
      }

      public IEnumerable<double>? DoubleEnumerable
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record EntityWithNullableValueTypes
    {
      public bool? BooleanValue
      {
        get; init;
      }
      public DateTime? DateTimeValue
      {
        get; init;
      }
      public Guid? GuidValue
      {
        get; init;
      }
      public decimal? DecimalValue
      {
        get; init;
      }
      public float? FloatValue
      {
        get; init;
      }
      public byte? ByteValue
      {
        get; init;
      }
      public char? CharValue
      {
        get; init;
      }
    }
  }
}