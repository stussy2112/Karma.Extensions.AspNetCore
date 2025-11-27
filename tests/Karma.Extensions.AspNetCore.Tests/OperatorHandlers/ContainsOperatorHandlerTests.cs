// -----------------------------------------------------------------------
// <copyright file="ContainsOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="ContainsOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class ContainsOperatorHandlerTests
  {
    private ContainsOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new ContainsOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_Contains_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.Contains;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_NotContains_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.NotContains;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_EqualTo_CanHandle_Returns_False()
    {
      // Arrange
      Operator op = Operator.EqualTo;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_Operator_Is_GreaterThan_CanHandle_Returns_False()
    {
      // Arrange
      Operator op = Operator.GreaterThan;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_Operator_Is_None_CanHandle_Returns_False()
    {
      // Arrange
      Operator op = Operator.None;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsFalse(result);
    }

    // ========== BuildExpression Tests - Contains Operator with String Properties ==========

    [TestMethod]
    public void When_Contains_Operator_With_String_Property_BuildExpression_Returns_True_For_Substring()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "John");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "John Doe" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Johnny" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Jane" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_String_Property_Is_Case_Insensitive()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "john");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "JOHN DOE" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "John Doe" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "johnny" }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Empty_String_Search_Value_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, string.Empty);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Empty string is contained in all non-null strings
      Assert.IsTrue(compiled(new TestEntity { Name = "John" }));
      Assert.IsTrue(compiled(new TestEntity { Name = string.Empty }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Null_String_Property_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    // ========== BuildExpression Tests - NotContains Operator with String Properties ==========

    [TestMethod]
    public void When_NotContains_Operator_With_String_Property_BuildExpression_Returns_True_For_Non_Substring()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotContains, "John");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = "John Doe" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Johnny" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Jane" }));
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_NotContains_Operator_With_String_Property_Is_Case_Insensitive()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotContains, "john");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = "JOHN DOE" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "John Doe" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "johnny" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Jane" }));
    }

    // ========== BuildExpression Tests - Contains Operator with Collection Properties ==========

    [TestMethod]
    public void When_Contains_Operator_With_List_Property_BuildExpression_Returns_True_For_Contained_Item()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Tags", nameof(TestEntity.Tags), Operator.Contains, "important");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Tags = ["important", "urgent"] }));
      Assert.IsFalse(compiled(new TestEntity { Tags = ["normal", "low"] }));
      Assert.IsFalse(compiled(new TestEntity { Tags = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Array_Property_BuildExpression_Returns_True_For_Contained_Item()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Codes", nameof(TestEntity.Codes), Operator.Contains, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Codes = [1, 42, 100] }));
      Assert.IsFalse(compiled(new TestEntity { Codes = [1, 2, 3] }));
      Assert.IsFalse(compiled(new TestEntity { Codes = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Empty_Collection_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Tags", nameof(TestEntity.Tags), Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Tags = [] }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Null_Collection_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Tags", nameof(TestEntity.Tags), Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Tags = null }));
    }

    // ========== BuildExpression Tests - NotContains Operator with Collection Properties ==========

    [TestMethod]
    public void When_NotContains_Operator_With_List_Property_BuildExpression_Returns_True_For_Non_Contained_Item()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Tags", nameof(TestEntity.Tags), Operator.NotContains, "important");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Tags = ["important", "urgent"] }));
      Assert.IsTrue(compiled(new TestEntity { Tags = ["normal", "low"] }));
      Assert.IsTrue(compiled(new TestEntity { Tags = null }));
    }

    [TestMethod]
    public void When_NotContains_Operator_With_Array_Property_BuildExpression_Returns_True_For_Non_Contained_Item()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Codes", nameof(TestEntity.Codes), Operator.NotContains, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Codes = [1, 42, 100] }));
      Assert.IsTrue(compiled(new TestEntity { Codes = [1, 2, 3] }));
      Assert.IsTrue(compiled(new TestEntity { Codes = null }));
    }

    // ========== BuildExpression Tests - Contains Operator with Non-String/Non-Collection Properties ==========

    [TestMethod]
    public void When_Contains_Operator_With_Int_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Contains, "42");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 42 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 142 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 420 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 100 }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_DateTime_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.Contains, "2023");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Decimal_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Price", nameof(TestEntity.Price), Operator.Contains, "99");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Price = 99.99m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 199.50m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 50.00m }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Bool_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("IsActive", nameof(TestEntity.IsActive), Operator.Contains, "True");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { IsActive = true }));
      Assert.IsFalse(compiled(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Nullable_Int_Property_With_Null_Value_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableId", nameof(TestEntity.NullableId), Operator.Contains, "42");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableId = 42 }));
      Assert.IsTrue(compiled(new TestEntity { NullableId = 142 }));
      Assert.IsFalse(compiled(new TestEntity { NullableId = null }));
    }

    // ========== BuildExpression Tests - Contains Operator with Nested Properties ==========

    [TestMethod]
    public void When_Contains_Operator_With_Nested_String_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.Contains, "ABC");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC123" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZABC" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Nested_Collection_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedItems", "Nested.Items", Operator.Contains, "item1");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Items = ["item1", "item2"] } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Items = ["item2", "item3"] } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    // ========== BuildExpression Tests - Null/Invalid Filter Cases ==========

    [TestMethod]
    public void When_Filter_Is_Null_BuildExpression_Returns_True_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = null!;

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Filter_Path_Is_Null_BuildExpression_Returns_True_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", null!, Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Filter_Path_Is_Empty_BuildExpression_Returns_True_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", "", Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Filter_Path_Is_Whitespace_BuildExpression_Returns_True_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", "   ", Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Property_Does_Not_Exist_BuildExpression_Returns_True_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return true constant when property doesn't exist
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Property_Path_Is_Invalid_BuildExpression_Returns_True_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.Contains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return true constant when property path is invalid
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    // ========== BuildExpression Tests - Unsupported Operator ==========

    [TestMethod]
    public void When_Operator_Is_EqualTo_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("ContainsOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("EqualTo", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_GreaterThan_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, 10);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("ContainsOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("GreaterThan", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_None_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.None);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("ContainsOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Edge Cases ==========

    [TestMethod]
    public void When_Contains_Operator_With_Null_Comparison_Value_Uses_Empty_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - null comparison value treated as empty string
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = string.Empty }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Special_Characters_In_Search_Value_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "@#$");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test@#$value" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Whitespace_In_Search_Value_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "John Doe");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "John Doe" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Mr. John Doe Jr." }));
      Assert.IsFalse(compiled(new TestEntity { Name = "JohnDoe" }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Guid_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
      FilterInfo filter = new("Identifier", nameof(TestEntity.Identifier), Operator.Contains, "1234-123456789012");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Identifier = testGuid }));
      Assert.IsFalse(compiled(new TestEntity { Identifier = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Enum_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.Contains, "ctiv");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - "ctiv" is contained in both "Active" and "Inactive"
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Inactive }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.None }));
    }

    [TestMethod]
    public void When_NotContains_Operator_With_Empty_Collection_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Tags", nameof(TestEntity.Tags), Operator.NotContains, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Tags = [] }));
    }

    [TestMethod]
    public void When_Contains_Operator_With_Collection_Of_Integers_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Numbers", nameof(TestEntity.Numbers), Operator.Contains, 5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Numbers = [1, 5, 10] }));
      Assert.IsFalse(compiled(new TestEntity { Numbers = [1, 2, 3] }));
      Assert.IsFalse(compiled(new TestEntity { Numbers = null }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public DateTime CreatedDate { get; init; }
      public decimal Price { get; init; }
      public bool IsActive { get; init; }
      public int? NullableId { get; init; }
      public Guid Identifier { get; init; }
      public TestStatus Status { get; init; }
      public List<string>? Tags { get; init; }
      public int[]? Codes { get; init; }
      public List<int>? Numbers { get; init; }
      public NestedEntity? Nested { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public string? Code { get; init; }
      public List<string>? Items { get; init; }
    }

    public enum TestStatus
    {
      None = 0,
      Active = 1,
      Inactive = 2
    }
  }
}
