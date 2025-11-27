// -----------------------------------------------------------------------
// <copyright file="StringOperatorHandlerTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.OperatorHandlers
{
  /// <summary>
  /// Unit tests for the <see cref="StringOperatorHandler"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class StringOperatorHandlerTests
  {
    private StringOperatorHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize() => _handler = new StringOperatorHandler();

    // ========== CanHandle Tests ==========

    [TestMethod]
    public void When_Operator_Is_StartsWith_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.StartsWith;

      // Act
      bool result = _handler.CanHandle(op);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Operator_Is_EndsWith_CanHandle_Returns_True()
    {
      // Arrange
      Operator op = Operator.EndsWith;

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
    public void When_Operator_Is_Contains_CanHandle_Returns_False()
    {
      // Arrange
      Operator op = Operator.Contains;

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

    // ========== BuildExpression Tests - StartsWith Operator ==========

    [TestMethod]
    public void When_StartsWith_Operator_With_Valid_String_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test123" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "testing" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "TEST" })); // Case-insensitive
      Assert.IsFalse(compiled(new TestEntity { Name = "pretest" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Empty_String_BuildExpression_Matches_All_NonNull()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - All non-null strings start with empty string
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Null_Value_BuildExpression_Uses_Empty_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, (string)null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null value treated as empty string
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Case_Insensitive_Match_BuildExpression_Matches()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "TEST");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test123" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "TEST123" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "Test123" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "other" }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.StartsWith, "ABC");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC-123" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "abc-123" } })); // Case-insensitive
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ-123" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Non_String_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.StartsWith, "12");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should convert Id to string and check StartsWith
      Assert.IsTrue(compiled(new TestEntity { Id = 123 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 12 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 456 }));
    }

    [TestMethod]
    public void When_StartsWith_Operator_With_Multiple_Values_BuildExpression_Uses_First_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "test", "other");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first value "test"
      Assert.IsTrue(compiled(new TestEntity { Name = "test123" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "other123" })); // Second value ignored
    }

    // ========== BuildExpression Tests - EndsWith Operator ==========

    [TestMethod]
    public void When_EndsWith_Operator_With_Valid_String_Property_BuildExpression_Returns_Correct_Expression()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "pretest" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "sometest" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "TEST" })); // Case-insensitive
      Assert.IsFalse(compiled(new TestEntity { Name = "test123" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Empty_String_BuildExpression_Matches_All_NonNull()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - All non-null strings end with empty string
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Null_Value_BuildExpression_Uses_Empty_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, (string)null!);

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null value treated as empty string
      Assert.IsTrue(compiled(new TestEntity { Name = "test" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "" }));
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Case_Insensitive_Match_BuildExpression_Matches()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "TEST");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "sometest" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "someTEST" }));
      Assert.IsTrue(compiled(new TestEntity { Name = "someTest" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "testother" }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Nested_Property_BuildExpression_Evaluates_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.EndsWith, "123");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC-123" } }));
      Assert.IsTrue(compiled(new TestEntity { Nested = new NestedEntity { Code = "XYZ-123" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = new NestedEntity { Code = "ABC-456" } }));
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Non_String_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Id", nameof(TestEntity.Id), Operator.EndsWith, "23");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should convert Id to string and check EndsWith
      Assert.IsTrue(compiled(new TestEntity { Id = 123 }));
      Assert.IsTrue(compiled(new TestEntity { Id = 23 }));
      Assert.IsFalse(compiled(new TestEntity { Id = 456 }));
    }

    [TestMethod]
    public void When_EndsWith_Operator_With_Multiple_Values_BuildExpression_Uses_First_Value()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "test", "other");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Should use first value "test"
      Assert.IsTrue(compiled(new TestEntity { Name = "sometest" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "someother" })); // Second value ignored
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
      FilterInfo filter = new("Name", null!, Operator.StartsWith, "test");

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
      FilterInfo filter = new("Name", "", Operator.StartsWith, "test");

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
      FilterInfo filter = new("Name", "   ", Operator.StartsWith, "test");

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
      FilterInfo filter = new("Invalid", "NonExistentProperty", Operator.StartsWith, "test");

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
      FilterInfo filter = new("Invalid", "NonExistent.Property", Operator.StartsWith, "test");

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
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EqualTo, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("StringOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("EqualTo", StringComparison.Ordinal));
    }

    [TestMethod]
    public void When_Operator_Is_Contains_BuildExpression_Throws_NotSupportedException()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.Contains, "test");

      // Act & Assert
      NotSupportedException ex = Assert.ThrowsExactly<NotSupportedException>(() =>
      {
        _ = _handler.BuildExpression(parameter, filter);
      });

      Assert.IsTrue(ex.Message.Contains("StringOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("Contains", StringComparison.Ordinal));
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

      Assert.IsTrue(ex.Message.Contains("StringOperatorHandler", StringComparison.Ordinal));
      Assert.IsTrue(ex.Message.Contains("None", StringComparison.Ordinal));
    }

    // ========== BuildExpression Tests - Special String Cases ==========

    [TestMethod]
    public void When_StartsWith_With_Special_Characters_BuildExpression_Matches_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "test.@#");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "test.@#123" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "test123" }));
    }

    [TestMethod]
    public void When_EndsWith_With_Special_Characters_BuildExpression_Matches_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, ".@#test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "123.@#test" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "123test" }));
    }

    [TestMethod]
    public void When_StartsWith_With_Unicode_Characters_BuildExpression_Matches_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "αβγ");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "αβγδεζ" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "δεζαβγ" }));
    }

    [TestMethod]
    public void When_EndsWith_With_Unicode_Characters_BuildExpression_Matches_Correctly()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "δεζ");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Name = "αβγδεζ" }));
      Assert.IsFalse(compiled(new TestEntity { Name = "δεζαβγ" }));
    }

    // ========== BuildExpression Tests - Type Conversion ==========

    [TestMethod]
    public void When_StartsWith_With_DateTime_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.StartsWith, "12");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Unspecified) }));
      Assert.IsFalse(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 25, 0, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_EndsWith_With_DateTime_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EndsWith, "AM");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      // This test depends on culture settings for DateTime.ToString()
      // In most US cultures, times before noon end with "AM"
      Assert.IsTrue(compiled(new TestEntity { CreatedDate = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Unspecified) }));
    }

    [TestMethod]
    public void When_StartsWith_With_Guid_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      FilterInfo filter = new("UniqueId", nameof(TestEntity.UniqueId), Operator.StartsWith, "1234");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(compiled(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_EndsWith_With_Guid_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      var testGuid = new Guid("12345678-1234-5678-9abc-123456789abc");
      FilterInfo filter = new("UniqueId", nameof(TestEntity.UniqueId), Operator.EndsWith, "9abc");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { UniqueId = testGuid }));
      Assert.IsFalse(compiled(new TestEntity { UniqueId = Guid.NewGuid() }));
    }

    [TestMethod]
    public void When_StartsWith_With_Enum_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.StartsWith, "Act");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active })); // "Active".StartsWith("Act")
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Pending }));
    }

    [TestMethod]
    public void When_EndsWith_With_Enum_Property_BuildExpression_Converts_To_String()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Status", nameof(TestEntity.Status), Operator.EndsWith, "tive");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Active })); // "Active".EndsWith("tive")
      Assert.IsTrue(compiled(new TestEntity { Status = TestStatus.Inactive })); // "Inactive".EndsWith("tive")
      Assert.IsFalse(compiled(new TestEntity { Status = TestStatus.Pending }));
    }

    // ========== BuildExpression Tests - Null Handling ==========

    [TestMethod]
    public void When_StartsWith_With_Null_Property_Value_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.StartsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null property should return false
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_EndsWith_With_Null_Property_Value_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("Name", nameof(TestEntity.Name), Operator.EndsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null property should return false
      Assert.IsFalse(compiled(new TestEntity { Name = null }));
    }

    [TestMethod]
    public void When_StartsWith_With_Null_Nested_Property_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.StartsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null nested entity should return false
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    [TestMethod]
    public void When_EndsWith_With_Null_Nested_Property_BuildExpression_Returns_False()
    {
      // Arrange
      ParameterExpression parameter = Expression.Parameter(typeof(TestEntity), "x");
      FilterInfo filter = new("NestedCode", "Nested.Code", Operator.EndsWith, "test");

      // Act
      Expression expression = _handler.BuildExpression(parameter, filter);
      Func<TestEntity, bool> compiled = Expression.Lambda<Func<TestEntity, bool>>(expression, parameter).Compile();

      // Assert - Null nested entity should return false
      Assert.IsFalse(compiled(new TestEntity { Nested = null }));
    }

    // ========== Test Entities ==========

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id { get; init; }
      public string? Name { get; init; }
      public DateTime CreatedDate { get; init; }
      public Guid UniqueId { get; init; }
      public TestStatus Status { get; init; }
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
