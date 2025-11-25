// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class PageInfoModelBinderProviderTests
  {
    private PageInfoModelBinderProvider _provider = null!;
    private Mock<IParseStrategy<PageInfo>> _mockParser = null!;

    [TestInitialize]
    public void TestInitialize()
    {
      _provider = new PageInfoModelBinderProvider();
      _mockParser = new Mock<IParseStrategy<PageInfo>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("page");
    }

    [TestCleanup]
    public void TestCleanup()
    {
      _provider = null!;
      _mockParser = null!;
    }

    [TestMethod]
    public void When_context_is_null_GetBinder_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));

    [TestMethod]
    public void When_context_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));
      Assert.AreEqual("context", exception.ParamName);
    }

    [TestMethod]
    public void When_ModelType_is_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_nullable_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo?>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_not_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<string>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_object_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<object>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_int_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<int>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

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
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(primitiveType);

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_called_multiple_times_with_same_context_GetBinder_returns_new_instances()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result1 = _provider.GetBinder(context);
      IModelBinder? result2 = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result1);
      Assert.IsNotNull(result2);
      Assert.AreNotSame(result1, result2);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result1);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result2);
    }

    [TestMethod]
    public void When_parser_service_not_registered_GetBinder_throws_InvalidOperationException()
    {
      // Arrange
      TestPageModelBinderProviderContext contextWithoutParser = CreateContextWithoutParser();
      contextWithoutParser.SetModelType<PageInfo>();

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _provider.GetBinder(contextWithoutParser));
    }

    [TestMethod]
    public void When_PageInfo_type_GetBinder_retrieves_parser_from_services()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, context.GetServiceCallCount);
      Assert.IsTrue(context.RequestedServiceTypes.Contains(typeof(IParseStrategy<PageInfo>)));
    }

    [TestMethod]
    public void When_services_is_null_GetBinder_throws_NullReferenceException()
    {
      // Arrange
      TestPageModelBinderProviderContext contextWithNullServices = CreateContextWithNullServices();
      contextWithNullServices.SetModelType<PageInfo>();

      // Act & Assert
      _ = Assert.ThrowsExactly<NullReferenceException>(() => _provider.GetBinder(contextWithNullServices));
    }

    [TestMethod]
    public void When_ModelType_is_derived_class_not_assignable_to_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<DerivedClassNotPageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_FilterInfoCollection_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<FilterInfoCollection>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_generic_type_definition_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(typeof(List<>));

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nested_generic_with_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<System.Threading.Tasks.Task<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_metadata_provider_is_null_GetBinder_does_not_throw()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParserAndNullMetadataProvider();
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_array_type_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo[]>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_List_of_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<List<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_IEnumerable_of_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IEnumerable<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_GetBinder_succeeds_returns_binder_with_registered_parser()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var services = new ServiceCollection();
      _ = services.AddSingleton<IParseStrategy<PageInfo>>(parser);
      TestPageModelBinderProviderContext context = new (services.BuildServiceProvider());
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_provider_implements_IModelBinderProvider_GetBinder_is_callable()
    {
      // Arrange
      IModelBinderProvider provider = _provider;
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_provider_created_multiple_times_instances_are_independent()
    {
      // Arrange
      PageInfoModelBinderProvider provider1 = new ();
      PageInfoModelBinderProvider provider2 = new ();

      // Assert
      Assert.IsNotNull(provider1);
      Assert.IsNotNull(provider2);
      Assert.AreNotSame(provider1, provider2);
    }

    [TestMethod]
    public void When_ModelType_is_struct_not_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<CustomStruct>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_interface_not_related_to_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      TestPageModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<ICustomInterface>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    private TestPageModelBinderProviderContext CreateContextWithParser()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestPageModelBinderProviderContext(services.BuildServiceProvider());
    }

    private static TestPageModelBinderProviderContext CreateContextWithoutParser() => new TestPageModelBinderProviderContext(new ServiceCollection().BuildServiceProvider());

    private static TestPageModelBinderProviderContext CreateContextWithNullServices() => new TestPageModelBinderProviderContext(null);

    private TestPageModelBinderProviderContextWithNullMetadataProvider CreateContextWithParserAndNullMetadataProvider()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestPageModelBinderProviderContextWithNullMetadataProvider(services.BuildServiceProvider());
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestPageModelBinderProviderContext : ModelBinderProviderContext
  {
    private readonly IServiceProvider? _services;
    private readonly List<Type> _requestedServiceTypes = [];
    private ModelMetadata? _metadata;
    private readonly TrackingPageServiceProvider? _trackingServiceProvider;

    public TestPageModelBinderProviderContext(IServiceProvider? services)
    {
      _services = services;
      if (_services is not null)
      {
        _trackingServiceProvider = new TrackingPageServiceProvider(_services, _requestedServiceTypes);
      }
    }

    public override BindingInfo BindingInfo => new BindingInfo();

    public override ModelMetadata Metadata => _metadata ?? throw new InvalidOperationException("ModelMetadata not set. Call SetModelType<T>() first.");

    public override IModelMetadataProvider MetadataProvider => new TestPageMetadataProvider();

    public override IServiceProvider Services
    {
      get
      {
        GetServiceCallCount++;
        if (_trackingServiceProvider is not null)
        {
          return _trackingServiceProvider;
        }

        return _services ?? throw new NullReferenceException("Services is null");
      }
    }

    public int GetServiceCallCount { get; private set; }

    public IReadOnlyList<Type> RequestedServiceTypes => _requestedServiceTypes.AsReadOnly();

    public void SetModelType<T>() => _metadata = CreateModelMetadata(typeof(T));

    public void SetModelTypeFromType(Type type) => _metadata = CreateModelMetadata(type);

    public override IModelBinder CreateBinder(ModelMetadata metadata) => new TestPageModelBinder();

    private static DefaultModelMetadata CreateModelMetadata(Type type)
    {
      var identity = ModelMetadataIdentity.ForType(type);
      var attributes = ModelAttributes.GetAttributesForType(type);

      return new DefaultModelMetadata(
        new TestPageMetadataProvider(),
        new TestPageCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TrackingPageServiceProvider : IServiceProvider
  {
    private readonly IServiceProvider _innerProvider;
    private readonly ICollection<Type> _requestedTypes;

    public TrackingPageServiceProvider(IServiceProvider innerProvider, ICollection<Type> requestedTypes)
    {
      _innerProvider = innerProvider;
      _requestedTypes = requestedTypes;
    }

    public object? GetService(Type serviceType)
    {
      _requestedTypes.Add(serviceType);
      return _innerProvider.GetService(serviceType);
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestPageModelBinderProviderContextWithNullMetadataProvider : TestPageModelBinderProviderContext
  {
    public TestPageModelBinderProviderContextWithNullMetadataProvider(IServiceProvider? services) : base(services)
    {
    }

    public override IModelMetadataProvider MetadataProvider => null!;
  }

  [ExcludeFromCodeCoverage]
  public class TestPageMetadataProvider : IModelMetadataProvider
  {
    public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType) => [];

    public ModelMetadata GetMetadataForType(Type modelType)
    {
      var identity = ModelMetadataIdentity.ForType(modelType);
      var attributes = ModelAttributes.GetAttributesForType(modelType);

      return new DefaultModelMetadata(
        this,
        new TestPageCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestPageCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
  {
    public void CreateBindingMetadata(BindingMetadataProviderContext context)
    {
    }

    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
    }

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestPageModelBinder : IModelBinder
  {
    public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
    {
      ArgumentNullException.ThrowIfNull(bindingContext);
      bindingContext.Result = ModelBindingResult.Success(null);
      return System.Threading.Tasks.Task.CompletedTask;
    }
  }

  [ExcludeFromCodeCoverage]
  public class DerivedClassNotPageInfo
  {
    public string Name { get; set; } = string.Empty;
  }

  [ExcludeFromCodeCoverage]
  public struct CustomStruct
  {
    public int Value { get; set; }
  }

  public interface ICustomInterface
  {
    string Name { get; }
  }
}
