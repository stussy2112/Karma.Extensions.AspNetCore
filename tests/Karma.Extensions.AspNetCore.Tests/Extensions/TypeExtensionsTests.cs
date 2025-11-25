// -----------------------------------------------------------------------
// <copyright file="TypeExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests.Extensions
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public partial class TypeExtensionsTests
  {
    [TestMethod]
    public void When_typeInfo_is_null_GetEnumerableElementType_returns_null()
    {
      // Arrange
      Type? typeInfo = null!;

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_typeInfo_is_array_GetEnumerableElementType_returns_element_type()
    {
      // Arrange
      Type typeInfo = typeof(int[]);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void When_typeInfo_is_string_array_GetEnumerableElementType_returns_string()
    {
      // Arrange
      Type typeInfo = typeof(string[]);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(string), result);
    }

    [TestMethod]
    public void When_typeInfo_is_multidimensional_array_GetEnumerableElementType_returns_element_type()
    {
      // Arrange
      Type typeInfo = typeof(int[,]);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void When_typeInfo_is_string_GetEnumerableElementType_returns_null()
    {
      // Arrange
      Type typeInfo = typeof(string);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_typeInfo_is_List_of_int_GetEnumerableElementType_returns_int()
    {
      // Arrange
      Type typeInfo = typeof(List<int>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void When_typeInfo_is_IEnumerable_of_string_GetEnumerableElementType_returns_string()
    {
      // Arrange
      Type typeInfo = typeof(IEnumerable<string>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(string), result);
    }

    [TestMethod]
    public void When_typeInfo_is_ICollection_of_double_GetEnumerableElementType_returns_double()
    {
      // Arrange
      Type typeInfo = typeof(ICollection<double>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(double), result);
    }

    [TestMethod]
    public void When_typeInfo_is_IList_of_decimal_GetEnumerableElementType_returns_decimal()
    {
      // Arrange
      Type typeInfo = typeof(IList<decimal>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(decimal), result);
    }

    [TestMethod]
    public void When_typeInfo_is_non_generic_IEnumerable_GetEnumerableElementType_returns_null()
    {
      // Arrange
      Type typeInfo = typeof(IEnumerable);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_typeInfo_is_custom_class_implementing_IEnumerable_GetEnumerableElementType_returns_element_type()
    {
      // Arrange
      Type typeInfo = typeof(CustomEnumerableClass);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void When_typeInfo_is_primitive_type_GetEnumerableElementType_returns_null()
    {
      // Arrange
      Type typeInfo = typeof(int);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_typeInfo_is_IQueryable_of_string_GetEnumerableElementType_returns_string()
    {
      // Arrange
      Type typeInfo = typeof(IQueryable<string>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(string), result);
    }

    [TestMethod]
    public void When_typeInfo_is_HashSet_of_int_GetEnumerableElementType_returns_int()
    {
      // Arrange
      Type typeInfo = typeof(HashSet<int>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int), result);
    }

    [TestMethod]
    public void When_typeInfo_is_Dictionary_GetEnumerableElementType_returns_KeyValuePair()
    {
      // Arrange
      Type typeInfo = typeof(Dictionary<string, int>);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(KeyValuePair<string, int>), result);
    }

    [TestMethod]
    public void When_instance_is_null_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type? instance = null;
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_genericType_is_null_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type? genericType = null;

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_both_parameters_are_null_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type? instance = null;
      Type? genericType = null;

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_instance_is_List_of_int_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_generic_type_definition_matching_genericType_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type genericType = typeof(List<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_implements_interface_matching_genericType_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(CustomEnumerableClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_does_not_match_genericType_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type instance = typeof(int);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_instance_is_string_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(string);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_has_base_type_assignable_to_genericType_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(DerivedEnumerableClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_ICollection_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(ICollection<string>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_array_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(int[]);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_Dictionary_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(Dictionary<string, int>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_typeInfo_is_null_IsEnumerable_returns_false()
    {
      // Arrange
      Type? typeInfo = null;

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_List_of_int_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(List<int>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(int), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_array_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(string[]);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(string), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_string_IsEnumerable_returns_false()
    {
      // Arrange
      Type typeInfo = typeof(string);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_primitive_IsEnumerable_returns_false()
    {
      // Arrange
      Type typeInfo = typeof(int);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_IEnumerable_of_string_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(IEnumerable<string>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(string), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_custom_enumerable_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(CustomEnumerableClass);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(int), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_non_generic_IEnumerable_IsEnumerable_returns_false()
    {
      // Arrange
      Type typeInfo = typeof(IEnumerable);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_ICollection_of_double_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(ICollection<double>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(double), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_IQueryable_of_string_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(IQueryable<string>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(string), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_Dictionary_IsEnumerable_returns_true_and_sets_containedType_to_KeyValuePair()
    {
      // Arrange
      Type typeInfo = typeof(Dictionary<string, int>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(KeyValuePair<string, int>), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_HashSet_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(HashSet<decimal>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(decimal), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_IReadOnlyCollection_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(IReadOnlyCollection<bool>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(bool), containedType);
    }

    [TestMethod]
    public void When_typeInfo_is_jagged_array_GetEnumerableElementType_returns_array_element_type()
    {
      // Arrange
      Type typeInfo = typeof(int[][]);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(int[]), result);
    }

    [TestMethod]
    public void When_instance_is_non_generic_type_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type instance = typeof(NonGenericClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_instance_is_struct_implementing_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(CustomEnumerableStruct);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_typeInfo_is_custom_struct_GetEnumerableElementType_returns_element_type()
    {
      // Arrange
      Type typeInfo = typeof(CustomEnumerableStruct);

      // Act
      Type? result = typeInfo.GetEnumerableElementType();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(typeof(char), result);
    }

    [TestMethod]
    public void When_typeInfo_is_IList_IsEnumerable_returns_true_and_sets_containedType()
    {
      // Arrange
      Type typeInfo = typeof(IList<float>);

      // Act
      bool result = typeInfo.IsEnumerable(out Type? containedType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(containedType);
      Assert.AreEqual(typeof(float), containedType);
    }

    [TestMethod]
    public void When_instance_is_deeply_nested_derived_class_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(DeeplyNestedDerivedClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }
  }

  [ExcludeFromCodeCoverage]
  public class CustomEnumerableClass : IEnumerable<int>
  {
    public IEnumerator<int> GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  [ExcludeFromCodeCoverage]
  public class DerivedEnumerableClass : CustomEnumerableClass
  {
  }

  [ExcludeFromCodeCoverage]
  public class DeeplyNestedDerivedClass : DerivedEnumerableClass
  {
  }

  [ExcludeFromCodeCoverage]
  public class NonGenericClass
  {
    public string Name { get; set; } = string.Empty;
  }

  [ExcludeFromCodeCoverage]
  public struct CustomEnumerableStruct : IEnumerable<char>
  {
    public readonly IEnumerator<char> GetEnumerator() => "test".GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
