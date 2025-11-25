// -----------------------------------------------------------------------
// <copyright file="TypeExtensionsTests.IsAssignableToGenericType.WithOutParameter.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests.Extensions
{
  public partial class TypeExtensionsTests
  {
    [TestMethod]
    public void When_instance_is_null_IsAssignableToGenericType_with_out_returns_false_and_null()
    {
      // Arrange
      Type? instance = null;
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(foundType);
    }

    [TestMethod]
    public void When_genericType_is_null_IsAssignableToGenericType_with_out_returns_false_and_null()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type? genericType = null;

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(foundType);
    }

    [TestMethod]
    public void When_both_parameters_are_null_IsAssignableToGenericType_with_out_returns_false_and_null()
    {
      // Arrange
      Type? instance = null;
      Type? genericType = null;

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(foundType);
    }

    [TestMethod]
    public void When_instance_is_List_of_int_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_int()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<int>), foundType);
    }

    [TestMethod]
    public void When_instance_is_List_of_int_and_genericType_is_List_IsAssignableToGenericType_with_out_returns_true_and_List_of_int()
    {
      // Arrange
      Type instance = typeof(List<int>);
      Type genericType = typeof(List<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(List<int>), foundType);
    }

    [TestMethod]
    public void When_instance_is_Dictionary_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_KeyValuePair()
    {
      // Arrange
      Type instance = typeof(Dictionary<string, int>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<KeyValuePair<string, int>>), foundType);
    }

    [TestMethod]
    public void When_instance_is_Dictionary_and_genericType_is_IDictionary_IsAssignableToGenericType_with_out_returns_true_and_IDictionary_of_string_int()
    {
      // Arrange
      Type instance = typeof(Dictionary<string, int>);
      Type genericType = typeof(IDictionary<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IDictionary<string, int>), foundType);
    }

    [TestMethod]
    public void When_instance_does_not_match_genericType_IsAssignableToGenericType_with_out_returns_false_and_null()
    {
      // Arrange
      Type instance = typeof(int);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(foundType);
    }

    [TestMethod]
    public void When_instance_is_IEnumerable_of_string_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_same_type()
    {
      // Arrange
      Type instance = typeof(IEnumerable<string>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<string>), foundType);
    }

    [TestMethod]
    public void When_instance_is_ICollection_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_interface()
    {
      // Arrange
      Type instance = typeof(ICollection<double>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<double>), foundType);
    }

    [TestMethod]
    public void When_instance_is_custom_enumerable_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_int()
    {
      // Arrange
      Type instance = typeof(CustomEnumerableClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<int>), foundType);
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_open_IEnumerable()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.IsTrue(foundType.IsGenericType);
      Assert.AreEqual(typeof(IEnumerable<>), foundType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_List_IsAssignableToGenericType_with_out_returns_true_and_open_List()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(List<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(List<>), foundType);
    }

    [TestMethod]
    public void When_instance_is_open_generic_Dictionary_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_open_IEnumerable()
    {
      // Arrange
      Type instance = typeof(Dictionary<,>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.IsTrue(foundType.IsGenericType);
      Assert.AreEqual(typeof(IEnumerable<>), foundType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void When_instance_is_open_generic_IList_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_open_IEnumerable()
    {
      // Arrange
      Type instance = typeof(IList<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.IsTrue(foundType.IsGenericType);
      Assert.AreEqual(typeof(IEnumerable<>), foundType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void When_instance_is_open_generic_IList_and_genericType_is_ICollection_IsAssignableToGenericType_with_out_returns_true_and_open_ICollection()
    {
      // Arrange
      Type instance = typeof(IList<>);
      Type genericType = typeof(ICollection<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.IsTrue(foundType.IsGenericType);
      Assert.AreEqual(typeof(ICollection<>), foundType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void When_instance_is_HashSet_of_string_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_string()
    {
      // Arrange
      Type instance = typeof(HashSet<string>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<string>), foundType);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_custom_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_T()
    {
      // Arrange
      Type instance = typeof(GenericEnumerableClass<decimal>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<decimal>), foundType);
    }

    [TestMethod]
    public void When_instance_is_open_generic_custom_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_open_IEnumerable()
    {
      // Arrange
      Type instance = typeof(GenericEnumerableClass<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.IsTrue(foundType.IsGenericType);
      Assert.AreEqual(typeof(IEnumerable<>), foundType.GetGenericTypeDefinition());
    }

    [TestMethod]
    public void When_instance_is_IQueryable_of_string_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_string()
    {
      // Arrange
      Type instance = typeof(IQueryable<string>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<string>), foundType);
    }

    [TestMethod]
    public void When_instance_is_string_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_char()
    {
      // Arrange
      Type instance = typeof(string);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<char>), foundType);
    }

    [TestMethod]
    public void When_instance_is_List_of_List_of_int_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_List_of_int()
    {
      // Arrange
      Type instance = typeof(List<List<int>>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<List<int>>), foundType);
    }

    [TestMethod]
    public void When_instance_is_non_generic_type_IsAssignableToGenericType_with_out_returns_false_and_null()
    {
      // Arrange
      Type instance = typeof(NonGenericClass);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(foundType);
    }

    [TestMethod]
    public void When_instance_is_struct_implementing_IEnumerable_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_char()
    {
      // Arrange
      Type instance = typeof(CustomEnumerableStruct);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<char>), foundType);
    }

    [TestMethod]
    public void When_instance_is_IReadOnlyCollection_of_bool_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_of_bool()
    {
      // Arrange
      Type instance = typeof(IReadOnlyCollection<bool>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<bool>), foundType);
    }

    [TestMethod]
    public void When_instance_is_multi_parameter_generic_and_genericType_matches_IsAssignableToGenericType_with_out_returns_true_and_same_type()
    {
      // Arrange
      Type instance = typeof(MultiParameterGenericClass<int, string>);
      Type genericType = typeof(MultiParameterGenericClass<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(MultiParameterGenericClass<int, string>), foundType);
    }

    [TestMethod]
    public void When_instance_is_constrained_generic_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_with_out_returns_true_and_IEnumerable_interface()
    {
      // Arrange
      Type instance = typeof(ConstrainedGenericClass<List<int>>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType, out Type? foundType);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(foundType);
      Assert.AreEqual(typeof(IEnumerable<List<int>>), foundType);
    }
  }
}
