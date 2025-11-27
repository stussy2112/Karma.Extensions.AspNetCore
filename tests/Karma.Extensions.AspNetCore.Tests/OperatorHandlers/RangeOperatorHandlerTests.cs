// -----------------------------------------------------------------------
// <copyright file="RangeOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="RangeOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class RangeOperatorHandlerTests
  {
    private RangeOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new RangeOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_Between_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.Between;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_NotBetween_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.NotBetween;

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

    // ========== BuildExpression Tests - Between Operator with Primitive Types ==========

    [TestMethod]
    public void When_Between_Operator_With_Int_Property_BuildExpression_Returns_True_For_Value_In_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 10, 20);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Exclusive bounds: value > 10 AND value < 20
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 15 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 19 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 })); // Lower bound is exclusive
      Assert.IsFalse(compiled(new TestEntity { Id = 20 })); // Upper bound is exclusive
      Assert.IsFalse(compiled(new TestEntity { Id = 5 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 25 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Decimal_Property_BuildExpression_Returns_True_For_Value_In_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Price", nameof(TestEntity.Price), Operator.Between, 10.0m, 20.0m);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Exclusive bounds
      Assert.IsTrue(compiled(new TestEntity { Price = 10.01m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 15.00m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 19.99m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 10.0m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 20.0m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 5.0m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 25.0m }));
    }

    [TestMethod]
    public void When_Between_Operator_With_DateTime_Property_BuildExpression_Returns_True_For_Value_In_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      DateTime start = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
      DateTime end = new(2023, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.Between, start, end);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Exclusive bounds
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 30, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = start }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = end }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2022, 12, 31, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Double_Property_BuildExpression_Returns_True_For_Value_In_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Rating", nameof(TestEntity.Rating), Operator.Between, 1.0, 5.0);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Exclusive bounds
      Assert.IsTrue(compiled(new TestEntity { Rating = 1.1 }));
      Assert.IsTrue(compiled(new TestEntity { Rating = 3.0 }));
      Assert.IsTrue(compiled(new TestEntity { Rating = 4.9 }));
      Assert.IsFalse(compiled(new TestEntity { Rating = 1.0 }));
      Assert.IsFalse(compiled(new TestEntity { Rating = 5.0 }));
      Assert.IsFalse(compiled(new TestEntity { Rating = 0.5 }));
      Assert.IsFalse(compiled(new TestEntity { Rating = 5.5 }));
    }

    // ========== BuildExpression Tests - NotBetween Operator ==========

    [TestMethod]
    public void When_NotBetween_Operator_With_Int_Property_BuildExpression_Returns_True_For_Value_Outside_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, 10, 20);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - NotBetween: value < 10 OR value > 20
      Assert.IsFalse(compiled(new TestEntity { Id = 11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 15 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 19 }));

      // Boundary values (start and end) return false because they don't satisfy value < start OR value > end
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 20 }));

      // Values outside the range return true
      Assert.IsTrue(compiled(new TestEntity { Id = 5 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 25 }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Decimal_Property_BuildExpression_Returns_True_For_Value_Outside_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Price", nameof(TestEntity.Price), Operator.NotBetween, 10.0m, 20.0m);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Price = 10.01m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 15.00m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 19.99m }));

      // Boundary values (start and end) return false because they don't satisfy value < start OR value > end
      Assert.IsFalse(compiled(new TestEntity { Price = 10.0m }));
      Assert.IsFalse(compiled(new TestEntity { Price = 20.0m }));

      // Values outside the range return true
      Assert.IsTrue(compiled(new TestEntity { Price = 5.0m }));
      Assert.IsTrue(compiled(new TestEntity { Price = 25.0m }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_DateTime_Property_BuildExpression_Returns_True_For_Value_Outside_Range()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      DateTime start = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
      DateTime end = new(2023, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.NotBetween, start, end);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - NotBetween uses exclusive bounds: value < start OR value > end
      // Values inside the range return false
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 30, 0, 0, 0, DateTimeKind.Unspecified) }));
      
      // Boundary values (start and end) return false because they don't satisfy value < start OR value > end
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = start }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = end }));
      
      // Values outside the range return true
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2022, 12, 31, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    // ========== BuildExpression Tests - Nested Properties ==========

    [TestMethod]
    public void When_Between_Operator_With_Nested_Int_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedId", "Nested.Id", Operator.Between, 100, 200);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 101 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 150 } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Id = 199 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Id = 100 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Id = 200 } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Id = 50 } }));
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
      FilterInfo filter = new("Id", null!, Operator.Between, 10, 20);

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
      FilterInfo filter = new("Id", "", Operator.Between, 10, 20);

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
      FilterInfo filter = new("Id", "   ", Operator.Between, 10, 20);

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.Between, 10, 20);

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
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.Between, 10, 20);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return true constant when property path is invalid
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsTrue((bool?)constantExpr.Value);
    }

    // ========== BuildExpression Tests - Insufficient Values ==========

    [TestMethod]
    public void When_Between_Operator_With_No_Values_BuildExpression_Returns_False_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return false constant when values are insufficient
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Between_Operator_With_One_Value_BuildExpression_Returns_False_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return false constant when values are insufficient
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_No_Values_BuildExpression_Returns_False_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return false constant when values are insufficient
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_One_Value_BuildExpression_Returns_False_Constant()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - Should return false constant when values are insufficient
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    // ========== BuildExpression Tests - Null Values ==========

    [TestMethod]
    public void When_Between_Operator_With_Null_Lower_Bound_BuildExpression_Throws_InvalidOperationException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, null!, 20);

      // Act & Assert
      InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("Both values must be non-null", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("Between", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Between_Operator_With_Null_Upper_Bound_BuildExpression_Throws_InvalidOperationException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 10, null!);

      // Act & Assert
      InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("Both values must be non-null", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("Between", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Between_Operator_With_Both_Null_Values_BuildExpression_Returns_False_Due_To_Deduplication()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, null!, null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - FilterInfo deduplicates values using HashSet, so (null, null) becomes just (null)
      // This results in insufficient values (< 2), returning a false constant before the null check
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Null_Lower_Bound_BuildExpression_Throws_InvalidOperationException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, null!, 20);

      // Act & Assert
      InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("Both values must be non-null", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("NotBetween", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Null_Upper_Bound_BuildExpression_Throws_InvalidOperationException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, 10, null!);

      // Act & Assert
      InvalidOperationException ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("Both values must be non-null", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("NotBetween", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Unsupported Operator ==========

    [TestMethod]
    public void When_Operator_Is_EqualTo_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EqualTo, 10, 20);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("RangeOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("EqualTo", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_GreaterThan_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.GreaterThan, 10, 20);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("RangeOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("GreaterThan", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_None_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.None, 10, 20);

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("RangeOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Edge Cases ==========

    [TestMethod]
    public void When_Between_Operator_With_Same_Lower_And_Upper_Bounds_BuildExpression_Returns_False_Due_To_Deduplication()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 10, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - FilterInfo deduplicates values using HashSet, so (10, 10) becomes just (10)
      // This results in insufficient values (< 2), returning a false constant
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Same_Lower_And_Upper_Bounds_BuildExpression_Returns_False_Due_To_Deduplication()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, 10, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);

      // Assert - FilterInfo deduplicates values using HashSet, so (10, 10) becomes just (10)
      // This results in insufficient values (< 2), returning a false constant
      _ = Assert.IsInstanceOfType<ConstantExpression>(expression);
      var constantExpr = (ConstantExpression)expression;
      Assert.IsFalse((bool?)constantExpr.Value);
    }

    [TestMethod]
    public void When_Between_Operator_With_Reversed_Bounds_BuildExpression_Returns_False_For_All()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 20, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Reversed bounds: value > 20 AND value < 10 is always false
      Assert.IsFalse(compiled(new TestEntity { Id = 5 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 15 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 25 }));
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Reversed_Bounds_BuildExpression_Returns_True_For_All()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.NotBetween, 20, 10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Reversed bounds: value < 20 OR value > 10 covers all values
      Assert.IsTrue(compiled(new TestEntity { Id = 5 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 15 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 25 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Type_Conversion_BuildExpression_Converts_Values()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, "10", "20");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - String values should be converted to int
      Assert.IsTrue(compiled(new TestEntity { Id = 11 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 15 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 19 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 20 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Three_Or_More_Values_Uses_First_Two()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, 10, 20, 30, 40);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first two values (10 and 20), ignore 30 and 40
      Assert.IsTrue(compiled(new TestEntity { Id = 15 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 20 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 25 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 35 }));
    }

    [TestMethod]
    public void When_Between_Operator_With_Negative_Bounds_BuildExpression_Works_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.Between, -20, -10);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Id = -19 }));
      Assert.IsTrue(compiled(new TestEntity { Id = -15 }));
      Assert.IsTrue(compiled(new TestEntity { Id = -11 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -20 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -10 }));
      Assert.IsFalse(compiled(new TestEntity { Id = -25 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 0 }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public decimal Price { get; init; }
      public DateTime CreatedDate { get; init; }
      public double Rating { get; init; }
      public NestedEntity? Nested { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public int Id { get; init; }
      public decimal Value { get; init; }
    }
  }
}
