// -----------------------------------------------------------------------
// <copyright file="FilterExpressionBuilderErrorScenarioTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Error scenario and edge case unit tests for FilterExpressionBuilder focusing on type conversion failures and exception handling.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class FilterExpressionBuilderErrorScenarioTests
  {
    [ExcludeFromCodeCoverage]
    public sealed class CustomObject
    {
      public override string ToString() => "CustomToString";
    }

    [TestMethod]
    public void When_Invalid_String_To_Int_Conversion_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.EqualTo, "not-a-number")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Invalid_String_To_Double_Conversion_BuildLambda_Throws_Exactly_FormatException()
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
    public void When_Invalid_String_To_Boolean_Conversion_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("IsActive", nameof(TestEntity.IsActive), Operator.EqualTo, "not-a-boolean")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Invalid_String_To_DateTime_Conversion_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("CreatedDate", nameof(TestEntity.CreatedDate), Operator.EqualTo, "invalid-date")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Invalid_String_To_Guid_Conversion_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("UniqueId", nameof(TestEntity.UniqueId), Operator.EqualTo, "not-a-guid")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_In_Operator_With_Mixed_Valid_And_Invalid_Values_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Id", nameof(TestEntity.Id), Operator.In, 1, "not-a-number", 3)];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Between_Operator_With_One_Invalid_Value_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.Between, 10.0, "not-a-number")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_NotBetween_Operator_With_Invalid_Type_Conversion_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("Value", nameof(TestEntity.Value), Operator.NotBetween, "invalid1", "invalid2")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      });
    }

    [TestMethod]
    public void When_Decimal_Conversion_With_Invalid_String_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("DecimalValue", nameof(EntityWithValueTypes.DecimalValue), Operator.EqualTo, "12.34.56")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<EntityWithValueTypes>(filters);
      });
    }

    [TestMethod]
    public void When_Float_Conversion_With_Invalid_String_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("FloatValue", nameof(EntityWithValueTypes.FloatValue), Operator.EqualTo, "invalid-float")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<EntityWithValueTypes>(filters);
      });
    }

    [TestMethod]
    public void When_Byte_Conversion_With_Invalid_String_BuildLambda_Throws_Exactly_OverflowException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("ByteValue", nameof(EntityWithValueTypes.ByteValue), Operator.EqualTo, "300")]; // byte max is 255

      // Act & Assert
      _ = Assert.ThrowsExactly<OverflowException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<EntityWithValueTypes>(filters);
      });
    }

    [TestMethod]
    public void When_Char_Conversion_With_Invalid_String_BuildLambda_Throws_Exactly_FormatException()
    {
      // Arrange
      List<IFilterInfo> filters = [new FilterInfo("CharValue", nameof(EntityWithValueTypes.CharValue), Operator.EqualTo, "multiple-chars")];

      // Act & Assert
      _ = Assert.ThrowsExactly<FormatException>(() =>
      {
        _ = FilterExpressionBuilder.BuildLambda<EntityWithValueTypes>(filters);
      });
    }

    [TestMethod]
    public void When_Contains_Operator_With_Custom_Object_BuildLambda_Uses_ToString_Method()
    {
      // Arrange - Use Contains operator with a custom object that has overridden ToString()
      List<IFilterInfo> filters = [new FilterInfo("Name", nameof(TestEntity.Name), Operator.Contains, new CustomObject())]; 

      // Act - Should use the custom ToString() method
      Func<TestEntity, bool> lambda = FilterExpressionBuilder.BuildLambda<TestEntity>(filters);
      
      // Assert - Should use the custom ToString() result "CustomToString"
      Assert.IsFalse(lambda(new TestEntity { Name = "test" })); // "test".Contains("CustomToString") == false
      Assert.IsTrue(lambda(new TestEntity { Name = "CustomToString" })); // "CustomToString".Contains("CustomToString") == true
      Assert.IsTrue(lambda(new TestEntity { Name = "MyCustomToStringValue" })); // "MyCustomToStringValue".Contains("CustomToString") == true
      Assert.IsFalse(lambda(new TestEntity { Name = null })); // null returns false
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
      public bool IsActive
      {
        get; init;
      }
      public DateTime CreatedDate
      {
        get; init;
      }
      public Guid UniqueId
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record EntityWithValueTypes
    {
      public bool BooleanValue
      {
        get; init;
      }
      public DateTime DateTimeValue
      {
        get; init;
      }
      public Guid GuidValue
      {
        get; init;
      }
      public decimal DecimalValue
      {
        get; init;
      }
      public float FloatValue
      {
        get; init;
      }
      public byte ByteValue
      {
        get; init;
      }
      public char CharValue
      {
        get; init;
      }
    }
  }
}