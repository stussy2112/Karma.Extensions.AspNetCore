// -----------------------------------------------------------------------
// <copyright file="MembershipOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="MembershipOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class MembershipOperatorHandlerTests
  {
    private MembershipOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new MembershipOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_In_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.In;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_NotIn_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.NotIn;

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

    // ========== BuildExpression Tests - In Operator with Primitive Types ==========

    [TestMethod]
    public void When_In_Operator_With_Int_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In, 1, 2, 3);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 2 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 3 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 4 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
    }

    [TestMethod]
    public void When_In_Operator_With_String_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.In, "John", "Jane", "Bob");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "John" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Jane" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Bob" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Alice" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_In_Operator_With_Decimal_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Price", nameof(TestEntity.Price), Operator.In, 10.5m, 20.75m, 30.0m);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Price = 10.5m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 20.75m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 30.0m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 15.0m }));
    }

    [TestMethod]
    public void When_In_Operator_With_Bool_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("IsActive", nameof(TestEntity.IsActive), Operator.In, true);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { IsActive = true }));
      Assert.IsFalse(compiled(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_In_Operator_With_DateTime_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      DateTime date1 = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
      DateTime date2 = new(2023, 6, 15, 0, 0, 0, DateTimeKind.Unspecified);
      DateTime date3 = new(2023, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.In, date1, date2, date3);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = date1 }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = date2 }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = date3 }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_In_Operator_With_Guid_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var guid1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
      var guid2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
      var guid3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
      FilterInfo filter = new("Identifier", nameof(TestEntity.Identifier), Operator.In, guid1, guid2, guid3);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Identifier = guid1 }));
      Assert.IsTrue(compiled(new TestEntity { Identifier = guid2 }));
      Assert.IsTrue(compiled(new TestEntity { Identifier = guid3 }));
      Assert.IsFalse(compiled(new TestEntity { Identifier = Guid.NewGuid() }));
    }

    // ========== BuildExpression Tests - In Operator with Enum Types ==========

    [TestMethod]
    public void When_In_Operator_With_Enum_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.In, TestStatus.Active, TestStatus.Pending);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Pending }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.None }));
    }

    [TestMethod]
    public void When_In_Operator_With_Enum_Property_And_String_Values_BuildExpression_Converts_And_Matches()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.In, "Active", "Pending");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Pending }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
    }

    // ========== BuildExpression Tests - In Operator with Nullable Types ==========

    [TestMethod]
    public void When_In_Operator_With_Nullable_Int_Property_BuildExpression_Returns_True_For_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableId", nameof(TestEntity.NullableId), Operator.In, 10, 20, 30);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableId = 10 }));
      Assert.IsTrue(compiled(new TestEntity { NullableId = 20 }));
      Assert.IsTrue(compiled(new TestEntity { NullableId = 30 }));
      Assert.IsFalse(compiled(new TestEntity { NullableId = 40 }));
      Assert.IsFalse(compiled(new TestEntity { NullableId = null }));
    }

    [TestMethod]
    public void When_In_Operator_With_Nullable_Int_Property_Including_Null_BuildExpression_Returns_True_For_Null()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableId", nameof(TestEntity.NullableId), Operator.In, 10, null!, 30);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableId = 10 }));
      Assert.IsTrue(compiled(new TestEntity { NullableId = null }));
      Assert.IsTrue(compiled(new TestEntity { NullableId = 30 }));
      Assert.IsFalse(compiled(new TestEntity { NullableId = 20 }));
    }

    // ========== BuildExpression Tests - NotIn Operator ==========

    [TestMethod]
    public void When_NotIn_Operator_With_Int_Property_BuildExpression_Returns_True_For_Non_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotIn, 1, 2, 3);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Id = 1 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 2 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 3 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 4 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_String_Property_BuildExpression_Returns_True_For_Non_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotIn, "John", "Jane", "Bob");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = "John" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Jane" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Bob" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Alice" }));
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_Enum_Property_BuildExpression_Returns_True_For_Non_Matching_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.NotIn, TestStatus.Active, TestStatus.Pending);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Pending }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Inactive }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.None }));
    }

    // ========== BuildExpression Tests - Nested Properties ==========

    [TestMethod]
    public void When_In_Operator_With_Nested_Int_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedId", "Nested.Id", Operator.In, 100, 200, 300);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 100 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 200 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 300 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Id = 400 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_In_Operator_With_Nested_String_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.In, "ABC", "DEF", "GHI");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "DEF" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "GHI" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ" } }));
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
      FilterInfo filter = new("Id", null!, Operator.In, 1, 2, 3);

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
      FilterInfo filter = new("Id", "", Operator.In, 1, 2, 3);

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
      FilterInfo filter = new("Id", "   ", Operator.In, 1, 2, 3);

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.In, 1, 2, 3);

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
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.In, 1, 2, 3);

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
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, 1);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("MembershipOperatorHandler", StringComparison.Ordinal));
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

      Assert.IsTrue(ex.Message.Contains("MembershipOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("GreaterThan", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_None_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.None);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("MembershipOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Edge Cases ==========

    [TestMethod]
    public void When_In_Operator_With_Single_Value_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 42 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 43 }));
    }

    [TestMethod]
    public void When_In_Operator_With_Empty_Values_BuildExpression_Returns_False_For_All()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Empty values array means nothing matches
      Assert.IsFalse(compiled(new TestEntity { Id = 1 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 2 }));
    }

    [TestMethod]
    public void When_In_Operator_With_Duplicate_Values_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In, 1, 1, 2, 2, 3);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 2 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 3 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 4 }));
    }

    [TestMethod]
    public void When_In_Operator_With_Type_Conversion_BuildExpression_Converts_Values()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In, "1", "2", "3");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - String values should be converted to int
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 2 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 3 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 4 }));
    }

    [TestMethod]
    public void When_In_Operator_With_Mixed_Null_And_Non_Null_String_Values_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.In, "John", null!, "Jane");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "John" }));
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Jane" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "Bob" }));
    }

    [TestMethod]
    public void When_NotIn_Operator_With_Empty_Values_BuildExpression_Returns_True_For_All()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotIn);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Empty values array with NotIn means everything matches
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 2 }));
    }

    [TestMethod]
    public void When_In_Operator_With_Large_Number_Of_Values_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      object[] values = [.. Enumerable.Range(1, 100).Cast<object>()];
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.In, values);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 50 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 100 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 101 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public decimal Price { get; init; }
      public bool IsActive { get; init; }
      public DateTime CreatedDate { get; init; }
      public Guid Identifier { get; init; }
      public TestStatus Status { get; init; }
      public int? NullableId { get; init; }
      public NestedEntity? Nested { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public int Id { get; init; }
      public string? Code { get; init; }
    }

    public enum TestStatus
    {
      None = 0,
      Active = 1,
      Inactive = 2,
      Pending = 3
    }
  }
}
