// -----------------------------------------------------------------------
// <copyright file="QueryStringParserModelBinderProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class QueryStringParserModelBinderProviderTests
  {
    private QueryStringParserModelBinderProvider<TestModelBinder, TestModel> _provider = null!;
    private Mock<IParseStrategy<TestModel>> _mockParser = null!;

    [TestInitialize]
    public void TestInitialize()
    {
      _provider = new QueryStringParserModelBinderProvider<TestModelBinder, TestModel>();
      _mockParser = new Mock<IParseStrategy<TestModel>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("test");
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
    public void When_ModelType_is_TestModel_GetBinder_returns_TestModelBinder()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_not_assignable_to_T_GetBinder_returns_null()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
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
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
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
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<int>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_derived_from_T_GetBinder_returns_binder()
    {
      // Arrange
      var provider = new QueryStringParserModelBinderProvider<TestModelBinder, TestModel>();
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<DerivedTestModel>();

      // Act
      IModelBinder? result = provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result);
    }

    [TestMethod]
    public void When_called_multiple_times_with_same_context_GetBinder_returns_new_instances()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result1 = _provider.GetBinder(context);
      IModelBinder? result2 = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result1);
      Assert.IsNotNull(result2);
      Assert.AreNotSame(result1, result2);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result1);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result2);
    }

    [TestMethod]
    public void When_parser_service_not_registered_GetBinder_throws_InvalidOperationException()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext contextWithoutParser = CreateContextWithoutParser();
      contextWithoutParser.SetModelType<TestModel>();

      // Act & Assert
      _ = Assert.ThrowsExactly<InvalidOperationException>(() => _provider.GetBinder(contextWithoutParser));
    }

    [TestMethod]
    public void When_TestModel_type_GetBinder_retrieves_parser_from_services()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, context.GetServiceCallCount);
      Assert.IsTrue(context.RequestedServiceTypes.Any((t) => t == typeof(IParseStrategy<TestModel>)));
    }

    [TestMethod]
    public void When_services_is_null_GetBinder_throws_NullReferenceException()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext contextWithNullServices = CreateContextWithNullServices();
      contextWithNullServices.SetModelType<TestModel>();

      // Act & Assert
      _ = Assert.ThrowsExactly<NullReferenceException>(() => _provider.GetBinder(contextWithNullServices));
    }

    [TestMethod]
    public void When_ModelType_is_unrelated_type_GetBinder_returns_null()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<UnrelatedModel>();

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
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(primitiveType);

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nullable_TestModel_GetBinder_returns_binder()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel?>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result);
    }

    [TestMethod]
    public void When_metadata_provider_is_null_GetBinder_does_not_throw()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContextWithNullMetadataProvider context = CreateContextWithParserAndNullMetadataProvider();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<TestModelBinder>(result);
    }

    [TestMethod]
    public void When_ModelType_is_generic_type_definition_GetBinder_returns_null()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelTypeFromType(typeof(List<>));

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_ModelType_is_nested_generic_with_TestModel_GetBinder_returns_null()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<System.Threading.Tasks.Task<TestModel>>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_provider_created_implements_IModelBinderProvider()
    {
      // Act
      var provider = new QueryStringParserModelBinderProvider<TestModelBinder, TestModel>();

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinderProvider>(provider);
    }

    [TestMethod]
    public void When_different_binder_types_used_GetBinder_returns_correct_binder_type()
    {
      // Arrange
      var provider = new QueryStringParserModelBinderProvider<AlternateModelBinder, TestModel>();
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<AlternateModelBinder>(result);
    }

    [TestMethod]
    public void When_parser_is_retrieved_from_services_GetBinder_uses_correct_parser()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      var testBinder = result as TestModelBinder;
      Assert.IsNotNull(testBinder);
      Assert.AreSame(_mockParser.Object, testBinder.Parser);
    }

    [TestMethod]
    public void When_multiple_different_providers_created_GetBinder_returns_different_binder_instances()
    {
      // Arrange
      var provider1 = new QueryStringParserModelBinderProvider<TestModelBinder, TestModel>();
      var provider2 = new QueryStringParserModelBinderProvider<TestModelBinder, TestModel>();
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? binder1 = provider1.GetBinder(context);
      IModelBinder? binder2 = provider2.GetBinder(context);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_context_metadata_ModelType_matches_T_GetBinder_returns_binder()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
    }

    [TestMethod]
    public void When_context_metadata_ModelType_does_not_match_T_GetBinder_returns_null()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<DifferentModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_binder_type_has_constructor_with_parser_parameter_GetBinder_creates_instance_successfully()
    {
      // Arrange
      TestQueryStringParserModelBinderProviderContext context = CreateContextWithParser();
      context.SetModelType<TestModel>();

      // Act
      IModelBinder? result = _provider.GetBinder(context);

      // Assert
      Assert.IsNotNull(result);
      var testBinder = result as TestModelBinder;
      Assert.IsNotNull(testBinder);
      Assert.IsNotNull(testBinder.Parser);
    }

    private TestQueryStringParserModelBinderProviderContext CreateContextWithParser()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestQueryStringParserModelBinderProviderContext(services.BuildServiceProvider());
    }

    private static TestQueryStringParserModelBinderProviderContext CreateContextWithoutParser() => new TestQueryStringParserModelBinderProviderContext(new ServiceCollection().BuildServiceProvider());

    private static TestQueryStringParserModelBinderProviderContext CreateContextWithNullServices() => new TestQueryStringParserModelBinderProviderContext(null);

    private TestQueryStringParserModelBinderProviderContextWithNullMetadataProvider CreateContextWithParserAndNullMetadataProvider()
    {
      var services = new ServiceCollection();
      _ = services.AddSingleton(_mockParser.Object);
      return new TestQueryStringParserModelBinderProviderContextWithNullMetadataProvider(services.BuildServiceProvider());
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestQueryStringParserModelBinderProviderContext : ModelBinderProviderContext
  {
    private readonly IServiceProvider? _services;
    private readonly List<Type> _requestedServiceTypes = [];
    private ModelMetadata? _metadata;
    private readonly TestQueryStringParserTrackingServiceProvider? _trackingServiceProvider;

    public TestQueryStringParserModelBinderProviderContext(IServiceProvider? services)
    {
      _services = services;
      if (_services is not null)
      {
        _trackingServiceProvider = new TestQueryStringParserTrackingServiceProvider(_services, _requestedServiceTypes);
      }
    }

    public override BindingInfo BindingInfo => new BindingInfo();

    public override ModelMetadata Metadata => _metadata ?? throw new InvalidOperationException("ModelMetadata not set. Call SetModelType<T>() first.");

    public override IModelMetadataProvider MetadataProvider => new TestQueryStringParserMetadataProvider();

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

    public override IModelBinder CreateBinder(ModelMetadata metadata) => new TestModelBinder(new Mock<IParseStrategy<TestModel>>().Object);

    private static DefaultModelMetadata CreateModelMetadata(Type type)
    {
      var identity = ModelMetadataIdentity.ForType(type);
      var attributes = ModelAttributes.GetAttributesForType(type);

      return new DefaultModelMetadata(
        new TestQueryStringParserMetadataProvider(),
        new TestQueryStringParserCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestQueryStringParserModelBinderProviderContextWithNullMetadataProvider : TestQueryStringParserModelBinderProviderContext
  {
    public TestQueryStringParserModelBinderProviderContextWithNullMetadataProvider(IServiceProvider? services) : base(services)
    {
    }

    public override IModelMetadataProvider MetadataProvider => null!;
  }

  [ExcludeFromCodeCoverage]
  public class TestQueryStringParserTrackingServiceProvider : IServiceProvider
  {
    private readonly IServiceProvider _innerProvider;
    private readonly ICollection<Type> _requestedTypes;

    public TestQueryStringParserTrackingServiceProvider(IServiceProvider innerProvider, ICollection<Type> requestedTypes)
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
  public class TestQueryStringParserMetadataProvider : IModelMetadataProvider
  {
    public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType) => [];

    public ModelMetadata GetMetadataForType(Type modelType)
    {
      var identity = ModelMetadataIdentity.ForType(modelType);
      var attributes = ModelAttributes.GetAttributesForType(modelType);

      return new DefaultModelMetadata(
        this,
        new TestQueryStringParserCompositeMetadataDetailsProvider(),
        new DefaultMetadataDetails(identity, attributes));
    }
  }

  [ExcludeFromCodeCoverage]
  public class TestQueryStringParserCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
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
  public class TestModel
  {
    public string Name { get; set; } = string.Empty;
  }

  [ExcludeFromCodeCoverage]
  public class DerivedTestModel : TestModel
  {
    public int Id { get; set; }
  }

  [ExcludeFromCodeCoverage]
  public class UnrelatedModel
  {
    public string Value { get; set; } = string.Empty;
  }

  [ExcludeFromCodeCoverage]
  public class DifferentModel
  {
    public int Number { get; set; }
  }

  [ExcludeFromCodeCoverage]
  public class TestModelBinder : QueryStringParserModelBinderBase<TestModel>
  {
    public TestModelBinder(IParseStrategy<TestModel> parser) : base(parser)
    {
    }

    public new IParseStrategy<TestModel> Parser => base.Parser;
  }

  [ExcludeFromCodeCoverage]
  public class AlternateModelBinder : QueryStringParserModelBinderBase<TestModel>
  {
    public AlternateModelBinder(IParseStrategy<TestModel> parser) : base(parser)
    {
    }
  }
}
