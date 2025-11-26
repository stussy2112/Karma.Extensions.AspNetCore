// -----------------------------------------------------------------------
// <copyright file="FilterInfoModelBinderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class FilterInfoModelBinderTests
  {
    private Mock<IParseStrategy<FilterInfoCollection>> _mockParser = null!;
    private FilterInfoModelBinder _sut = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
      _mockParser = new Mock<IParseStrategy<FilterInfoCollection>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("filter");
      _sut = new FilterInfoModelBinder(_mockParser.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      _mockParser = null!;
      _sut = null!;
    }

    [TestMethod]
    public void When_parser_is_null_constructor_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new FilterInfoModelBinder(null!));

    [TestMethod]
    public void When_parser_is_valid_constructor_succeeds()
    {
      // Arrange
      var parser = new Mock<IParseStrategy<FilterInfoCollection>>();

      // Act
      var binder = new FilterInfoModelBinder(parser.Object);

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_created_with_valid_parser_FilterInfoModelBinder_implements_IModelBinder()
    {
      // Act
      var binder = new FilterInfoModelBinder(_mockParser.Object);

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinder>(binder);
    }

    [TestMethod]
    public void When_bindingContext_is_null_BindModelAsync_throws_ArgumentNullException()
    {
      // Arrange
      var binder = new FilterInfoModelBinder(_mockParser.Object);

      // Act & Assert
      AggregateException aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        Task.Run(() => binder.BindModelAsync(null!), TestContext.CancellationToken).Wait(TestContext.CancellationToken));

      _ = Assert.IsInstanceOfType<ArgumentNullException>(aggregateException.InnerException);
    }

    [TestMethod]
    public void When_query_string_is_empty_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_query_string_is_whitespace_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("   "));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_TryParse_returns_false_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("invalid"));

      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out It.Ref<FilterInfoCollection?>.IsAny)).Returns(false);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_TryParse_returns_true_but_result_is_null_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("validstring"));

      FilterInfoCollection? nullResult = null;
      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out nullResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_TryParse_succeeds_BindModelAsync_returns_success_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      var parsedCollection = new FilterInfoCollection("testFilter");

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name:eq:value"));

      FilterInfoCollection? outResult = parsedCollection;
      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is FilterInfoCollection && ((FilterInfoCollection)r.Model) == parsedCollection), Times.Once);
    }

    [TestMethod]
    public void When_ModelName_is_null_BindModelAsync_uses_ParameterKey_from_parser()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      var parsedCollection = new FilterInfoCollection("testFilter");

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue("filter")).Returns(new ValueProviderResult("name:eq:value"));

      FilterInfoCollection? outResult = parsedCollection;
      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("filter"), Times.Once);
    }

    [TestMethod]
    public void When_valid_filter_string_provided_BindModelAsync_calls_parser_with_correct_input()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string filterString = "name:eq:John";
      var parsedCollection = new FilterInfoCollection("testFilter");

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(filterString));

      FilterInfoCollection? outResult = parsedCollection;
      _ = _mockParser.Setup((x) => x.TryParse(filterString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.TryParse(filterString, out outResult), Times.Once);
    }

    [TestMethod]
    public void When_ValueProviderResult_FirstValue_is_null_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult((string)null!));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_complex_filter_string_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string complexFilterString = "name:eq:John,age:gt:25,status:in:active,pending";
      var parsedCollection = new FilterInfoCollection("complexFilter");

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(complexFilterString));

      FilterInfoCollection? outResult = parsedCollection;
      _ = _mockParser.Setup((x) => x.TryParse(complexFilterString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet), Times.Once);
      _mockParser.Verify((x) => x.TryParse(complexFilterString, out outResult), Times.Once);
    }

    [TestMethod]
    public void When_BindModelAsync_completes_task_is_completed_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod]
    public void When_constructor_called_multiple_times_creates_different_instances()
    {
      // Act
      var binder1 = new FilterInfoModelBinder(_mockParser.Object);
      var binder2 = new FilterInfoModelBinder(_mockParser.Object);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_parser_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new FilterInfoModelBinder(null!));
      Assert.AreEqual("parser", exception.ParamName);
    }

    [TestMethod]
    public void When_ValueProvider_returns_None_GetFilterQueryStringFromValueProvider_returns_empty_string()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("filter");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("filter"), Times.Once);
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_ParameterKey_is_used_BindModelAsync_queries_correct_value()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string customKey = "customFilterKey";

      _ = _mockParser.Setup((x) => x.ParameterKey).Returns(customKey);
      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(customKey)).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue(customKey), Times.Once);
    }
  }
}
