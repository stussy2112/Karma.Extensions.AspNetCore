// -----------------------------------------------------------------------
// <copyright file="ComparisonOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="ComparisonOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class ComparisonOperatorHandlerTests
  {
    private ComparisonOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new ComparisonOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_GreaterThan_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.GreaterThan;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_LessThan_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.LessThan;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_GreaterThanOrEqualTo_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.GreaterThanOrEqualTo;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_LessThanOrEqualTo_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.LessThanOrEqualTo;

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

    // ========== BuildExpression Tests - GreaterThan Operator ==========

    [TestMethod]
    public void When_GreaterThan_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 100 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 9 }));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.6 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 100.0 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.0 }));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.GreaterThan, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, "10");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 9 }));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCount", "Nested.Count", Operator.GreaterThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 11 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Count = 10 } }));

      // When Nested is null, the null-safe navigation returns default(int) which is 0
      // 0 > 10 evaluates to false
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_GreaterThan_Operator_With_Negative_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, -10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -20 }));
    }

    // ========== BuildExpression Tests - LessThan Operator ==========

    [TestMethod]
    public void When_LessThan_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 9 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 11 }));
    }

    [TestMethod]
    public void When_LessThan_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.LessThan, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.4 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 0.0 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 11.0 }));
    }

    [TestMethod]
    public void When_LessThan_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.LessThan, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_LessThan_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThan, "10");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 9 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 11 }));
    }

    [TestMethod]
    public void When_LessThan_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCount", "Nested.Count", Operator.LessThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 9 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Count = 10 } }));

      // When Nested is null, the null-safe navigation returns default(int) which is 0
      // 0 < 10 evaluates to true
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_LessThan_Operator_With_Negative_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThan, -10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = -20 }));
      Assert.IsTrue(compiled(new TestEntity { Id = -11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
    }

    // ========== BuildExpression Tests - GreaterThanOrEqualTo Operator ==========

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThanOrEqualTo, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 100 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 9 }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.GreaterThanOrEqualTo, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 10.6 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 100.0 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.4 }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.GreaterThanOrEqualTo, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThanOrEqualTo, "10");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 9 }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCount", "Nested.Count", Operator.GreaterThanOrEqualTo, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 10 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 11 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Count = 9 } }));

      // When Nested is null, the null-safe navigation returns default(int) which is 0
      // 0 >= 10 evaluates to false
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_GreaterThanOrEqualTo_Operator_With_Zero_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThanOrEqualTo, 0);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 1 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -1 }));
    }

    // ========== BuildExpression Tests - LessThanOrEqualTo Operator ==========

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_Int_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThanOrEqualTo, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 9 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 11 }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_Double_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.LessThanOrEqualTo, 10.5);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.5 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 10.4 }));
      Assert.IsTrue(compiled(new TestEntity { Value = 0.0 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.6 }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_DateTime_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.LessThanOrEqualTo, testDate);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = testDate }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_String_To_Int_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThanOrEqualTo, "10");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 9 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 11 }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCount", "Nested.Count", Operator.LessThanOrEqualTo, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 10 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Count = 9 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Count = 11 } }));
      // When Nested is null, the null-safe navigation returns default(int) which is 0
      // 0 < 10 evaluates to true
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_LessThanOrEqualTo_Operator_With_Zero_Value_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThanOrEqualTo, 0);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = 0 }));
      Assert.IsTrue(compiled(new TestEntity { Id = -1 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 1 }));
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
      FilterInfo filter = new("Id", null!, Operator.GreaterThan, 10);

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
      FilterInfo filter = new("Id", "", Operator.GreaterThan, 10);

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
      FilterInfo filter = new("Id", "   ", Operator.GreaterThan, 10);

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.GreaterThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should return true when property doesn't exist
      Assert.IsTrue(compiled(new TestEntity { Id = 5 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 15 }));
    }

    [TestMethod]
    public void When_Property_Path_Is_Invalid_BuildExpression_Returns_True_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.GreaterThan, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should return true when property path is invalid
      Assert.IsTrue(compiled(new TestEntity()));
    }

    // ========== BuildExpression Tests - Unsupported Operator ==========

    [TestMethod]
    public void When_Operator_Is_EqualTo_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, 10);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("ComparisonOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("EqualTo", StringComparison.Ordinal));
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

      Assert.IsTrue(ex.Message.Contains("ComparisonOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("StartsWith", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_None_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.None, 10);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("ComparisonOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Special Cases ==========

    [TestMethod]
    public void When_GreaterThan_With_Max_Int_Boundary_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, int.MaxValue - 1);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = int.MaxValue }));
      Assert.IsFalse(compiled(new TestEntity { Id = int.MaxValue - 1 }));
    }

    [TestMethod]
    public void When_LessThan_With_Min_Int_Boundary_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.LessThan, int.MinValue + 1);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = int.MinValue }));
      Assert.IsFalse(compiled(new TestEntity { Id = int.MinValue + 1 }));
    }

    [TestMethod]
    public void When_GreaterThan_With_Decimal_Precision_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Value", nameof(TestEntity.Value), Operator.GreaterThan, 10.123456);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Value = 10.123457 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.123456 }));
      Assert.IsFalse(compiled(new TestEntity { Value = 10.123455 }));
    }

    [TestMethod]
    public void When_Multiple_Values_Provided_BuildExpression_Uses_First_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, 10, 20, 30);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first value (10)
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 9 }));
    }

    [TestMethod]
    public void When_DateTime_String_Conversion_BuildExpression_Converts_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.GreaterThan, "2023-12-25");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = testDate }));
    }

    [TestMethod]
    public void When_All_Operators_On_Same_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filterGt = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, 10);
      FilterInfo filterLt = new("Id", nameof(TestEntity.Id), Operator.LessThan, 10);
      FilterInfo filterGte = new("Id", nameof(TestEntity.Id), Operator.GreaterThanOrEqualTo, 10);
      FilterInfo filterLte = new("Id", nameof(TestEntity.Id), Operator.LessThanOrEqualTo, 10);

      // Act
      Expression exprGt = _handler.BuildExpression(parameter, filterGt);
      Expression exprLt = _handler.BuildExpression(parameter, filterLt);
      Expression exprGte = _handler.BuildExpression(parameter, filterGte);
      Expression exprLte = _handler.BuildExpression(parameter, filterLte);

      Func<TestEntity, bool> compiledGt = Expression.Lambda<Func<TestEntity, bool>>(exprGt, parameter).Compile();
      Func<TestEntity, bool> compiledLt = Expression.Lambda<Func<TestEntity, bool>>(exprLt, parameter).Compile();
      Func<TestEntity, bool> compiledGte = Expression.Lambda<Func<TestEntity, bool>>(exprGte, parameter).Compile();
      Func<TestEntity, bool> compiledLte = Expression.Lambda<Func<TestEntity, bool>>(exprLte, parameter).Compile();

      // Assert - Test value 9
      Assert.IsFalse(compiledGt(new TestEntity { Id = 9 }));
      Assert.IsTrue(compiledLt(new TestEntity { Id = 9 }));
      Assert.IsFalse(compiledGte(new TestEntity { Id = 9 }));
      Assert.IsTrue(compiledLte(new TestEntity { Id = 9 }));

      // Assert - Test value 10
      Assert.IsFalse(compiledGt(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiledLt(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiledGte(new TestEntity { Id = 10 }));
      Assert.IsTrue(compiledLte(new TestEntity { Id = 10 }));

      // Assert - Test value 11
      Assert.IsTrue(compiledGt(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiledLt(new TestEntity { Id = 11 }));
      Assert.IsTrue(compiledGte(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiledLte(new TestEntity { Id = 11 }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public double Value { get; init; }
      public DateTime CreatedDate { get; init; }
      public NestedEntity? Nested { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public int Count { get; init; }
    }
  }
}
