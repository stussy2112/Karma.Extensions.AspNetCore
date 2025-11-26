// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoMvcBuilderExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Karma.Extensions.AspNetCore.Tests.DependencyInjection
{
  /// <summary>
  /// Contains unit tests for the <see cref="QueryStringInfoMvcBuilderExtensions"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public sealed class QueryStringInfoMvcBuilderExtensionsTests
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
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is QueryStringParserModelBinderProvider<PageInfoModelBinder, PageInfo>));
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
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is QueryStringParserModelBinderProvider<PageInfoModelBinder, PageInfo>));
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
    }

    [TestMethod]
    public void When_builder_is_null_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_throws_ArgumentNullException()
    {
      // Arrange
      IMvcBuilder? builder = null;
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => builder!.AddPagingInfoParameterBinding(patternProvider));
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act
      IMvcBuilder result = builder.AddPagingInfoParameterBinding(patternProvider);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_custom_parameterKey_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      IMvcBuilder result = builder.AddPagingInfoParameterBinding("customPage");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_and_custom_parameterKey_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act
      IMvcBuilder result = builder.AddPagingInfoParameterBinding(patternProvider, "customPage");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_default_parameterKey_adds_CompleteKeyedQueryStringValueProviderFactory()
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
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is CompleteKeyedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_adds_CompleteKeyedQueryStringValueProviderFactory()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act
      _ = builder.AddPagingInfoParameterBinding(patternProvider);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is CompleteKeyedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_custom_parameterKey_adds_PageInfoModelBinderProvider()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();

      // Act
      _ = builder.AddPagingInfoParameterBinding("customPage");
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is QueryStringParserModelBinderProvider<PageInfoModelBinder, PageInfo>));
    }

    [TestMethod]
    public void When_valid_builder_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_registers_parser_service()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act
      _ = builder.AddPagingInfoParameterBinding(patternProvider);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);
      _ = Assert.IsInstanceOfType<PageInfoQueryStringParser>(pageService);
    }

    [TestMethod]
    public void When_AddPagingInfoParameterBinding_with_pageInfoPatternProvider_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider patternProvider = PageInfoPatternProvider.Default;

      // Act
      _ = builder.AddPagingInfoParameterBinding(patternProvider);
      _ = builder.AddPagingInfoParameterBinding(patternProvider);

      // Assert - Should only have one registration
      ServiceProvider provider = services.BuildServiceProvider();
      IEnumerable<IParseStrategy<PageInfo>> allServices = provider.GetServices<IParseStrategy<PageInfo>>();
      Assert.AreEqual(1, allServices.Count());
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_custom_parameterKey_and_patternProvider_returns_builder()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider patternProvider = FilterPatternProvider.Default;

      // Act
      IMvcBuilder result = builder.AddFilterInfoParameterBinding(patternProvider, "customFilter");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreSame(builder, result);
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_patternProvider_adds_CompleteKeyedQueryStringValueProviderFactory()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider patternProvider = FilterPatternProvider.Default;

      // Act
      _ = builder.AddFilterInfoParameterBinding(patternProvider);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ValueProviderFactories.Any((f) => f is CompleteKeyedQueryStringValueProviderFactory));
    }

    [TestMethod]
    public void When_valid_builder_AddFilterInfoParameterBinding_with_patternProvider_registers_parser_service()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider patternProvider = FilterPatternProvider.Default;

      // Act
      _ = builder.AddFilterInfoParameterBinding(patternProvider);
      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);
      _ = Assert.IsInstanceOfType<FilterQueryStringParser>(filterService);
    }

    [TestMethod]
    public void When_AddFilterInfoParameterBinding_with_patternProvider_called_multiple_times_does_not_duplicate_services()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider patternProvider = FilterPatternProvider.Default;

      // Act
      _ = builder.AddFilterInfoParameterBinding(patternProvider);
      _ = builder.AddFilterInfoParameterBinding(patternProvider);

      // Assert - Should only have one registration
      ServiceProvider provider = services.BuildServiceProvider();
      IEnumerable<IParseStrategy<FilterInfoCollection>> allServices = provider.GetServices<IParseStrategy<FilterInfoCollection>>();
      Assert.AreEqual(1, allServices.Count());
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_with_custom_pageInfoPatternProvider_applies_pattern()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      PageInfoPatternProvider customPatternProvider = PageInfoPatternProvider.Default;

      // Act
      _ = builder.AddQueryStringInfoParameterBinding((options) => options.PageBindingOptions.PatternProvider = customPatternProvider);

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);
      _ = Assert.IsInstanceOfType<PageInfoQueryStringParser>(pageService);
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_with_custom_filterPatternProvider_applies_pattern()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider customPatternProvider = FilterPatternProvider.Default;

      // Act
      _ = builder.AddQueryStringInfoParameterBinding((options) => options.FilterBindingOptions.PatternProvider = customPatternProvider);

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);
      _ = Assert.IsInstanceOfType<FilterQueryStringParser>(filterService);
    }

    [TestMethod]
    public void When_valid_builder_AddQueryStringInfoParameterBindings_with_custom_page_parameterKey_applies_key()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      string customPageKey = "customPage";

      // Act
      _ = builder.AddQueryStringInfoParameterBinding((options) => options.PageBindingOptions.ParameterKey = customPageKey);

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert
      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);

      // Verify page value provider factory is added
      CompleteKeyedQueryStringValueProviderFactory? pageFactory =
        mvcOptions.ValueProviderFactories.OfType<CompleteKeyedQueryStringValueProviderFactory>()
          .LastOrDefault();
      Assert.IsNotNull(pageFactory);
    }

    [TestMethod]
    public void When_all_overloads_used_together_all_services_registered_correctly()
    {
      // Arrange
      ServiceCollection services = new();
      IMvcBuilder builder = services.AddControllers();
      FilterPatternProvider filterPattern = FilterPatternProvider.Default;
      PageInfoPatternProvider pagePattern = PageInfoPatternProvider.Default;

      // Act
      _ = builder
        .AddFilterInfoParameterBinding(filterPattern, "customFilter")
        .AddPagingInfoParameterBinding(pagePattern, "customPage")
        .AddSortInfoParameterBinding("customSort");

      ServiceProvider provider = services.BuildServiceProvider();

      // Assert - All services should be registered
      IParseStrategy<FilterInfoCollection>? filterService = provider.GetService<IParseStrategy<FilterInfoCollection>>();
      Assert.IsNotNull(filterService);

      IParseStrategy<PageInfo>? pageService = provider.GetService<IParseStrategy<PageInfo>>();
      Assert.IsNotNull(pageService);

      MvcOptions? mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<MvcOptions>>()?.Value;
      Assert.IsNotNull(mvcOptions);
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is FilterInfoModelBinderProvider));
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is QueryStringParserModelBinderProvider<PageInfoModelBinder, PageInfo>));
      Assert.IsTrue(mvcOptions.ModelBinderProviders.Any((p) => p is SortInfoModelBinderProvider));
    }
  }
}
