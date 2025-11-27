// -----------------------------------------------------------------------
// <copyright file="NullOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="NullOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class NullOperatorHandlerTests
  {
    private NullOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new NullOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_IsNull_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.IsNull;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_IsNotNull_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.IsNotNull;

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

    // ========== BuildExpression Tests - IsNull Operator ==========

    [TestMethod]
    public void When_IsNull_Operator_With_Null_String_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = null }));
      Assert.IsFalse(compiled(new TestEntity { Name = "test" }));
      Assert.IsFalse(compiled(new TestEntity { Name = string.Empty }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Null_Nullable_Int_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableInt = null }));
      Assert.IsFalse(compiled(new TestEntity { NullableInt = 0 }));
      Assert.IsFalse(compiled(new TestEntity { NullableInt = 42 }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Null_Nullable_DateTime_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableDate", nameof(TestEntity.NullableDate), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableDate = null }));
      Assert.IsFalse(compiled(new TestEntity { NullableDate = DateTime.Now }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Null_Reference_Type_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Nested", nameof(TestEntity.Nested), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity() }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Nested_Property_BuildExpression_Returns_True_For_Null_Parent()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = null } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "test" } }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Nullable_Enum_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = null }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Active }));
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Inactive }));
    }

    // ========== BuildExpression Tests - IsNotNull Operator ==========

    [TestMethod]
    public void When_IsNotNull_Operator_With_Non_Null_String_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = string.Empty }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Non_Null_Nullable_Int_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableInt = 0 }));
      Assert.IsTrue(compiled(new TestEntity { NullableInt = 42 }));
      Assert.IsFalse(compiled(new TestEntity { NullableInt = null }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Non_Null_Nullable_DateTime_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableDate", nameof(TestEntity.NullableDate), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { NullableDate = DateTime.Now }));
      Assert.IsTrue(compiled(new TestEntity { NullableDate = DateTime.MinValue }));
      Assert.IsFalse(compiled(new TestEntity { NullableDate = null }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Non_Null_Reference_Type_Property_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Nested", nameof(TestEntity.Nested), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity() }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Nested_Property_BuildExpression_Returns_False_For_Null_Parent()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = null } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "test" } }));
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
      FilterInfo filter = new("Name", null!, Operator.IsNull);

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
      FilterInfo filter = new("Name", "", Operator.IsNull);

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
      FilterInfo filter = new("Name", "   ", Operator.IsNull);

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.IsNull);

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
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.IsNull);

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

      Assert.IsTrue(ex.Message.Contains("NullOperatorHandler", StringComparison.Ordinal));
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

      Assert.IsTrue(ex.Message.Contains("NullOperatorHandler", StringComparison.Ordinal));
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

      Assert.IsTrue(ex.Message.Contains("NullOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Edge Cases ==========

    [TestMethod]
    public void When_IsNull_Operator_With_Empty_String_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Empty string is not null
      Assert.IsFalse(compiled(new TestEntity { Name = string.Empty }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Empty_String_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Empty string is not null
      Assert.IsTrue(compiled(new TestEntity { Name = string.Empty }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Zero_Value_Nullable_Int_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Zero is not null
      Assert.IsFalse(compiled(new TestEntity { NullableInt = 0 }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Zero_Value_Nullable_Int_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableInt", nameof(TestEntity.NullableInt), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Zero is not null
      Assert.IsTrue(compiled(new TestEntity { NullableInt = 0 }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_MinValue_DateTime_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableDate", nameof(TestEntity.NullableDate), Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - DateTime.MinValue is not null
      Assert.IsFalse(compiled(new TestEntity { NullableDate = DateTime.MinValue }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_MinValue_DateTime_BuildExpression_Returns_True()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NullableDate", nameof(TestEntity.NullableDate), Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - DateTime.MinValue is not null
      Assert.IsTrue(compiled(new TestEntity { NullableDate = DateTime.MinValue }));
    }

    [TestMethod]
    public void When_IsNull_Operator_With_Deeply_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("DeepNested", "Nested.Child.Value", Operator.IsNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = null }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Child = null } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Child = new ChildEntity { Value = null } } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Child = new ChildEntity { Value = "test" } } }));
    }

    [TestMethod]
    public void When_IsNotNull_Operator_With_Deeply_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("DeepNested", "Nested.Child.Value", Operator.IsNotNull);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Child = null } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Child = new ChildEntity { Value = null } } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Child = new ChildEntity { Value = "test" } } }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public int? NullableInt { get; init; }
      public DateTime? NullableDate { get; init; }
      public NestedEntity? Nested { get; init; }
      public TestStatus? Status { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record NestedEntity
    {
      public string? Code { get; init; }
      public ChildEntity? Child { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record ChildEntity
    {
      public string? Value { get; init; }
    }

    public enum TestStatus
    {
      None = 0,
      Active = 1,
      Inactive = 2
    }
  }
}
