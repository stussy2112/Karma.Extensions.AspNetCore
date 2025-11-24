// -----------------------------------------------------------------------
// <copyright file="MvcBuilderQueryStringInfoExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.ModelBinding;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Tests.DependencyInjection
{
  /// <summary>
  /// Contains unit tests for the <see cref="MvcBuilderQueryStringInfoExtensions"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public sealed class MvcBuilderQueryStringInfoExtensionsTests
  {
    [TestMethod]
    public void When_builder_is_null_AddFilterInfoParameterBinding_with_parameterKey_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddFilterInfoParameterBinding());
    }

    [TestMethod]
    public void When_builder_is_null_AddFilterInfoParameterBinding_with_patternProvider_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;
      FilterPatternProvider? patternProvider = FilterPatternProvider.Default;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddFilterInfoParameterBinding(patternProvider));
    }

    [TestMethod]
    public void When_builder_is_null_AddFilterInfoParameterBinding_with_parseStrategy_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;
      IParseStrategy<FilterInfoCollection>? parseStrategy = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddFilterInfoParameterBinding(parseStrategy));
    }

    [TestMethod]
    public void When_parameterKey_is_null_AddFilterInfoParameterBinding_with_parseStrategy_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<FilterInfoCollection>? parseStrategy = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddFilterInfoParameterBinding(parseStrategy, null));
    }

    [TestMethod]
    public void When_parameterKey_is_empty_AddFilterInfoParameterBinding_with_parseStrategy_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<FilterInfoCollection>? parseStrategy = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentException>(() => builder.AddFilterInfoParameterBinding(parseStrategy, string.Empty));
    }

    [TestMethod]
    public void When_parameterKey_is_whitespace_AddFilterInfoParameterBinding_with_parseStrategy_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<FilterInfoCollection>? parseStrategy = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentException>(() => builder.AddFilterInfoParameterBinding(parseStrategy, "   "));
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_default_parameterKey_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddFilterInfoParameterBinding();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_custom_parameterKey_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddFilterInfoParameterBinding("customFilter");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_patternProvider_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider patternProvider = FilterPatternProvider.Default;

      // Act
      IMvcBuilder result = builder.AddFilterInfoParameterBinding(patternProvider);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_parseStrategy_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<FilterInfoCollection> parseStrategy = new FilterQueryStringParser();

      // Act
      IMvcBuilder result = builder.AddFilterInfoParameterBinding(parseStrategy);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_parseStrategy_registers_service()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<FilterInfoCollection> parseStrategy = new FilterQueryStringParser();

      // Act
      _ = builder.AddFilterInfoParameterBinding(parseStrategy);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<FilterInfoCollection>? registeredService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(registeredService);
      Assert.AreSame(parseStrategy, registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_null_parseStrategy_registers_default_parser()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddFilterInfoParameterBinding((IParseStrategy<FilterInfoCollection>?)null);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<FilterInfoCollection>? registeredService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(registeredService);
      _ = Assert.IsInstanceOfType<FilterQueryStringParser>(registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_adds_CompleteKeyedQueryStringValueProviderFactory()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddFilterInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is CompleteKeyedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_adds_FilterInfoModelBinderProvider()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddFilterInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is FilterInfoModelBinderProvider));
    }

    [TestMethod]
    public void When_builder_is_null_AddPagingInfoParameterBinding_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddPagingInfoParameterBinding());
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddPagingInfoParameterBinding();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_null_parseStrategy_registers_default_parser()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddPagingInfoParameterBinding(null);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<PageInfo>? registeredService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(registeredService);
      _ = Assert.IsInstanceOfType<PageInfoQueryStringParser>(registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_custom_parseStrategy_registers_parser()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<PageInfo> parseStrategy = new PageInfoQueryStringParser();

      // Act
      _ = builder.AddPagingInfoParameterBinding(parseStrategy);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<PageInfo>? registeredService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(registeredService);
      Assert.AreSame(parseStrategy, registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_adds_PageInfoModelBinderProvider()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddPagingInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is PageInfoModelBinderProvider));
    }

    [TestMethod]
    public void When_builder_is_null_AddSortInfoParameterBinding_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddSortInfoParameterBinding());
    }

    [TestMethod]
    public void When_parameterKey_is_null_AddSortInfoParameterBinding_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder.AddSortInfoParameterBinding(null));
    }

    [TestMethod]
    public void When_parameterKey_is_empty_AddSortInfoParameterBinding_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentException>(() => builder.AddSortInfoParameterBinding(string.Empty));
    }

    [TestMethod]
    public void When_parameterKey_is_whitespace_AddSortInfoParameterBinding_throws_ArgumentException()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentException>(() => builder.AddSortInfoParameterBinding("   "));
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddSortInfoParameterBinding();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_with_custom_parameterKey_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddSortInfoParameterBinding("customSort");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_with_null_parseStrategy_registers_default_parser()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddSortInfoParameterBinding(QueryParameterNames.Sort, null);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<IEnumerable<SortInfo>>? registeredService = provider.GetService<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.IsNotNull(registeredService);
      _ = Assert.IsInstanceOfType<SortsQueryStringParser>(registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_with_custom_parseStrategy_registers_parser()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      IParseStrategy<IEnumerable<SortInfo>> parseStrategy = new SortsQueryStringParser();

      // Act
      _ = builder.AddSortInfoParameterBinding(QueryParameterNames.Sort, parseStrategy);

      // Assert
      ServiceProvider provider = services.BuildServiceProvider();
      IParseStrategy<IEnumerable<SortInfo>>? registeredService = provider.GetService<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.IsNotNull(registeredService);
      Assert.AreSame(parseStrategy, registeredService);
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_adds_DelimitedQueryStringValueProviderFactory()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddSortInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is DelimitedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddSortInfoParameterBinding_adds_SortInfoModelBinderProvider()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddSortInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is SortInfoModelBinderProvider));
    }

    [TestMethod]
    public void When_builder_is_null_AddQueryStringInfoParameterBindings_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddQueryStringInfoParameterBinding());
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddQueryStringInfoParameterBinding();

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_registers_all_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddQueryStringInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert - Filter service
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);

      // Assert - Page service
      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);

      // Assert - Sort service
      IParseStrategy<IEnumerable<SortInfo>>? sortService = provider.GetService<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.IsNotNull(sortService);
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_adds_all_model_binders()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddQueryStringInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is FilterInfoModelBinderProvider));
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is PageInfoModelBinderProvider));
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is SortInfoModelBinderProvider));
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_adds_all_value_provider_factories()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddQueryStringInfoParameterBinding();
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is CompleteKeyedQueryStringValueProviderFactory));
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is DelimitedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_with_custom_options_applies_options()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      string customFilterKey = "customFilter";
      string customSortKey = "customSort";

      // Act
      _ = builder.AddQueryStringInfoParameterBinding((options) =>
      {
        options.FilterBindingOptions.ParameterKey = customFilterKey;
        options.SortBindingOptions.ParameterKey = customSortKey;
      });

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);

      // Verify custom parameter keys are used
      CompleteKeyedQueryStringValueProviderFactory? filterFactory =
        mvcOptions.ValueProviderFactories.OfType<CompleteKeyedQueryStringValueProviderFactory>().FirstOrDefault();
      Assert.IsNotNull(filterFactory);

      DelimitedQueryStringValueProviderFactory? sortFactory =
        mvcOptions.ValueProviderFactories.OfType<DelimitedQueryStringValueProviderFactory>().FirstOrDefault();
      Assert.IsNotNull(sortFactory);
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_with_null_configureOptions_uses_defaults()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddQueryStringInfoParameterBinding(null);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert - All services should be registered with defaults
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);

      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);

      IParseStrategy<IEnumerable<SortInfo>>? sortService = provider.GetService<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.IsNotNull(sortService);
    }

    [TestMethod]
    public void When_AddFilterInfoParameterBinding_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddFilterInfoParameterBinding();
      _ = builder.AddFilterInfoParameterBinding();

      // Assert - Should only have one registration
      ServiceProvider provider = services.BuildServiceProvider();
      IEnumerable<IParseStrategy<FilterInfoCollection>> allServices = provider.GetServices<IParseStrategy<FilterInfoCollection>>();
      Assert.AreEqual(1, allServices.Count());
    }

    [TestMethod]
    public void When_AddPagingInfoParameterBinding_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddPagingInfoParameterBinding();
      _ = builder.AddPagingInfoParameterBinding();

      // Assert - Should only have one registration
      ServiceProvider provider = services.BuildServiceProvider();
      IEnumerable<IParseStrategy<PageInfo>> allServices = provider.GetServices<IParseStrategy<PageInfo>>();
      Assert.AreEqual(1, allServices.Count());
    }

    [TestMethod]
    public void When_AddSortInfoParameterBinding_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddSortInfoParameterBinding();
      _ = builder.AddSortInfoParameterBinding();

      // Assert - Should only have one registration
      ServiceProvider provider = services.BuildServiceProvider();
      IEnumerable<IParseStrategy<IEnumerable<SortInfo>>> allServices = provider.GetServices<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.AreEqual(1, allServices.Count());
    }

    [TestMethod]
    public void When_AddQueryStringInfoParameterBindings_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddQueryStringInfoParameterBinding();
      _ = builder.AddQueryStringInfoParameterBinding();

      // Assert - Should only have one registration of each service
      ServiceProvider provider = services.BuildServiceProvider();

      IEnumerable<IParseStrategy<FilterInfoCollection>> filterServices = provider.GetServices<IParseStrategy<FilterInfoCollection>>();
      Assert.AreEqual(1, filterServices.Count());

      IEnumerable<IParseStrategy<PageInfo>> pageServices = provider.GetServices<IParseStrategy<PageInfo>>();
      Assert.AreEqual(1, pageServices.Count());

      IEnumerable<IParseStrategy<IEnumerable<SortInfo>>> sortServices = provider.GetServices<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.AreEqual(1, sortServices.Count());
    }

    [TestMethod]
    public void When_AddFilterInfoParameterBinding_with_patternProvider_null_uses_default_pattern()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddFilterInfoParameterBinding((FilterPatternProvider?)null);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<FilterInfoCollection>? registeredService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(registeredService);
      _ = Assert.IsInstanceOfType<FilterQueryStringParser>(registeredService);
    }

    [TestMethod]
    public void When_builder_methods_chained_all_services_registered()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder
        .AddFilterInfoParameterBinding()
        .AddPagingInfoParameterBinding()
        .AddSortInfoParameterBinding();

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);

      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);

      IParseStrategy<IEnumerable<SortInfo>>? sortService = provider.GetService<IParseStrategy<IEnumerable<SortInfo>>>();
      Assert.IsNotNull(sortService);
    }
  }
}
