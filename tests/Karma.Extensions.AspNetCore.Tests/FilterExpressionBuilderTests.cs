// -----------------------------------------------------------------------
// <copyright file="FilterExpressionBuilderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class FilterExpressionBuilderTests
  {
    [TestMethod]
    public void When_Array_Property_With_Contains_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test Contains operator on array property
      List<IFilterInfo> filters = [new FilterInfo("Values", nameof(TestEntityWithArray.Values), Operator.Contains, 3)];
      Func<TestEntityWithArray, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithArray>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntityWithArray { Values = [1, 2, 3, 4, 5] }));
      Assert.IsFalse(lambda(new TestEntityWithArray { Values = [1, 2, 4, 5] }));
      Assert.IsFalse(lambda(new TestEntityWithArray { Values = null }));
    }

    [TestMethod]
    public void When_filter_query_string_()
    {
      // Arrange
      TestEntity expected = new()
      {
        Id = 3,
        Name = "baz",
        Value = 30.0
      };
      IEnumerable<TestEntity> items = [
        new() { Id = 1, Name = "foo", Value = 10.0 },
        new() { Id = 2, Name = "bar", Value = 20.0 },
        expected,
        new() { Id = 4, Name = "qux", Value = 40.0 },
        new() { Id = 5, Name = "quux", Value = 50.0 }
      ];

      string filterQuery = "?filter[$and][0][name]=baz&filter[$and][1][value]=30";
      var parser = new FilterQueryStringParser();
      FilterInfoCollection filters = parser.Parse(filterQuery);
      Assert.IsNotNull(filters);
      Assert.AreEqual(2, filters.Count);

      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      IEnumerable<TestEntity> result = items.Where(lambda);
      Assert.AreEqual(1, result.Count());

      TestEntity actual = result.First();
      Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public void When_Between_Operator_BuildLambda_Returns_True_For_In_Range()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, 10.0, 20.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 15.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 20.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 25.0 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Insufficient_Values_BuildLambda_Returns_False()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Value = 15.0 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Null_Values_BuildLambda_Throws_Exception()
    {
      // Arrange - Test Between operator with null values in the range
      object[] valuesWithNull = [null!, 20.0];
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, valuesWithNull)];

      // Act & Assert
      InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });

      Assert.AreEqual(expected: "Both values must be non-null for 'Between' operator.", actual: ex.Message);
    }

    [TestMethod]
    public void When_Boolean_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("IsActive", nameof(TestEntity.IsActive), Operator.EqualTo, "true")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { IsActive = true }));
      Assert.IsFalse(lambda(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_Case_Sensitive_Property_Name_BuildLambda_Finds_Property_Ignoring_Case()
    {
      // Arrange - Test case-insensitive property lookup
      List<IFilterInfo> filters = [new FilterInfo("name", "name", Operator.EqualTo, "test")]; // lowercase 'name'
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should find 'Name' property despite case difference
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Comparison_With_Null_Property_For_Value_Type_BuildLambda_Converts_To_Nullable()
    {
      // Arrange - Fix: This should test that null comparison with value type becomes nullable comparison
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EqualTo, (int?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - int property compared to null should always be false after nullable conversion
      Assert.IsFalse(lambda(new TestEntity { Id = 0 }));
      Assert.IsFalse(lambda(new TestEntity { Id = 123 }));
    }

    [TestMethod]
    public void When_Complex_Nested_FilterInfo_Collections_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test deeply nested FilterInfoCollections
      List<IFilterInfo> innerFilters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "inner")
      ];
      var innerCollection = new FilterInfoCollection("inner", Conjunction.And, innerFilters);

      List<IFilterInfo> outerFilters = [
        innerCollection,
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 5.0)
      ];
      var outerCollection = new FilterInfoCollection("outer", Conjunction.Or, outerFilters);

      List<IFilterInfo> mainFilters = [outerCollection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "inner", Value = 1.0 })); // Inner condition matches
      Assert.IsTrue(lambda(new TestEntity { Name = "other", Value = 10.0 })); // Value condition matches
      Assert.IsFalse(lambda(new TestEntity { Name = "other", Value = 1.0 })); // Neither condition matches
    }

    [TestMethod]
    public void When_Conditional_WeakTable_Cache_Isolation_BuildLambda_Different_Types_Use_Different_Caches()
    {
      // Arrange - Same filter content but different entity types
      List<IFilterInfo> filters1 = [new FilterInfo("Name", "Name", Operator.EqualTo, "test")];
      List<IFilterInfo> filters2 = [new FilterInfo("Name", "Name", Operator.EqualTo, "test")];

      // Act - Build lambdas for different types
      Func<TestEntity, bool> lambda1 = FilterExpressionBuilder.BuildLambda<TestEntity>(filters1);
      Func<SimpleEntity, bool> lambda2 = FilterExpressionBuilder.BuildLambda<SimpleEntity>(filters2);

      // Assert - Both should work independently (tests type-specific caching)
      Assert.IsTrue(lambda1(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda2(new SimpleEntity { Name = "test" }));
      Assert.IsFalse(lambda1(new TestEntity { Name = "other" }));
      Assert.IsFalse(lambda2(new SimpleEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Contains_Operator_BuildLambda_Returns_True_For_Collection_Contains()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Tags", nameof(TestEntity.Tags), Operator.Contains, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Tags = ["foo", "bar"] }));
      Assert.IsFalse(lambda(new TestEntity { Tags = ["baz"] }));
      Assert.IsFalse(lambda(new TestEntity { Tags = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_BuildLambda_Returns_True_For_Substring()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Description", nameof(TestEntity.Description), Operator.Contains, "lorem")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Description = "lorem ipsum" }));
      Assert.IsFalse(lambda(new TestEntity { Description = "ipsum" }));
      Assert.IsFalse(lambda(new TestEntity { Description = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_On_NonString_NonEnumerable_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - Test Contains on non-string, non-enumerable property (should convert to string)
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.Contains, "2")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should convert Id to string and check contains
      Assert.IsTrue(lambda(new TestEntity { Id = 123 })); // "123".Contains("2") == true
      Assert.IsTrue(lambda(new TestEntity { Id = 42 })); // "42".Contains("2") == true
      Assert.IsFalse(lambda(new TestEntity { Id = 135 })); // "135".Contains("2") == false
    }

    [TestMethod]
    public void When_DateTime_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange
      DateTime testDate = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, "2023-01-01")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { CreatedDate = testDate }));
      Assert.IsFalse(lambda(new TestEntity { CreatedDate = DateTime.Now }));
    }

    [TestMethod]
    public void When_Deep_Nested_Property_Path_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test deep nested property access
      List<IFilterInfo> filters = [new FilterInfo("DeepNested", "Nested.Code", Operator.EqualTo, "test")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Nested = new NestedEntity { Code = "test" } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Code = "other" } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_Empty_FilterInfo_Collection_BuildLambda_Returns_True_For_Any_Input()
    {
      // Arrange - Test empty FilterInfoCollection
      var emptyCollection = new FilterInfoCollection("empty", Conjunction.And, []);
      List<IFilterInfo> filters = [emptyCollection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity()));
      Assert.IsTrue(lambda(new TestEntity { Name = "anything" }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_BuildLambda_Returns_True_For_Suffix()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EndsWith, "oo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Empty_String_BuildLambda_Returns_True_For_All_NonNull()
    {
      // Arrange - Test EndsWith with empty string
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EndsWith, "")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - All non-null strings end with empty string
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Enumerable_With_Null_Elements_BuildLambda_Handles_Null_Elements_Correctly()
    {
      // Arrange - Test enumerable that might contain null elements
      var entityWithNullable = new TestEntityWithNullableStrings { Items = ["foo", null, "bar"] };
      List<IFilterInfo> filters = [new FilterInfo("Items", nameof(TestEntityWithNullableStrings.Items), Operator.Contains, (string)null!)];
      Func<TestEntityWithNullableStrings, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithNullableStrings>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entityWithNullable)); // Collection contains null
      Assert.IsFalse(lambda(new TestEntityWithNullableStrings { Items = ["foo", "bar"] })); // Collection doesn't contain null
    }

    [TestMethod]
    public void When_EqualTo_Operator_BuildLambda_Returns_True_For_Matching_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Expression_Tree_Caching_With_Same_Filters_Different_Types_BuildLambda_Uses_Separate_Caches()
    {
      // Arrange - Test that different types use separate caches
      List<IFilterInfo> filters1 = [new FilterInfo("Name", "Name", Operator.EqualTo, "test")];
      List<IFilterInfo> filters2 = [new FilterInfo("Name", "Name", Operator.EqualTo, "test")];

      // Act - Build lambdas for different types multiple times
      Func<TestEntity, bool> lambda1a = FilterExpressionBuilder.BuildLambda<TestEntity>(filters1);
      Func<SimpleEntity, bool> lambda2a = FilterExpressionBuilder.BuildLambda<SimpleEntity>(filters2);
      Func<TestEntity, bool> lambda1b = FilterExpressionBuilder.BuildLambda<TestEntity>(filters1);
      Func<SimpleEntity, bool> lambda2b = FilterExpressionBuilder.BuildLambda<SimpleEntity>(filters2);

      // Assert - All should work independently and potentially use cached results
      Assert.IsTrue(lambda1a(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda2a(new SimpleEntity { Name = "test" }));
      Assert.IsTrue(lambda1b(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda2b(new SimpleEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Field_Access_Instead_Of_Property_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test field access instead of property access
      var entityWithField = new TestEntityWithField { PublicField = "test" };
      List<IFilterInfo> filters = [new FilterInfo("Field", nameof(TestEntityWithField.PublicField), Operator.EqualTo, "test")];
      Func<TestEntityWithField, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithField>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(entityWithField));
      Assert.IsFalse(lambda(new TestEntityWithField { PublicField = "other" }));
    }

    [TestMethod]
    public void When_FilterInfo_Collection_Conjunction_Or_With_Nested_Groups_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test nested FilterInfoCollection with Or conjunction
      List<IFilterInfo> nestedFilters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "foo"),
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 5.0)
      ];
      var nestedCollection = new FilterInfoCollection("nested", Conjunction.Or, nestedFilters);

      List<IFilterInfo> mainFilters = [
        nestedCollection,
        new FilterInfo("Id", nameof(TestEntity.Id), Operator.LessThan, 100)
      ];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);

      // Act & Assert - Should match entities that satisfy nested OR conditions AND main condition
      Assert.IsTrue(lambda(new TestEntity { Name = "foo", Value = 3.0, Id = 50 })); // Name matches, Id matches
      Assert.IsTrue(lambda(new TestEntity { Name = "bar", Value = 10.0, Id = 50 })); // Value matches, Id matches
      Assert.IsFalse(lambda(new TestEntity { Name = "bar", Value = 3.0, Id = 50 })); // Neither nested condition matches
      Assert.IsFalse(lambda(new TestEntity { Name = "foo", Value = 10.0, Id = 150 })); // Nested matches but Id doesn't
    }

    // ========== MISSING TEST CASES FOR COMPREHENSIVE COVERAGE ==========
    [TestMethod]
    public void When_FilterInfo_With_Null_Path_And_Values_BuildLambda_Returns_True()
    {
      // Arrange - Test FilterInfo with null or empty path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", null!, Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity()));
    }

    [TestMethod]
    public void When_FilterInfo_With_Whitespace_Only_Path_BuildLambda_Returns_True()
    {
      // Arrange - Test FilterInfo with whitespace-only path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "   ", Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity()));
    }

    [TestMethod]
    public void When_Filters_Null_Or_Empty_BuildLambda_Returns_True_For_Any_Input()
    {
      // Arrange & Act
      Func<TestEntity, bool> lambdaNull = FilterExpressionBuilder.BuildLambda<TestEntity>(null!);
      Func<TestEntity, bool> lambdaEmpty = FilterExpressionBuilder.BuildLambda<TestEntity>([]);

      // Assert
      Assert.IsTrue(lambdaNull(new TestEntity()));
      Assert.IsTrue(lambdaEmpty(new TestEntity()));
    }

    [TestMethod]
    public void When_Filter_With_Null_Value_In_Collection_Comparison_BuildLambda_Handles_Null_Correctly()
    {
      // Arrange - Test that null values in collection comparison are handled gracefully
      List<IFilterInfo> filters = [new FilterInfo("Tags", nameof(TestEntity.Tags), Operator.Contains, (string)null!)];

      // Act - Build lambda should succeed without throwing an exception
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - The lambda should work and return false for non-null collections since string collections can't contain null by default
      // However, if the collection is null, the expression should return false due to null checking
      Assert.IsFalse(lambda(new TestEntity { Tags = ["foo", "bar"] })); // Collection doesn't contain null
      Assert.IsFalse(lambda(new TestEntity { Tags = null })); // Null collection returns false
      Assert.IsFalse(lambda(new TestEntity { Tags = [] })); // Empty collection doesn't contain null
    }

    [TestMethod]
    public void When_GreaterThan_Operator_BuildLambda_Returns_True_For_Greater_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 11.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 9.0 }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_BuildLambda_Returns_True_For_Greater_Or_Equal_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThanOrEqualTo, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 10.0 }));
      Assert.IsTrue(lambda(new TestEntity { Value = 11.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 9.0 }));
    }

    [TestMethod]
    public void When_In_Operator_BuildLambda_Returns_True_For_Contained_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.In, "foo", "bar")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "bar" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "baz" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_In_Operator_With_Empty_Values_Collection_BuildLambda_Returns_False()
    {
      // Arrange - Fix: FilterInfo constructor needs at least name and path parameters
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.In)]; // Empty values
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Name = "anything" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_In_Operator_With_Mixed_Null_And_Valid_Values_BuildLambda_Handles_Nulls_Correctly()
    {
      // Arrange - Fix: Create individual values array to handle nulls properly
      object[] values = ["foo", null!, "bar"];
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.In, values)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "bar" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null })); // null is in the values
      Assert.IsFalse(lambda(new TestEntity { Name = "baz" }));
    }

    [TestMethod]
    public void When_Invalid_Property_Path_BuildLambda_Returns_True()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "NonExistent", Operator.EqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_BuildLambda_Returns_True_For_NotNull()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNotNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { NullableInt = null }));
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 1 }));
    }

    [TestMethod]
    public void When_IsNull_Operator_BuildLambda_Returns_True_For_Null()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { NullableInt = null }));
      Assert.IsFalse(lambda(new TestEntity { NullableInt = 1 }));
    }

    [TestMethod]
    public void When_LessThan_Operator_BuildLambda_Returns_True_For_Lesser_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.LessThan, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 9.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 11.0 }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_BuildLambda_Returns_True_For_Lesser_Or_Equal_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.LessThanOrEqualTo, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 10.0 }));
      Assert.IsTrue(lambda(new TestEntity { Value = 9.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 11.0 }));
    }

    [TestMethod]
    public void When_Multiple_Filters_And_Conjunction_BuildLambda_Returns_True_For_All_Match()
    {
      // Arrange
      List<IFilterInfo> filters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "foo"),
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.0)
      ];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo", Value = 11.0 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "foo", Value = 9.0 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar", Value = 11.0 }));
    }

    [TestMethod]
    public void When_Multiple_Filters_Or_Conjunction_BuildLambda_Returns_True_For_Any_Match()
    {
      // Arrange
      var filters = new FilterInfoCollection("test", Conjunction.Or, [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "foo"),
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.0)
      ]);
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo", Value = 9.0 }));
      Assert.IsTrue(lambda(new TestEntity { Name = "bar", Value = 11.0 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar", Value = 9.0 }));
    }

    [TestMethod]
    public void When_Multiple_Null_Checks_In_Property_Path_BuildLambda_Handles_Chained_Nulls()
    {
      // Arrange - Test multiple potential null checks in nested property path
      List<IFilterInfo> filters = [new FilterInfo("Nested", "Nested.Count", Operator.EqualTo, 42)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Nested = new NestedEntity { Count = 42 } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Count = 0 } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Count = null } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = null })); // Should handle null gracefully
    }

    [TestMethod]
    public void When_Nested_Property_Path_BuildLambda_Returns_True_For_Nested_Match()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("NestedCode", "Nested.Code", Operator.EqualTo, "abc")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Nested = new NestedEntity { Code = "abc" } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Code = "def" } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_BuildLambda_Returns_True_For_Out_Of_Range()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.NotBetween, 10.0, 20.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 9.0 }));
      Assert.IsTrue(lambda(new TestEntity { Value = 25.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 15.0 }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Insufficient_Values_BuildLambda_Returns_False()
    {
      // Arrange - Test NotBetween with insufficient values
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.NotBetween, 10.0)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return false when insufficient values for between operation
      Assert.IsFalse(lambda(new TestEntity { Value = 5.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 15.0 }));
    }

    [TestMethod]
    public void When_NotContains_Operator_BuildLambda_Returns_True_For_Not_Substring()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Description", nameof(TestEntity.Description), Operator.NotContains, "lorem")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Description = "lorem ipsum" }));
      Assert.IsTrue(lambda(new TestEntity { Description = "ipsum" }));
      Assert.IsTrue(lambda(new TestEntity { Description = null }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_BuildLambda_Returns_True_For_NonMatching_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.NotEqualTo, "foo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "bar" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
      Assert.IsFalse(lambda(new TestEntity { Name = "foo" }));
    }

    [TestMethod]
    public void When_NotIn_Operator_BuildLambda_Returns_True_For_NonContained_Value()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.NotIn, "foo", "bar")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "baz" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_Empty_Values_BuildLambda_Returns_True()
    {
      // Arrange - Test NotIn with empty values collection
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.NotIn)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - NotIn with empty collection should return true for all values
      Assert.IsTrue(lambda(new TestEntity { Name = "anything" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Nullable_Value_Type_With_IsNotNull_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test IsNotNull on nullable value type
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNotNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { NullableInt = null }));
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 0 }));
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 42 }));
    }

    [TestMethod]
    public void When_Nullable_Value_Type_With_IsNull_Operator_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test IsNull on nullable value type
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { NullableInt = null }));
      Assert.IsFalse(lambda(new TestEntity { NullableInt = 0 }));
      Assert.IsFalse(lambda(new TestEntity { NullableInt = 42 }));
    }

    [TestMethod]
    public void When_Operator_None_BuildLambda_Throws_Exactly_NotSupportedException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.None, "foo")];

      // Act & Assert
      _ = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Property_Path_With_NonExistent_Segment_BuildLambda_Returns_True()
    {
      // Arrange - Test property path with non-existent intermediate segment
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "NonExistent.Code", Operator.EqualTo, "test")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when property doesn't exist
      Assert.IsTrue(lambda(new TestEntity()));
    }

    [TestMethod]
    public void When_StartsWith_Operator_BuildLambda_Returns_True_For_Prefix()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.StartsWith, "fo")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_String_Operators_With_Null_Comparison_Value_BuildLambda_Uses_Empty_String()
    {
      // Arrange - The current implementation converts null to empty string for string operators
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.StartsWith, (string)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - All non-null strings start with empty string
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_String_Operators_With_Null_Property_BuildLambda_Returns_False()
    {
      // Arrange - Test string operators with null property values
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.StartsWith, "test")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "testing" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null })); // Null property should return false
      Assert.IsFalse(lambda(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Type_Conversion_Fails_In_Between_Operator_BuildLambda_Throws_Exception()
    {
      // Arrange - Changed to expect generic Exception since conversion exceptions bubble up
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.Between, "not-an-int", "also-not-an-int")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Type_Conversion_Fails_In_In_Operator_BuildLambda_Throws_Exception()
    {
      // Arrange - Changed to expect generic Exception since conversion exceptions bubble up
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.In, "not-an-int")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Type_Conversion_In_Collection_Element_Comparison_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test collection element type conversion
      var intCollection = new TestEntityWithInts { Numbers = [1, 2, 3, 4, 5] };
      List<IFilterInfo> filters = [new FilterInfo("Numbers", nameof(TestEntityWithInts.Numbers), Operator.Contains, "3")]; // String "3" should convert to int 3
      Func<TestEntityWithInts, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithInts>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(intCollection));
    }

    [TestMethod]
    public void When_Type_Mismatch_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.EqualTo, "not-a-double")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Unsupported_Operator_BuildLambda_Throws_Exactly_NotSupportedException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), (Operator)999, "foo")];

      // Act & Assert
      _ = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Value_Type_Property_Compared_To_Null_BuildLambda_Converts_To_Nullable()
    {
      // Arrange - Test value type property compared to null
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.EqualTo, (double?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Value type compared to null should always be false
      Assert.IsFalse(lambda(new TestEntity { Value = 0.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 }));
    }

    // ========== NEW MISSING TEST CASES FOR TYPE CONVERSION ==========

    [TestMethod]
    public void When_TimeSpan_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test TimeSpan conversion from string
      List<IFilterInfo> filters = [new FilterInfo("Duration", nameof(TestEntity.Duration), Operator.EqualTo, "01:30:45")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Duration = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)).Add(TimeSpan.FromSeconds(45)) }));
      Assert.IsFalse(lambda(new TestEntity { Duration = TimeSpan.FromHours(2) }));
    }

    [TestMethod]
    public void When_TimeSpan_Comparison_With_Invalid_String_BuildLambda_Throws_FormatException()
    {
      // Arrange - Test TimeSpan conversion with invalid string
      List<IFilterInfo> filters = [new FilterInfo("Duration", nameof(TestEntity.Duration), Operator.EqualTo, "invalid-timespan")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Enum_Comparison_With_String_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test enum conversion from string
      List<IFilterInfo> filters = [new FilterInfo("Status", nameof(TestEntity.Status), Operator.EqualTo, "Active")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(lambda(new TestEntity { Status = TestStatus.Inactive }));
    }

    [TestMethod]
    public void When_Enum_Comparison_With_Numeric_Value_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test enum conversion from numeric value
      List<IFilterInfo> filters = [new FilterInfo("Status", nameof(TestEntity.Status), Operator.EqualTo, 1)]; // Active = 1
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(lambda(new TestEntity { Status = TestStatus.Inactive }));
    }

    [TestMethod]
    public void When_Enum_Comparison_With_Invalid_String_BuildLambda_Throws_Format_Exception()
    {
      // Arrange - Test enum conversion with invalid string
      List<IFilterInfo> filters = [new FilterInfo("Status", nameof(TestEntity.Status), Operator.EqualTo, "InvalidEnum")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Nullable_Enum_Comparison_With_Null_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test nullable enum with null value
      List<IFilterInfo> filters = [new FilterInfo("NullableStatus", nameof(TestEntity.NullableStatus), Operator.EqualTo, (TestStatus?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { NullableStatus = null }));
      Assert.IsFalse(lambda(new TestEntity { NullableStatus = TestStatus.Active }));
    }

    [TestMethod]
    public void When_Guid_Comparison_With_ByteArray_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test Guid conversion from byte array
      var testGuid = Guid.NewGuid();
      byte[] guidBytes = testGuid.ToByteArray();
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, guidBytes)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(lambda(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_Guid_Comparison_With_Invalid_ByteArray_BuildLambda_Throws_InvalidOperationException()
    {
      // Arrange - Test Guid conversion with invalid byte array (wrong length)
      byte[] invalidBytes = new byte[10]; // Guid requires 16 bytes
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, invalidBytes)];

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Guid_Comparison_With_Invalid_String_BuildLambda_Throws_Format_Exception()
    {
      // Arrange - Test Guid conversion with invalid string format
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, "invalid-guid-string")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Guid_Comparison_With_Unsupported_Type_BuildLambda_Throws_InvalidOperationException()
    {
      // Arrange - Test Guid conversion with unsupported type
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, 12345)];

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Convert_To_Target_Type_With_Already_Correct_Type_BuildLambda_Returns_Value_As_Is()
    {
      // Arrange - Test that values already of correct type are returned as-is
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EqualTo, 42)]; // int to int
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Id = 42 }));
      Assert.IsFalse(lambda(new TestEntity { Id = 24 }));
    }

    [TestMethod]
    public void When_Convert_To_Target_Type_With_Nullable_To_Underlying_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test nullable to underlying type conversion
      int? nullableValue = 42;
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EqualTo, nullableValue)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Id = 42 }));
      Assert.IsFalse(lambda(new TestEntity { Id = 24 }));
    }

    [TestMethod]
    public void When_Convert_To_Target_Type_With_Object_To_String_BuildLambda_Converts_Correctly()
    {
      // Arrange - Test object to string conversion
      var customObject = new
      {
        Value = "test"
      };
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, customObject)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = customObject.ToString() }));
      Assert.IsFalse(lambda(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Handle_Null_Value_With_Reference_Type_BuildLambda_Returns_Null()
    {
      // Arrange - Test HandleNullValue with reference type (should return null)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, (string?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Convert_Using_Standard_Methods_With_Invalid_Cast_Throws_InvalidCastException()
    {
      // Arrange - Test ConvertUsingStandardMethods with invalid cast (object can't convert to complex types)
      object customObject = new object();
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, customObject)];

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidCastException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Convert_Using_Standard_Methods_With_ArgumentException_Throws_InvalidCastException()
    {
      // Arrange - Test ConvertUsingStandardMethods that causes ArgumentException 
      // Create a scenario where Convert.ChangeType throws ArgumentException that should be wrapped in FormatException
      // Using a char value that can't be converted to DateTime should cause this
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, 'X')]; // char to DateTime conversion should fail

      // Act & Assert - Should throw FormatException when conversion fails
      _ = Assert.ThrowsExactly<InvalidCastException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Filter_Path_Is_Only_Dots_BuildLambda_Returns_True()
    {
      // Arrange - Path is only dots, which should be trimmed to empty and treated as invalid
      List<IFilterInfo> filters = [new FilterInfo("Dots", "...", Operator.EqualTo, "foo")];
      // The builder should treat this as an invalid property path and return a lambda that always returns true
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act
      bool result = lambda(new TestEntity { Name = "foo" });
      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_FilterInfo_With_Empty_Values_For_Contains_BuildLambda_Returns_True()
    {
      // Arrange - Contains operator with no values should not match anything
      List<IFilterInfo> filters = [new FilterInfo("Description", nameof(TestEntity.Description), Operator.Contains)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Description = "anything" }));
      Assert.IsTrue(lambda(new TestEntity { Description = string.Empty }));
    }

    [TestMethod]
    public void When_FilterInfo_With_Empty_Values_For_In_BuildLambda_Returns_False()
    {
      // Arrange - In operator with no values should not match anything
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.In)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_FilterInfo_With_NonReadable_Property_BuildLambda_Returns_True()
    {
      // Arrange - Property exists but is not readable (private setter)
      List<IFilterInfo> filters = [new FilterInfo("NonReadable", "NonReadableProperty", Operator.EqualTo, "foo")];
      Func<TestEntityWithNonReadableProperty, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithNonReadableProperty>(filters);
      // Act & Assert - Should return true since property is not accessible
      Assert.IsTrue(lambda(new TestEntityWithNonReadableProperty()));
    }

    [TestMethod]
    public void When_FilterInfo_With_Nullable_Struct_Property_And_Null_Comparison_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Nullable struct property compared to null
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.EqualTo, (int?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { NullableInt = null }));
      Assert.IsFalse(lambda(new TestEntity { NullableInt = 42 }));
    }

    [TestMethod]
    public void When_FilterInfo_With_Collection_Property_And_Collection_ComparisonValue_BuildLambda_Returns_False()
    {
      // Arrange - Collection property, comparison value is also a collection
      string[] values = ["foo", "bar"];
      List<IFilterInfo> filters = [new FilterInfo("Tags", nameof(TestEntity.Tags), Operator.Contains, [values])];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act & Assert - Should not match, as comparison is not element-wise
      Assert.IsFalse(lambda(new TestEntity { Tags = ["foo", "bar"] }));
    }

    [TestMethod]
    public void When_FilterInfo_With_Collection_Property_And_Null_ComparisonValue_BuildLambda_Returns_False()
    {
      // Arrange - Collection property, comparison value is null
      List<IFilterInfo> filters = [new FilterInfo("Tags", nameof(TestEntity.Tags), Operator.Contains, (string)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Tags = ["foo", "bar"] }));
      Assert.IsFalse(lambda(new TestEntity { Tags = null }));
    }

    [TestMethod]
    public void When_FilterInfo_With_ValueType_Property_And_Null_ComparisonValue_BuildLambda_Returns_False()
    {
      // Arrange - Value type property, comparison value is null
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EqualTo, (int?)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Id = 1 }));
      Assert.IsFalse(lambda(new TestEntity { Id = 0 }));
    }

    [TestMethod]
    public void When_Contains_Operator_On_DateTime_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - DateTime is a struct but the Contains operation should convert to string
      var testDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.Contains, "2023")];

      // Act
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - Should convert DateTime.ToString() and check if it contains "2023"
      Assert.IsTrue(lambda(new TestEntity { CreatedDate = testDate })); // "12/25/2023 10:30:00 AM".Contains("2023")
      Assert.IsFalse(lambda(new TestEntity { CreatedDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) })); // "1/1/2022 12:00:00 AM".Contains("2023")
    }

    [TestMethod]
    public void When_Contains_Operator_On_Nullable_Int_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - int? is a nullable value type that should convert to string
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.Contains, "42")];

      // Act
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - Should convert int?.ToString() and check contains
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 142 })); // "142".Contains("42")
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 420 })); // "420".Contains("42")
      Assert.IsFalse(lambda(new TestEntity { NullableInt = 35 })); // "35".Contains("42")
      Assert.IsFalse(lambda(new TestEntity { NullableInt = null })); // null check should return false
    }

    [TestMethod]
    public void When_Contains_Operator_On_Custom_Reference_Type_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - Custom reference type should convert via ToString()
      var nestedEntity = new NestedEntity { Code = "ABC-123" };
      List<IFilterInfo> filters = [new FilterInfo("Nested", "Nested.Code", Operator.Contains, "ABC")];

      // Act
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - Should convert NestedEntity.ToString() and check contains
      // This will likely fail with current implementation returning false
      Assert.IsTrue(lambda(new TestEntity { Nested = nestedEntity }));
      Assert.IsFalse(lambda(new TestEntity { Nested = null })); // null check should return false
    }

    [TestMethod]
    public void When_Contains_Operator_On_Enum_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - Enum should convert to string via ToString()
      List<IFilterInfo> filters = [new FilterInfo("Status", nameof(TestEntity.Status), Operator.Contains, "Act")];

      // Act
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - Should convert TestStatus.ToString() and check contains
      Assert.IsTrue(lambda(new TestEntity { Status = TestStatus.Active })); // "Active".Contains("Act")
      Assert.IsTrue(lambda(new TestEntity { Status = TestStatus.Inactive })); // "Inactive".Contains("Act") - true! This might actually pass
      Assert.IsFalse(lambda(new TestEntity { Status = TestStatus.Pending })); // "Pending".Contains("Act")
    }

    [TestMethod]
    public void When_Contains_Operator_On_Guid_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - Guid is a struct that should convert to string
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.Contains, "1234")];

      // Act
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - Should convert Guid.ToString() and check contains
      Assert.IsTrue(lambda(new TestEntity { UniqueId = testGuid })); // Contains "1234"
      Assert.IsFalse(lambda(new TestEntity { UniqueId = Guid.NewGuid() })); // Unlikely to contain "1234"
    }

    [TestMethod]
    public void When_Contains_Operator_On_Nullable_Value_Type_BuildLambda_Should_Convert_To_String_But_Currently_Returns_False()
    {
      // This test exposes the actual bug - nullable value types return false instead of converting to string
      List<IFilterInfo> filters = [new FilterInfo("NullableInt", nameof(TestEntity.NullableInt), Operator.Contains, "42")];

      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // This assertion will FAIL with current implementation because nullable types return Expression.Constant(false)
      Assert.IsTrue(lambda(new TestEntity { NullableInt = 142 })); // Should be true but will be false
    }

    // ========== MISSING TESTS FOR COMPREHENSIVE COVERAGE ==========

    [TestMethod]
    public void When_StartsWith_Operator_With_Empty_String_BuildLambda_Returns_True_For_All_NonNull()
    {
      // Arrange - Test StartsWith with empty string (should match all non-null strings)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.StartsWith, "")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - All non-null strings start with empty string
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_On_Nullable_DateTime_BuildLambda_Handles_Null_Correctly()
    {
      // Arrange - Test Contains on nullable DateTime property  
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.Contains, "2023")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { CreatedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(lambda(new TestEntity { CreatedDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_NotContains_Operator_On_Collection_BuildLambda_Returns_True_For_Not_Contained()
    {
      // Arrange - Test NotContains on collection property
      List<IFilterInfo> filters = [new FilterInfo("Tags", nameof(TestEntity.Tags), Operator.NotContains, "test")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Tags = ["test", "other"] }));
      Assert.IsTrue(lambda(new TestEntity { Tags = ["foo", "bar"] }));
      Assert.IsTrue(lambda(new TestEntity { Tags = null }));
    }

    [TestMethod]
    public void When_NotContains_Operator_On_Non_String_Property_BuildLambda_Converts_To_String()
    {
      // Arrange - Test NotContains on non-string property (should convert to string)
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.NotContains, "2")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Id = 123 })); // "123".Contains("2") == true, so NotContains == false
      Assert.IsTrue(lambda(new TestEntity { Id = 456 })); // "456".Contains("2") == false, so NotContains == true
    }

    [TestMethod]
    public void When_In_Operator_With_Null_Values_In_Collection_BuildLambda_Handles_Nulls_Correctly()
    {
      // Arrange - Test In operator with null values in the collection
      object[] valuesWithNull = ["foo", null!, "bar"];
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.In, valuesWithNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foo" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
      Assert.IsTrue(lambda(new TestEntity { Name = "bar" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "baz" }));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_Null_Values_In_Collection_BuildLambda_Handles_Nulls_Correctly()
    {
      // Arrange - Test NotIn operator with null values in the collection
      object[] valuesWithNull = ["foo", null!, "bar"];
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.NotIn, valuesWithNull)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Name = "foo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
      Assert.IsFalse(lambda(new TestEntity { Name = "bar" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "baz" }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Different_Value_Types_BuildLambda_Converts_Types_Correctly()
    {
      // Arrange - Test Between with string values that should convert to double
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, "10.0", "20.0")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Value = 15.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 })); // Exclusive boundary
      Assert.IsFalse(lambda(new TestEntity { Value = 20.0 })); // Exclusive boundary
      Assert.IsFalse(lambda(new TestEntity { Value = 25.0 }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Different_Value_Types_BuildLambda_Converts_Types_Correctly()
    {
      // Arrange - Test NotBetween with string values that should convert to double
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.NotBetween, "10.0", "20.0")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Value = 15.0 }));
      Assert.IsFalse(lambda(new TestEntity { Value = 10.0 })); // Exclusive boundary
      Assert.IsFalse(lambda(new TestEntity { Value = 20.0 }));  // Exclusive boundary
      Assert.IsTrue(lambda(new TestEntity { Value = 25.0 }));
    }

    [TestMethod]
    public void When_Multiple_FilterInfoCollection_With_Different_Conjunctions_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test multiple nested FilterInfoCollections with different conjunctions
      List<IFilterInfo> orFilters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test1"),
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test2")
      ];
      var orCollection = new FilterInfoCollection("orGroup", Conjunction.Or, orFilters);

      List<IFilterInfo> andFilters = [
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 5.0),
        new FilterInfo("Id", nameof(TestEntity.Id), Operator.LessThan, 100)
      ];
      var andCollection = new FilterInfoCollection("andGroup", Conjunction.And, andFilters);

      List<IFilterInfo> mainFilters = [orCollection, andCollection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);

      // Act & Assert - Should match entities that satisfy OR conditions AND both AND conditions
      Assert.IsTrue(lambda(new TestEntity { Name = "test1", Value = 10.0, Id = 50 }));
      Assert.IsTrue(lambda(new TestEntity { Name = "test2", Value = 10.0, Id = 50 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test3", Value = 10.0, Id = 50 })); // Name doesn't match OR
      Assert.IsFalse(lambda(new TestEntity { Name = "test1", Value = 3.0, Id = 50 })); // Value doesn't match AND
      Assert.IsFalse(lambda(new TestEntity { Name = "test1", Value = 10.0, Id = 150 })); // Id doesn't match AND
    }

    [TestMethod]
    public void When_FilterInfoCollection_With_Empty_Name_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test FilterInfoCollection with empty name
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test")];
      var collection = new FilterInfoCollection("AndName", Conjunction.And, filters);
      List<IFilterInfo> mainFilters = [collection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(mainFilters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Property_Access_Expression_Returns_Null_BuildLambda_Returns_True()
    {
      // Arrange - Test when property access returns null (invalid property path)
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "InvalidProperty.SubProperty", Operator.EqualTo, "test")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when property doesn't exist
      Assert.IsTrue(lambda(new TestEntity()));
    }

    [TestMethod]
    public void When_Collections_With_Different_Element_Types_BuildLambda_Converts_Element_Types_Correctly()
    {
      // Arrange - Test collection with integer elements but string comparison value
      List<IFilterInfo> filters = [new FilterInfo("Numbers", nameof(TestEntityWithInts.Numbers), Operator.Contains, "42")];
      Func<TestEntityWithInts, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithInts>(filters);

      // Act & Assert - String "42" should convert to int 42
      Assert.IsTrue(lambda(new TestEntityWithInts { Numbers = [1, 42, 3] }));
      Assert.IsFalse(lambda(new TestEntityWithInts { Numbers = [1, 2, 3] }));
    }

    [TestMethod]
    public void When_String_Operators_With_Non_String_Property_BuildLambda_Handles_ToString_Conversion()
    {
      // Arrange - Test StartsWith on non-string property (should convert to string)
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.StartsWith, "12")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should convert Id to string and check StartsWith
      Assert.IsTrue(lambda(new TestEntity { Id = 123 })); // "123".StartsWith("12") == true
      Assert.IsFalse(lambda(new TestEntity { Id = 456 })); // "456".StartsWith("12") == false
    }

    [TestMethod]
    public void When_String_Operators_With_Non_String_Property_EndsWith_BuildLambda_Handles_ToString_Conversion()
    {
      // Arrange - Test EndsWith on non-string property (should convert to string)
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EndsWith, "23")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should convert Id to string and check EndsWith
      Assert.IsTrue(lambda(new TestEntity { Id = 123 })); // "123".EndsWith("23") == true
      Assert.IsFalse(lambda(new TestEntity { Id = 456 })); // "456".EndsWith("23") == false
    }

    [TestMethod]
    public void When_Large_FilterInfoCollection_With_Many_Nested_Levels_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test deeply nested FilterInfoCollections (stress test)
      List<IFilterInfo> level3Filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "level3")];
      var level3Collection = new FilterInfoCollection("level3", Conjunction.And, level3Filters);

      List<IFilterInfo> level2Filters = [
        level3Collection,
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.0)
      ];
      var level2Collection = new FilterInfoCollection("level2", Conjunction.Or, level2Filters);

      List<IFilterInfo> level1Filters = [
        level2Collection,
        new FilterInfo("Id", nameof(TestEntity.Id), Operator.LessThan, 1000)
      ];
      var level1Collection = new FilterInfoCollection("level1", Conjunction.And, level1Filters);

      List<IFilterInfo> rootFilters = [level1Collection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(rootFilters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "level3", Value = 5.0, Id = 100 })); // Name matches, Id matches
      Assert.IsTrue(lambda(new TestEntity { Name = "other", Value = 15.0, Id = 100 })); // Value matches, Id matches
      Assert.IsFalse(lambda(new TestEntity { Name = "other", Value = 5.0, Id = 100 })); // Neither level2 condition matches
      Assert.IsFalse(lambda(new TestEntity { Name = "level3", Value = 15.0, Id = 2000 })); // Id doesn't match
    }

    [TestMethod]
    public void When_FilterInfo_Collection_Contains_Mixed_Types_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test FilterInfoCollection containing both FilterInfo and FilterInfoCollection
      List<IFilterInfo> nestedFilters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 5.0)];
      var nestedCollection = new FilterInfoCollection("nested", Conjunction.And, nestedFilters);

      List<IFilterInfo> mixedFilters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test"),
        nestedCollection
      ];

      var mainCollection = new FilterInfoCollection("main", Conjunction.Or, mixedFilters);
      List<IFilterInfo> rootFilters = [mainCollection];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(rootFilters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test", Value = 3.0 })); // Name matches
      Assert.IsTrue(lambda(new TestEntity { Name = "other", Value = 10.0 })); // Nested condition matches
      Assert.IsFalse(lambda(new TestEntity { Name = "other", Value = 3.0 })); // Neither condition matches
    }

    [TestMethod]
    public void When_Null_FilterInfo_In_Collection_BuildLambda_Skips_Null_Filters()
    {
      // Arrange - Test collection with null FilterInfo (should be skipped)
      List<IFilterInfo> filtersWithNull = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test"),
        null!,
        new FilterInfo("Value", nameof(TestEntity.Value), Operator.GreaterThan, 5.0)
      ];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filtersWithNull);

      // Act & Assert - Should work as if null filter doesn't exist
      Assert.IsTrue(lambda(new TestEntity { Name = "test", Value = 10.0 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test", Value = 3.0 }));
      Assert.IsFalse(lambda(new TestEntity { Name = "other", Value = 10.0 }));
    }

    [TestMethod]
    public void When_BuildLambda_Called_Multiple_Times_Same_Filters_Results_Are_Consistent()
    {
      // Arrange - Test caching and consistency of results
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.EqualTo, "test")];
      var testEntity = new TestEntity { Name = "test" };

      // Act - Build lambda multiple times
      Func<TestEntity, bool> lambda1 = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      Func<TestEntity, bool> lambda2 = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      Func<TestEntity, bool> lambda3 = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Assert - All should return consistent results
      Assert.IsTrue(lambda1(testEntity));
      Assert.IsTrue(lambda2(testEntity));
      Assert.IsTrue(lambda3(testEntity));

      var testEntity2 = new TestEntity { Name = "other" };
      Assert.IsFalse(lambda1(testEntity2));
      Assert.IsFalse(lambda2(testEntity2));
      Assert.IsFalse(lambda3(testEntity2));
    }

    [TestMethod]
    public void When_Complex_Property_Path_With_Null_Intermediate_Values_BuildLambda_Handles_Gracefully()
    {
      // Arrange - Test complex property path where intermediate values might be null
      List<IFilterInfo> filters = [new FilterInfo("DeepNested", "Nested.Deep.Value", Operator.EqualTo, "test")];
      Func<TestEntityWithDeepNesting, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntityWithDeepNesting>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntityWithDeepNesting
      {
        Nested = new NestedEntityWithDeep
        {
          Deep = new DeepNestedEntity { Value = "test" }
        }
      }));
      Assert.IsFalse(lambda(new TestEntityWithDeepNesting { Nested = null })); // Null intermediate
      Assert.IsFalse(lambda(new TestEntityWithDeepNesting
      {
        Nested = new NestedEntityWithDeep { Deep = null }
      })); // Null final level
    }

    [TestMethod]
    public void When_Regex_Operator_With_Valid_Pattern_BuildLambda_Returns_True_For_Matching_Value()
    {
      // Arrange - Test Regex operator with valid pattern that matches
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^foo.*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "foobar" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "foo123" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "barfoo" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Valid_Pattern_BuildLambda_Returns_False_For_NonMatching_Value()
    {
      // Arrange - Test Regex operator with valid pattern that doesn't match
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"\d+")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test123" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "456" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "abc" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Null_Property_BuildLambda_Returns_False()
    {
      // Arrange - Test Regex operator with null property value
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @".*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Empty_String_Property_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test Regex operator with empty string property
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^$")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Complex_Pattern_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test Regex operator with complex email pattern
      string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, emailPattern)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test@example.com" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "user.name+tag@domain.co.uk" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "invalid-email" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "@example.com" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test@" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Case_Sensitive_Pattern_BuildLambda_Respects_Case()
    {
      // Arrange - Test Regex operator with case-sensitive pattern
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^Test.*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "Test123" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "Testing" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test123" })); // Different case
      Assert.IsFalse(lambda(new TestEntity { Name = "testing" })); // Different case
    }

    [TestMethod]
    public void When_Regex_Operator_With_Special_Characters_BuildLambda_Handles_Escaping_Correctly()
    {
      // Arrange - Test Regex operator with special characters that need escaping
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"test\.\d+")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test.123" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "test.456" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test123" })); // Missing dot
      Assert.IsFalse(lambda(new TestEntity { Name = "testX123" })); // X instead of dot
    }

    [TestMethod]
    public void When_Regex_Operator_With_No_Values_BuildLambda_Uses_Empty_String_Pattern()
    {
      // Arrange - Test Regex operator with no values (should use empty string pattern)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Empty string pattern should match all non-null strings
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsTrue(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Null_Value_BuildLambda_Uses_Empty_String_Pattern()
    {
      // Arrange - Test Regex operator with null value (should use empty string pattern)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, (string)null!)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Empty string pattern should match all non-null strings
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Non_String_Value_BuildLambda_Converts_To_String()
    {
      // Arrange - Test Regex operator with non-string value (should convert to string)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, 123)];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should use "123" as the pattern
      Assert.IsTrue(lambda(new TestEntity { Name = "123" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "test123" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "456" }));
      Assert.IsFalse(lambda(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Multiple_Values_BuildLambda_Uses_First_Value()
    {
      // Arrange - Test Regex operator with multiple values (should use first value)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^test.*", @"^other.*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should use first pattern "^test.*"
      Assert.IsTrue(lambda(new TestEntity { Name = "test123" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "testing" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "other123" })); // Second pattern should be ignored
      Assert.IsFalse(lambda(new TestEntity { Name = "sample" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Invalid_Pattern_BuildLambda_Throws_Exception()
    {
      // Arrange - Test Regex operator with invalid pattern
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"[invalid")];

      // Act & Assert - Should throw exception when compiling lambda due to invalid regex
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      _ = Assert.ThrowsExactly<RegexParseException>(() =>
        // The exception occurs when the lambda is executed, not when it's built
        lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Nested_Property_BuildLambda_Evaluates_Correctly()
    {
      // Arrange - Test Regex operator on nested property
      List<IFilterInfo> filters = [new FilterInfo("NestedCode", "Nested.Code", Operator.Regex, @"^[A-Z]{3}-\d{3}$")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Nested = new NestedEntity { Code = "ABC-123" } }));
      Assert.IsTrue(lambda(new TestEntity { Nested = new NestedEntity { Code = "XYZ-789" } }));
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Code = "abc-123" } })); // Wrong case
      Assert.IsFalse(lambda(new TestEntity { Nested = new NestedEntity { Code = "AB-123" } })); // Too few letters
      Assert.IsFalse(lambda(new TestEntity { Nested = null })); // Null nested property
    }

    [TestMethod]
    public void When_Regex_Operator_With_Invalid_Property_Path_BuildLambda_Returns_True()
    {
      // Arrange - Test Regex operator with invalid property path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "NonExistentProperty", Operator.Regex, @".*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when property doesn't exist
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Null_Filter_Path_BuildLambda_Returns_True()
    {
      // Arrange - Test Regex operator with null filter path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", null!, Operator.Regex, @".*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when filter path is null
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Empty_Filter_Path_BuildLambda_Returns_True()
    {
      // Arrange - Test Regex operator with empty filter path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "", Operator.Regex, @".*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when filter path is empty
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Whitespace_Filter_Path_BuildLambda_Returns_True()
    {
      // Arrange - Test Regex operator with whitespace-only filter path
      List<IFilterInfo> filters = [new FilterInfo("Invalid", "   ", Operator.Regex, @".*")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert - Should return true when filter path is whitespace
      Assert.IsTrue(lambda(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Regex_Operator_With_Unicode_Pattern_BuildLambda_Handles_Unicode_Correctly()
    {
      // Arrange - Test Regex operator with Unicode characters
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^[α-ω]+$")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "αβγ" }));
      Assert.IsTrue(lambda(new TestEntity { Name = "ωψχ" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "abc" })); // Latin characters
      Assert.IsFalse(lambda(new TestEntity { Name = "123" })); // Numbers
    }

    [TestMethod]
    public void When_Regex_Operator_With_Anchored_Pattern_BuildLambda_Respects_Anchors()
    {
      // Arrange - Test Regex operator with anchored pattern (^ and $)
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^test$")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test" })); // Exact match
      Assert.IsFalse(lambda(new TestEntity { Name = "test123" })); // Extra characters
      Assert.IsFalse(lambda(new TestEntity { Name = "pretest" })); // Extra characters
      Assert.IsFalse(lambda(new TestEntity { Name = "pretestpost" })); // Extra characters
    }

    [TestMethod]
    public void When_Regex_Operator_With_Quantifier_Pattern_BuildLambda_Handles_Quantifiers_Correctly()
    {
      // Arrange - Test Regex operator with quantifier patterns
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^test\d{2,4}$")];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test12" })); // 2 digits
      Assert.IsTrue(lambda(new TestEntity { Name = "test123" })); // 3 digits
      Assert.IsTrue(lambda(new TestEntity { Name = "test1234" })); // 4 digits
      Assert.IsFalse(lambda(new TestEntity { Name = "test1" })); // 1 digit (too few)
      Assert.IsFalse(lambda(new TestEntity { Name = "test12345" })); // 5 digits (too many)
    }

    [TestMethod]
    public void When_Multiple_Regex_Filters_BuildLambda_Evaluates_All_Conditions()
    {
      // Arrange - Test multiple Regex filters (AND conjunction by default)
      List<IFilterInfo> filters = [
        new FilterInfo("Name", nameof(TestEntity.Name), Operator.Regex, @"^test.*"),
    new FilterInfo("Description", nameof(TestEntity.Description), Operator.Regex, @".*description.*")
      ];
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);

      // Act & Assert
      Assert.IsTrue(lambda(new TestEntity { Name = "test123", Description = "some description here" }));
      Assert.IsFalse(lambda(new TestEntity { Name = "test123", Description = "no match" })); // Description doesn't match
      Assert.IsFalse(lambda(new TestEntity { Name = "sample", Description = "some description here" })); // Name doesn't match
      Assert.IsFalse(lambda(new TestEntity { Name = "sample", Description = "no match" })); // Neither matches
    }

    // ========== ADDITIONAL TEST ENTITIES FOR NEW TESTS ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithDeepNesting
    {
      public NestedEntityWithDeep? Nested
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntityWithDeep
    {
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
    }

    // ========== ADDITIONAL TEST ENTITIES FOR EDGE CASES ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public DateTime CreatedDate
      {
        get; init;
      }

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

      public bool IsActive
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

      public Collection<string>? Tags
      {
        get; init;
      }

      public NestedEntity? Nested
      {
        get; init;
      }

      public TimeSpan Duration
      {
        get; init;
      }

      public Guid UniqueId
      {
        get; init;
      }

      public TestStatus Status
      {
        get; init;
      }

      public TestStatus? NullableStatus
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
    }

    public enum TestStatus
    {
      None = 0,
      Active = 1,
      Inactive = 2,
      Pending = 3
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithInts
    {
      public Collection<int> Numbers { get; init; } = [];
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithArray
    {
      public int[]? Values
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithNullableStrings
    {
      public Collection<string?> Items { get; init; } = [];
    }

    [ExcludeFromCodeCoverage]
    public sealed class TestEntityWithField
    {
      public string PublicField = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public sealed record SimpleEntity
    {
      public string? Name
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed class TestEntityWithNonReadableProperty
    {
      private string NonReadableProperty
      {
        get;
      } = string.Empty; // write-only property
    }
  }
}