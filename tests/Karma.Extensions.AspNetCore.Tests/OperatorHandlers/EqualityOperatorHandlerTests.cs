// -----------------------------------------------------------------------
// <copyright file="EqualityOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="EqualityOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class EqualityOperatorHandlerTests
  {
    private EqualityOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new EqualityOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_EqualTo_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.EqualTo;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_NotEqualTo_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.NotEqualTo;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
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
    public void When_Operator_Is_StartsWith_CanHandle_Returns_False()
    {
      // Arrange
      Operator op = Operator.StartsWith;

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

    // ========== BuildExpression Tests - EqualTo Operator ==========

    [TestMethod]
    public void When_EqualTo_Operator_With_Valid_String_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "other" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 42 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 100 }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.EqualTo, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.0 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 11.0 }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Boolean_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("IsActive", nameof(TestEntity.IsActive), Operator.EqualTo, true);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { IsActive = true }));
      Assert.IsFalse(compiled(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = DateTime.Now }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Guid_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      FilterInfo filter = new("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, testGuid);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(compiled(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Enum_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.EqualTo, TestStatus.Active);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Pending }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Nullable_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.EqualTo, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableInt = 42 }));
      Assert.IsFalse(compiled(new TestEntity { NullableInt = null }));
      Assert.IsFalse(compiled(new TestEntity { NullableInt = 100 }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Null_Value_BuildExpression_Handles_Null_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, (string?)null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.EqualTo, "ABC-123");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC-123" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ-789" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, "42");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 42 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 24 }));
    }

    [TestMethod]
    public void When_EqualTo_Operator_With_Empty_String_BuildExpression_Matches_Empty_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, "");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    // ========== BuildExpression Tests - NotEqualTo Operator ==========

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Valid_String_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotEqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "other" }));
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotEqualTo, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Id = 42 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 100 }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.NotEqualTo, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 10.0 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 11.0 }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Boolean_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("IsActive", nameof(TestEntity.IsActive), Operator.NotEqualTo, true);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { IsActive = true }));
      Assert.IsTrue(compiled(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.NotEqualTo, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = DateTime.Now }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Guid_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      FilterInfo filter = new("UniqueId", nameof(TestEntity.UniqueId), Operator.NotEqualTo, testGuid);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { UniqueId = testGuid }));
      Assert.IsTrue(compiled(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Enum_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.NotEqualTo, TestStatus.Active);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Inactive }));
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Pending }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Nullable_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.NotEqualTo, 42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { NullableInt = 42 }));
      Assert.IsTrue(compiled(new TestEntity { NullableInt = null }));
      Assert.IsTrue(compiled(new TestEntity { NullableInt = 100 }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Null_Value_BuildExpression_Handles_Null_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotEqualTo, (string?)null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.NotEqualTo, "ABC-123");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC-123" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ-789" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotEqualTo, "42");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Id = 42 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 24 }));
    }

    [TestMethod]
    public void When_NotEqualTo_Operator_With_Empty_String_BuildExpression_Matches_NonEmpty()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotEqualTo, "");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Name = "" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
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
      FilterInfo filter = new("Name", null!, Operator.EqualTo, "test");

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
      FilterInfo filter = new("Name", "", Operator.EqualTo, "test");

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
      FilterInfo filter = new("Name", "   ", Operator.EqualTo, "test");

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.EqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should return true when property doesn't exist
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_Property_Path_Is_Invalid_BuildExpression_Returns_True_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.EqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should return true when property path is invalid
      Assert.IsTrue(compiled(new TestEntity()));
    }

    // ========== BuildExpression Tests - Unsupported Operator ==========

    [TestMethod]
    public void When_Operator_Is_Not_Supported_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.GreaterThan, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("EqualityOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("GreaterThan", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_StartsWith_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("EqualityOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("StartsWith", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_None_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.None, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("EqualityOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Type Conversion ==========

    [TestMethod]
    public void When_EqualTo_With_Enum_String_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.EqualTo, "Active");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
    }

    [TestMethod]
    public void When_EqualTo_With_Enum_Numeric_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.EqualTo, 1);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
    }

    [TestMethod]
    public void When_EqualTo_With_DateTime_String_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, "2023-12-25");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = DateTime.Now }));
    }

    [TestMethod]
    public void When_EqualTo_With_Guid_String_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      FilterInfo filter = new("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, "12345678-1234-5678-9abc-123456789abc");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(compiled(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_EqualTo_With_Boolean_String_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("IsActive", nameof(TestEntity.IsActive), Operator.EqualTo, "true");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { IsActive = true }));
      Assert.IsFalse(compiled(new TestEntity { IsActive = false }));
    }

    [TestMethod]
    public void When_EqualTo_With_Double_String_Value_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.EqualTo, "10.5");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.0 }));
    }

    // ========== BuildExpression Tests - Special Cases ==========

    [TestMethod]
    public void When_EqualTo_With_Zero_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, 0);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 1 }));
    }

    [TestMethod]
    public void When_EqualTo_With_Negative_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, -42);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = -42 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 42 }));
    }

    [TestMethod]
    public void When_EqualTo_With_Max_Int_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, int.MaxValue);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = int.MaxValue }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
    }

    [TestMethod]
    public void When_EqualTo_With_Null_Nested_Property_BuildExpression_Handles_Gracefully()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.EqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = null } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "test" } }));
    }

    [TestMethod]
    public void When_NotEqualTo_With_Null_Nested_Property_BuildExpression_Handles_Gracefully()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.NotEqualTo, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = null } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "test" } }));
    }

    [TestMethod]
    public void When_EqualTo_With_Multiple_Values_BuildExpression_Uses_First_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, "test", "other");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first value "test"
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_NotEqualTo_With_Multiple_Values_BuildExpression_Uses_First_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.NotEqualTo, "test", "other");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first value "test"
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "other" }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public double Value { get; init; }
      public bool IsActive { get; init; }
      public DateTime CreatedDate { get; init; }
      public Guid UniqueId { get; init; }
      public TestStatus Status { get; init; }
      public int? NullableInt { get; init; }
      public NestedEntity? Nested { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
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
