// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoModelBinderProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class QueryStringInfoModelBinderProviderTests
  {
    // When context is null, GetBinder should throw ArgumentNullException
    [TestMethod]
    public void When_ContextIsNull_GetBinder_Throws_ArgumentNullException()
    {
      // Arrange
      var provider = new QueryStringInfoModelBinderProvider();

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = provider.GetBinder(null!);
      });
    }

    // Provider should return a BinderTypeModelBinder for FilterInfoCollection.
    [TestMethod]
    public void When_ModelType_Is_FilterInfoCollection_GetBinder_Returns_BinderTypeModelBinder()
    {
      // Arrange
      var provider = new QueryStringInfoModelBinderProvider();
      var metadataProvider = new EmptyModelMetadataProvider();
      ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(FilterInfoCollection));

      var mockContext = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
      _ = mockContext.Setup(c => c.Metadata).Returns(metadata);

      // Act
      IModelBinder? binder = provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(binder);
      Assert.IsInstanceOfType(binder, typeof(BinderTypeModelBinder));
    }

    // Provider should return a BinderTypeModelBinder for types assignable to IEnumerable<SortInfo>.
    [TestMethod]
    public void When_ModelType_Is_AssignableTo_IEnumerableOfSortInfo_GetBinder_Returns_BinderTypeModelBinder()
    {
      // Arrange
      var provider = new QueryStringInfoModelBinderProvider();
      var metadataProvider = new EmptyModelMetadataProvider();
      ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(List<SortInfo>)); // assignable to IEnumerable<SortInfo>

      var mockContext = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
      _ = mockContext.Setup(c => c.Metadata).Returns(metadata);

      // Act
      IModelBinder? binder = provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(binder);
      Assert.IsInstanceOfType(binder, typeof(BinderTypeModelBinder));
    }

    // Provider should return a BinderTypeModelBinder for PageInfo.
    [TestMethod]
    public void When_ModelType_Is_PageInfo_GetBinder_Returns_BinderTypeModelBinder()
    {
      // Arrange
      var provider = new QueryStringInfoModelBinderProvider();
      var metadataProvider = new EmptyModelMetadataProvider();
      ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(PageInfo));

      var mockContext = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
      _ = mockContext.Setup(c => c.Metadata).Returns(metadata);

      // Act
      IModelBinder? binder = provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNotNull(binder);
      Assert.IsInstanceOfType(binder, typeof(BinderTypeModelBinder));
    }

    // When model type is unsupported, provider should return null.
    [TestMethod]
    public void When_ModelType_Is_Unsupported_GetBinder_Returns_Null()
    {
      // Arrange
      var provider = new QueryStringInfoModelBinderProvider();
      var metadataProvider = new EmptyModelMetadataProvider();
      ModelMetadata metadata = metadataProvider.GetMetadataForType(typeof(string)); // unsupported

      var mockContext = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
      _ = mockContext.Setup(c => c.Metadata).Returns(metadata);

      // Act
      IModelBinder? binder = provider.GetBinder(mockContext.Object);

      // Assert
      Assert.IsNull(binder);
    }
  }
}