// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class PageInfoModelBinderProviderTests
  {
    private PageInfoModelBinderProvider _provider = null!;
    private TestModelBinderProviderContext _testContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
      _provider = new PageInfoModelBinderProvider();
      _testContext = new TestModelBinderProviderContext();
    }

    [TestMethod]
    public void When_context_is_null_GetBinder_throws_ArgumentNullException() =>
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => _provider.GetBinder(null!));

    [TestMethod]
    public void When_ModelType_is_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_not_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<string>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_derived_from_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      _testContext.SetModelType<DerivedPageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_object_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<object>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_int_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<int>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nullable_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      _testContext.SetModelType<PageInfo?>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_generic_collection_of_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<List<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_array_of_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<PageInfo[]>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_valid_PageInfo_type_GetBinder_creates_binder_with_string_and_uint_binders()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      var pageInfoBinder = result as PageInfoModelBinder;
      Assert.IsNotNull(pageInfoBinder);

      // Verify that the context was used to create string and uint binders
      Assert.AreEqual(2, _testContext.CreateBinderCallCount);
      Assert.IsTrue(_testContext.CreatedBinderTypes.Contains(typeof(string)));
      Assert.IsTrue(_testContext.CreatedBinderTypes.Contains(typeof(uint)));
    }

    [TestMethod]
    public void When_called_multiple_times_with_same_context_GetBinder_returns_new_instances()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();

      // Act
      IModelBinder? result1 = _provider.GetBinder(_testContext);
      IModelBinder? result2 = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result1);
      Assert.IsNotNull(result2);
      Assert.AreNotSame(result1, result2);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result1);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result2);
    }

    [TestMethod]
    public void When_ModelType_implements_interface_that_PageInfo_implements_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<IEquatable<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_base_type_of_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<ValueType>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_context_metadata_provider_returns_different_types_GetBinder_uses_correct_metadata()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();
      _testContext.SetCustomStringMetadata("CustomStringMetadata");
      _testContext.SetCustomUintMetadata("CustomUintMetadata");

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
      
      // Verify that different metadata objects were created and used
      Assert.AreEqual(2, _testContext.CreateBinderCallCount);
      Assert.IsTrue(_testContext.CreatedBinderTypes.Contains(typeof(string)));
      Assert.IsTrue(_testContext.CreatedBinderTypes.Contains(typeof(uint)));
      
      // Verify the custom metadata was actually used
      Assert.IsTrue(_testContext.CustomStringMetadataWasUsed);
      Assert.IsTrue(_testContext.CustomUintMetadataWasUsed);
    }

    [TestMethod]
    public void When_PageInfo_type_GetBinder_requests_metadata_for_string_and_uint_types()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
      Assert.AreEqual(2, _testContext.GetMetadataForTypeCallCount);
      Assert.IsTrue(_testContext.RequestedMetadataTypes.Contains(typeof(string)));
      Assert.IsTrue(_testContext.RequestedMetadataTypes.Contains(typeof(uint)));
    }

    [TestMethod]
    public void When_metadata_provider_throws_exception_GetBinder_propagates_exception()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();
      _testContext.SetThrowExceptionOnGetMetadata(true);

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _provider.GetBinder(_testContext));
    }

    [TestMethod]
    public void When_create_binder_throws_exception_GetBinder_propagates_exception()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();
      _testContext.SetThrowExceptionOnCreateBinder(true);

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _provider.GetBinder(_testContext));
    }

    [TestMethod]
    public void When_metadata_provider_is_null_GetBinder_throws_NullReferenceException()
    {
      // Arrange
      var contextWithNullProvider = new TestModelBinderProviderContextWithNullProvider();
      contextWithNullProvider.SetModelType<PageInfo>();

      // Act & Assert
      _ = Assert.ThrowsExactly<NullReferenceException>(() => _provider.GetBinder(contextWithNullProvider));
    }

    [TestMethod]
    public void When_ModelType_is_generic_type_definition_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelTypeFromType(typeof(List<>));

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nested_generic_with_PageInfo_GetBinder_returns_null()
    {
      // Arrange
      _testContext.SetModelType<Task<PageInfo>>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_binder_creation_order_matters_GetBinder_creates_string_binder_first()
    {
      // Arrange
      _testContext.SetModelType<PageInfo>();
      _testContext.TrackBinderCreationOrder = true;

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(2, _testContext.CreateBinderCallCount);
      Assert.AreEqual(typeof(string), _testContext.CreatedBinderTypes[0]);
      Assert.AreEqual(typeof(uint), _testContext.CreatedBinderTypes[1]);
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
    public void When_ModelType_is_numeric_type_GetBinder_returns_null(Type numericType)
    {
      // Arrange
      _testContext.SetModelTypeFromType(numericType);

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_deeply_derived_from_PageInfo_GetBinder_returns_PageInfoModelBinder()
    {
      // Arrange
      _testContext.SetModelType<DoublyDerivedPageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(_testContext);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }

    [TestMethod]
    public void When_context_services_is_null_GetBinder_still_creates_binder()
    {
      // Arrange
      var contextWithNullServices = new TestModelBinderProviderContextWithNullServices();
      contextWithNullServices.SetModelType<PageInfo>();

      // Act
      IModelBinder? result = _provider.GetBinder(contextWithNullServices);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<PageInfoModelBinder>(result);
    }
  }

  [ExcludeFromCodeCoverage]
  public record DerivedPageInfo : PageInfo
  {
    public DerivedPageInfo() : base()
    {
    }

    public DerivedPageInfo(string after, uint limit = uint.MaxValue) : base(after, limit)
    {
    }

    public DerivedPageInfo(uint offset, uint limit = uint.MaxValue) : base(offset, limit)
    {
    }
  }

  [ExcludeFromCodeCoverage]
  public record DoublyDerivedPageInfo : DerivedPageInfo
  {
    public DoublyDerivedPageInfo() : base()
    {
    }

    public DoublyDerivedPageInfo(string after, uint limit = uint.MaxValue) : base(after, limit)
    {
    }

    public DoublyDerivedPageInfo(uint offset, uint limit = uint.MaxValue) : base(offset, limit)
    {
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestModelBinderProviderContext : ModelBinderProviderContext
  {
    private readonly Dictionary<Type, ModelMetadata> _metadataCache = [];
    private readonly List<Type> _createdBinderTypes = [];
    private readonly List<Type> _requestedMetadataTypes = [];
    private ModelMetadata? _metadata;
    private string? _customStringMetadata;
    private string? _customUintMetadata;

    private bool _throwExceptionOnGetMetadata;
    private bool _throwExceptionOnCreateBinder;
    public bool TrackBinderCreationOrder
    {
      get; set;
    }

    public int CreateBinderCallCount
    {
      get; private set;
    }
    public int GetMetadataForTypeCallCount
    {
      get; private set;
    }
    public IReadOnlyList<Type> CreatedBinderTypes => _createdBinderTypes.AsReadOnly();
    public IReadOnlyList<Type> RequestedMetadataTypes => _requestedMetadataTypes.AsReadOnly();

    public override BindingInfo BindingInfo => new BindingInfo();

    public override ModelMetadata Metadata => _metadata ?? throw new InvalidOperationException("ModelMetadata not set. Call SetModelType<T>() first.");

    public override IModelMetadataProvider MetadataProvider => new TestMetadataProvider(this);

    public override IServiceProvider Services => CreateServiceProvider();

    public void SetModelType<T>() => _metadata = CreateModelMetadata(typeof(T));

    public void SetCustomStringMetadata(string metadata) => _customStringMetadata = metadata;

    public void SetCustomUintMetadata(string metadata) => _customUintMetadata = metadata;

    public void SetThrowExceptionOnGetMetadata(bool throwException) => _throwExceptionOnGetMetadata = throwException;

    public void SetThrowExceptionOnCreateBinder(bool throwException) => _throwExceptionOnCreateBinder = throwException;

    public void SetModelTypeFromType(Type type) => _metadata = CreateModelMetadata(type);

    public bool CustomStringMetadataWasUsed
    {
      get; private set;
    }

    public bool CustomUintMetadataWasUsed
    {
      get; private set;
    }

    internal ModelMetadata GetMetadataForTypeInternal(Type type)
    {
      if (_throwExceptionOnGetMetadata)
      {
        throw new InvalidOperationException("Test exception in GetMetadataForType");
      }

      GetMetadataForTypeCallCount++;
      _requestedMetadataTypes.Add(type);

      if (!_metadataCache.TryGetValue(type, out ModelMetadata? metadata))
      {
        // Use custom metadata if available
        if (type == typeof(string) && !string.IsNullOrEmpty(_customStringMetadata))
        {
          CustomStringMetadataWasUsed = true;
          metadata = CreateCustomModelMetadata(type, _customStringMetadata);
        }
        else if (type == typeof(uint) && !string.IsNullOrEmpty(_customUintMetadata))
        {
          CustomUintMetadataWasUsed = true;
          metadata = CreateCustomModelMetadata(type, _customUintMetadata);
        }
        else
        {
          metadata = CreateModelMetadata(type);
        }
        
        _metadataCache[type] = metadata;
      }

      return metadata;
    }

    private static DefaultModelMetadata CreateCustomModelMetadata(Type type, string customData)
    {
      var identity = ModelMetadataIdentity.ForType(type);
      var attributes = ModelAttributes.GetAttributesForType(type);
    
      // Create metadata with custom display name to differentiate
      var details = new DefaultMetadataDetails(identity, attributes)
      {
        DisplayMetadata = new DisplayMetadata
        {
          DisplayName = () => customData
        }
      };

      return new DefaultModelMetadata(
        new TestMetadataProvider(null!),
        new TestCompositeMetadataDetailsProvider(),
        details);
    }

    public override IModelBinder CreateBinder(ModelMetadata metadata)
    {
      if (_throwExceptionOnCreateBinder)
      {
        throw new InvalidOperationException("Test exception in CreateBinder");
      }

      ArgumentNullException.ThrowIfNull(metadata);

      CreateBinderCallCount++;
      _createdBinderTypes.Add(metadata.ModelType);

      return new TestModelBinder();
    }

    private static DefaultModelMetadata CreateModelMetadata(Type type)
    {
      var identity = ModelMetadataIdentity.ForType(type);

      var attributes = ModelAttributes.GetAttributesForType(type);

      return new DefaultModelMetadata(
        new TestMetadataProvider(null!),
        new TestCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }

    private static ServiceProvider CreateServiceProvider()
    {
      var services = new ServiceCollection();
      _ = services.AddOptions();
      return services.BuildServiceProvider();
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestMetadataProvider : IModelMetadataProvider
  {
    private readonly TestModelBinderProviderContext _context;

    public TestMetadataProvider(TestModelBinderProviderContext context) => _context = context;

    public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType) => [];

    public ModelMetadata GetMetadataForType(Type modelType) => _context?.GetMetadataForTypeInternal(modelType) ??
             throw new InvalidOperationException("TestModelBinderProviderContext not available");
  }

  [ExcludeFromCodeCoverage]
  public class TestCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
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
  public class TestModelBinder : IModelBinder
  {
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
      ArgumentNullException.ThrowIfNull(bindingContext);
      bindingContext.Result = ModelBindingResult.Success(null);
      return Task.CompletedTask;
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestModelBinderProviderContextWithNullProvider : TestModelBinderProviderContext
  {
    public override IModelMetadataProvider MetadataProvider => null!;
  }

  [ExcludeFromCodeCoverage]
  public class TestModelBinderProviderContextWithNullServices : TestModelBinderProviderContext
  {
    public override IServiceProvider Services => null!;
  }
}