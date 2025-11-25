// -----------------------------------------------------------------------
// <copyright file="TypeExtensionsTests.IsAssignableToGenericType.OpenGenerics.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Karma.Extensions.AspNetCore.Tests.Extensions
{
  public partial class TypeExtensionsTests
  {
    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_List_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(List<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_Dictionary_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(Dictionary<,>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_IEnumerable_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(IEnumerable<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_Dictionary_IsAssignableToGenericType_returns_false()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(Dictionary<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_List_and_genericType_is_open_generic_List_IsAssignableToGenericType_returns_true()
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
    public void When_instance_is_closed_generic_List_and_genericType_is_open_generic_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<string>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_custom_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(GenericEnumerableClass<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_custom_class_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(GenericEnumerableClass<int>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_derived_class_and_genericType_is_base_open_generic_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(DerivedGenericClass<>);
      Type genericType = typeof(BaseGenericClass<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_derived_class_and_genericType_is_base_open_generic_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(DerivedGenericClass<string>);
      Type genericType = typeof(BaseGenericClass<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_ICollection_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(ICollection<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_IList_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(IList<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_IList_and_genericType_is_ICollection_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(IList<>);
      Type genericType = typeof(ICollection<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_ICollection_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(ICollection<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_List_and_genericType_is_IList_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(List<>);
      Type genericType = typeof(IList<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_with_constraints_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(ConstrainedGenericClass<>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_satisfying_constraints_and_genericType_is_IEnumerable_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(ConstrainedGenericClass<List<int>>);
      Type genericType = typeof(IEnumerable<>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_multiple_parameters_and_genericType_matches_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(MultiParameterGenericClass<,>);
      Type genericType = typeof(MultiParameterGenericClass<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_closed_generic_multiple_parameters_and_genericType_is_open_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(MultiParameterGenericClass<int, string>);
      Type genericType = typeof(MultiParameterGenericClass<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_instance_is_open_generic_Dictionary_and_genericType_is_IDictionary_IsAssignableToGenericType_returns_true()
    {
      // Arrange
      Type instance = typeof(Dictionary<,>);
      Type genericType = typeof(IDictionary<,>);

      // Act
      bool result = instance.IsAssignableToGenericType(genericType);

      // Assert
      Assert.IsTrue(result);
    }
  }

  [ExcludeFromCodeCoverage]
  public class GenericEnumerableClass<T> : IEnumerable<T>
  {
    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  [ExcludeFromCodeCoverage]
  public class BaseGenericClass<T>
  {
    public T Value { get; set; } = default!;
  }

  [ExcludeFromCodeCoverage]
  public class DerivedGenericClass<T> : BaseGenericClass<T>
  {
  }

  [ExcludeFromCodeCoverage]
  public class ConstrainedGenericClass<T> : IEnumerable<T>
    where T : IEnumerable<int>
  {
    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  [ExcludeFromCodeCoverage]
  public class MultiParameterGenericClass<T1, T2>
  {
    public T1 First { get; set; } = default!;
    public T2 Second { get; set; } = default!;
  }
}
