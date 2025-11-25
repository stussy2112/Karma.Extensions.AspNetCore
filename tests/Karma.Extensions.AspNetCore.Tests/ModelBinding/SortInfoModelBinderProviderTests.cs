// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinderProviderTests.cs" company="Karma, LLC">
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
  public class SortInfoModelBinderProviderTests
  {
    private SortInfoModelBinderProvider _provider = null!;
    private Mock<IParseStrategy<IEnumerable<SortInfo>>> _mockParser = null!;

    [TestInitialize]
    public void TestInitialize()
    {
      _provider = new SortInfoModelBinderProvider();
      _mockParser = new Mock<IParseStrategy<IEnumerable<SortInfo>>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("sort");
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
    public void When_ModelType_is_IEnumerable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_List_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<List<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_not_related_to_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
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
      TestSortModelBinderProviderContext context = CreateContextWithParser();
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
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<int>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_ICollection_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<ICollection<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyCollection_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IReadOnlyCollection<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_array_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<SortInfo[]>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_called_multiple_times_with_same_context_GetBinder_returns_new_instances()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result1 = _provider.GetBinder(context);
      IModelBinder? result2 = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result1);
      Assert.IsNotNull(result2);
      Assert.AreNotSame(result1, result2);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result1);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result2);
    }

    [TestMethod]
    public void When_parser_service_not_registered_GetBinder_throws_InvalidOperationException()
    {
      // Arrange
      TestSortModelBinderProviderContext contextWithoutParser = CreateContextWithoutParser();
      contextWithoutParser.SetModelType<IEnumerable<SortInfo>>();

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _provider.GetBinder(contextWithoutParser));
    }

    [TestMethod]
    public void When_IEnumerable_of_SortInfo_type_GetBinder_retrieves_parser_from_services()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, context.GetServiceCallCount);
      Assert.IsTrue(context.RequestedServiceTypes.Contains(typeof(IParseStrategy<IEnumerable<SortInfo>>)));
    }

    [TestMethod]
    public void When_services_is_null_GetBinder_throws_NullReferenceException()
    {
      // Arrange
      TestSortModelBinderProviderContext contextWithNullServices = CreateContextWithNullServices();
      contextWithNullServices.SetModelType<IEnumerable<SortInfo>>();

      // Act & Assert
      _ = Assert.ThrowsExactly<NullReferenceException>(() => _provider.GetBinder(contextWithNullServices));
    }

    [TestMethod]
    public void When_ModelType_is_derived_type_that_does_not_implement_required_interface_GetBinder_returns_null()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<DerivedClassNotImplementingSortInfo>();

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
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(primitiveType);

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_custom_class_implementing_IEnumerable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<CustomSortInfoCollection>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_context_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));
      Assert.AreEqual("context", exception.ParamName);
    }

    [TestMethod]
    public void When_ModelType_is_generic_type_definition_GetBinder_returns_null()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(typeof(List<>));

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nested_generic_with_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<System.Threading.Tasks.Task<IEnumerable<SortInfo>>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_metadata_provider_is_null_GetBinder_does_not_throw()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParserAndNullMetadataProvider();
      context.SetModelType<IEnumerable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IQueryable_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IQueryable<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyList_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IReadOnlyList<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_HashSet_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<HashSet<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_single_SortInfo_GetBinder_returns_null()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<SortInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_IList_of_SortInfo_GetBinder_returns_SortInfoModelBinder()
    {
      // Arrange
      TestSortModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<IList<SortInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<SortInfoModelBinder>(result);
    }

    private TestSortModelBinderProviderContext CreateContextWithParser()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestSortModelBinderProviderContext(services.BuildServiceProvider());
    }

    private static TestSortModelBinderProviderContext CreateContextWithoutParser() =>
      new TestSortModelBinderProviderContext(new ServiceCollection().BuildServiceProvider());

    private static TestSortModelBinderProviderContext CreateContextWithNullServices() =>
      new TestSortModelBinderProviderContext(null);

    private TestSortModelBinderProviderContextWithNullMetadataProvider CreateContextWithParserAndNullMetadataProvider()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestSortModelBinderProviderContextWithNullMetadataProvider(services.BuildServiceProvider());
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestSortModelBinderProviderContext : ModelBinderProviderContext
  {
    private readonly IServiceProvider? _services;
    private readonly List<Type> _requestedServiceTypes = [];
    private ModelMetadata? _metadata;
    private readonly TrackingServiceProvider? _trackingServiceProvider;

    public TestSortModelBinderProviderContext(IServiceProvider? services)
    {
      _services = services;
      if (_services is not null)
      {
        _trackingServiceProvider = new TrackingServiceProvider(_services, _requestedServiceTypes);
      }
    }

    public override BindingInfo BindingInfo => new BindingInfo();

    public override ModelMetadata Metadata => _metadata ?? throw new InvalidOperationException("ModelMetadata not set. Call SetModelType<T>() first.");

    public override IModelMetadataProvider MetadataProvider => new TestSortMetadataProvider();

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

    public override IModelBinder CreateBinder(ModelMetadata metadata) => new TestSortModelBinder();

    private static DefaultModelMetadata CreateModelMetadata(Type type)
    {
      var identity = ModelMetadataIdentity.ForType(type);
      var attributes = ModelAttributes.GetAttributesForType(type);

      return new DefaultModelMetadata(
        new TestSortMetadataProvider(),
        new TestSortCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TrackingSortServiceProvider : IServiceProvider
  {
    private readonly IServiceProvider _innerProvider;
    private readonly ICollection<Type> _requestedTypes;

    public TrackingSortServiceProvider(IServiceProvider innerProvider, ICollection<Type> requestedTypes)
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
  public class TestSortModelBinderProviderContextWithNullMetadataProvider : TestSortModelBinderProviderContext
  {
    public TestSortModelBinderProviderContextWithNullMetadataProvider(IServiceProvider? services) : base(services)
    {
    }

    public override IModelMetadataProvider MetadataProvider => null!;
  }

  [ExcludeFromCodeCoverage]
  public class TestSortMetadataProvider : IModelMetadataProvider
  {
    public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType) => [];

    public ModelMetadata GetMetadataForType(Type modelType)
    {
      var identity = ModelMetadataIdentity.ForType(modelType);
      var attributes = ModelAttributes.GetAttributesForType(modelType);

      return new DefaultModelMetadata(
        this,
        new TestSortCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestSortCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
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
  public class TestSortModelBinder : IModelBinder
  {
    public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
    {
      ArgumentNullException.ThrowIfNull(bindingContext);
      bindingContext.Result = ModelBindingResult.Success(null);
      return System.Threading.Tasks.Task.CompletedTask;
    }
  }

  [ExcludeFromCodeCoverage]
  public class DerivedClassNotImplementingSortInfo
  {
    public string Name { get; set; } = string.Empty;
  }

  [ExcludeFromCodeCoverage]
  public class CustomSortInfoCollection : IEnumerable<SortInfo>
  {
    private readonly List<SortInfo> _items = [];

    public IEnumerator<SortInfo> GetEnumerator() => _items.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
