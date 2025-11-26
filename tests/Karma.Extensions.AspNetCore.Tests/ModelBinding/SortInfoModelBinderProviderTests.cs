// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinderProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class SortInfoModelBinderProviderTests
  {
    private SortInfoModelBinderProvider _provider = null!;

    [TestInitialize]
    public void TestInitialize() => _provider = new SortInfoModelBinderProvider();

    [TestCleanup]
    public void TestCleanup() => _provider = null!;

    [TestMethod]
    public void When_context_is_null_GetBinder_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));

    [TestMethod]
    public void When_context_is_null_GetBinder_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));
      Assert.AreEqual("context", exception.ParamName);
    }

    [TestMethod]
    public void When_ModelType_is_IEnumerable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_List_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<List<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_ICollection_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<ICollection<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyCollection_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IReadOnlyCollection<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyList_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IReadOnlyList<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IList_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IList<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_array_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<SortInfo[]>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_HashSet_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<HashSet<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IQueryable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IQueryable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_custom_class_implementing_IEnumerable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<CustomSortInfoCollection>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_not_related_to_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<string>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_object_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<object>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_int_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<int>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_single_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<SortInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_derived_type_that_does_not_implement_required_interface_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<DerivedClassNotImplementingSortInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(typeof(sbyte))]
    [DataRow(typeof(byte))]
    [DataRow(typeof(short))]
    [DataRow(typeof(ushort))]
    [DataRow(typeof(int))]
    [DataRow(typeof(long))]
    [DataRow(typeof(ulong))]
    [DataRow(typeof(decimal))]
    [DataRow(typeof(double))]
    [DataRow(typeof(float))]
    [DataRow(typeof(bool))]
    [DataRow(typeof(char))]
    public void When_ModelType_is_primitive_type_GetBinder_returns_null(Type primitiveType)
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContextForType(primitiveType);

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_generic_type_definition_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContextForType(typeof(List<>));

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nested_generic_with_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<System.Threading.Tasks.Task<IEnumerable<SortInfo>>>(
        );

      // Act
      IModelBinder? result = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_called_multiple_times_with_same_context_GetBinder_returns_new_instances()
    {
      // Arrange
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result1 = _provider.GetBinder(mockContext.Object);
      IModelBinder? result2 = _provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(result1);
      Assert.IsNotNull(result2);
      Assert.AreNotSame(result1, result2);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result1);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result2);
    }

    [TestMethod]
    public void When_provider_created_implements_IModelBinderProvider()
    {
      // Act
      SortInfoModelBinderProvider provider = new();

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinderProvider>(provider);
    }

    [TestMethod]
    public void When_provider_called_multiple_times_different_providers_return_different_binders()
    {
      // Arrange
      SortInfoModelBinderProvider provider1 = new();
      SortInfoModelBinderProvider provider2 = new();
      Mock<ModelBinderProviderContext> mockContext = CreateMockContext<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? binder1 = provider1.GetBinder(mockContext.Object);
      IModelBinder? binder2 = provider2.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    private static Mock<ModelBinderProviderContext> CreateMockContext<T>()
    {
      Mock<ModelBinderProviderContext> mockContext = new();
      ModelMetadata metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(T));

      _ = mockContext.Setup((x) => x.Metadata).Returns(metadata);

      return mockContext;
    }

    private static Mock<ModelBinderProviderContext> CreateMockContextForType(Type type)
    {
      Mock<ModelBinderProviderContext> mockContext = new();
      ModelMetadata metadata = new EmptyModelMetadataProvider().GetMetadataForType(type);

      _ = mockContext.Setup((x) => x.Metadata).Returns(metadata);

      return mockContext;
    }
  }

  // Test helper classes
  [ExcludeFromCodeCoverage]
  public class CustomSortInfoCollection : List<SortInfo>
  {
  }

  [ExcludeFromCodeCoverage]
  public class DerivedClassNotImplementingSortInfo
  {
    public string Name { get; set; } = string.Empty;
  }
}
