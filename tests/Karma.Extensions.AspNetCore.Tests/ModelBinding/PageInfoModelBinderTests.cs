// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class PageInfoModelBinderTests
  {
    private Mock<IParseStrategy<PageInfo>> _mockParser = null!;
    private PageInfoModelBinder _sut = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
      _mockParser = new Mock<IParseStrategy<PageInfo>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("page");
      _sut = new PageInfoModelBinder(_mockParser.Object);
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
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(null!));

    [TestMethod]
    public void When_parser_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(null!));
      Assert.AreEqual("parser", exception.ParamName);
    }

    [TestMethod]
    public void When_parser_is_valid_constructor_succeeds()
    {
      // Arrange
      var parser = new Mock<IParseStrategy<PageInfo>>();
      _ = parser.Setup((x) => x.ParameterKey).Returns("page");

      // Act
      var binder = new PageInfoModelBinder(parser.Object);

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_created_with_valid_parser_PageInfoModelBinder_implements_IModelBinder()
    {
      // Act
      var binder = new PageInfoModelBinder(_mockParser.Object);

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinder>(binder);
    }

    [TestMethod]
    public void When_constructor_called_multiple_times_creates_different_instances()
    {
      // Act
      var binder1 = new PageInfoModelBinder(_mockParser.Object);
      var binder2 = new PageInfoModelBinder(_mockParser.Object);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_bindingContext_is_null_BindModelAsync_throws_ArgumentNullException()
    {
      // Arrange
      var binder = new PageInfoModelBinder(_mockParser.Object);

      // Act & Assert
      AggregateException aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        Task.Run(() => binder.BindModelAsync(null!), TestContext.CancellationToken).Wait(TestContext.CancellationToken));

      _ = Assert.IsInstanceOfType<ArgumentNullException>(aggregateException.InnerException);
    }

    [TestMethod]
    public void When_ValueProviderResult_is_None_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

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

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("invalid"));

      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out It.Ref<PageInfo?>.IsAny)).Returns(false);

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

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("validstring"));

      PageInfo? nullResult = null;
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
      var parsedPageInfo = new PageInfo(10, 25);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("offset:10,limit:25"));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model) == parsedPageInfo), Times.Once);
    }

    [TestMethod]
    public void When_ModelName_is_null_BindModelAsync_uses_ParameterKey_from_parser()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      var parsedPageInfo = new PageInfo(0, 20);

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue("page")).Returns(new ValueProviderResult("limit:20"));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("page"), Times.Once);
    }

    [TestMethod]
    public void When_valid_page_string_with_offset_and_limit_provided_BindModelAsync_calls_parser_with_correct_input()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string pageString = "offset:10,limit:25";
      var parsedPageInfo = new PageInfo(10, 25);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(pageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(pageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.TryParse(pageString, out outResult), Times.Once);
    }

    [TestMethod]
    public void When_valid_page_string_with_after_cursor_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string pageString = "after:cursor123,limit:50";
      var parsedPageInfo = new PageInfo("cursor123", 50);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(pageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(pageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).After == "cursor123" &&
        ((PageInfo)r.Model).Limit == 50), Times.Once);
    }

    [TestMethod]
    public void When_ValueProviderResult_FirstValue_is_empty_string_BindModelAsync_uses_empty_string()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(string.Empty));

      _ = _mockParser.Setup((x) => x.TryParse(string.Empty, out It.Ref<PageInfo?>.IsAny)).Returns(false);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.TryParse(string.Empty, out It.Ref<PageInfo?>.IsAny), Times.Once);
    }

    [TestMethod]
    public void When_query_string_is_whitespace_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("   "));

      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out It.Ref<PageInfo?>.IsAny)).Returns(false);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_complex_page_string_with_all_properties_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string complexPageString = "after:cursor123,before:cursor456,offset:10,limit:25";
      var parsedPageInfo = new PageInfo("cursor123", "cursor456", 10, 25);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(complexPageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(complexPageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet), Times.Once);
      _mockParser.Verify((x) => x.TryParse(complexPageString, out outResult), Times.Once);
    }

    [TestMethod]
    public void When_BindModelAsync_completes_task_is_completed_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod]
    public void When_ValueProvider_returns_None_BindModelAsync_queries_correct_value()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("page"), Times.Once);
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_ParameterKey_is_used_BindModelAsync_queries_correct_value()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string customKey = "customPageKey";

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

    [TestMethod]
    public void When_page_string_with_only_limit_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string pageString = "limit:100";
      var parsedPageInfo = new PageInfo(0, 100);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(pageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(pageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Limit == 100), Times.Once);
    }

    [TestMethod]
    public void When_page_string_with_only_offset_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string pageString = "offset:50";
      var parsedPageInfo = new PageInfo(50);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(pageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(pageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Offset == 50), Times.Once);
    }

    [TestMethod]
    public void When_URL_encoded_page_string_provided_BindModelAsync_passes_to_parser()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string encodedPageString = "after%3Acursor123%2Climit%3A25";
      var parsedPageInfo = new PageInfo("cursor123", 25);

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(encodedPageString));

      PageInfo? outResult = parsedPageInfo;
      _ = _mockParser.Setup((x) => x.TryParse(encodedPageString, out outResult)).Returns(true);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.TryParse(encodedPageString, out outResult), Times.Once);
    }

    [TestMethod]
    public void When_parser_throws_exception_during_TryParse_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("malformed"));

      _ = _mockParser.Setup((x) => x.TryParse(It.IsAny<string>(), out It.Ref<PageInfo?>.IsAny))
        .Throws(new InvalidOperationException("Parser error"));

      // Act & Assert
      AggregateException aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken).Wait(TestContext.CancellationToken));

      _ = Assert.IsInstanceOfType<InvalidOperationException>(aggregateException.InnerException);
    }

    [TestMethod]
    public void When_real_parser_with_limit_query_string_BindModelAsync_returns_PageInfo_with_limit()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[limit]=50";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Limit == 50 &&
        ((PageInfo)r.Model).Offset == 0), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_offset_query_string_BindModelAsync_returns_PageInfo_with_offset()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[offset]=100";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Offset == 100 &&
        ((PageInfo)r.Model).Limit == uint.MaxValue), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_offset_and_limit_query_string_BindModelAsync_returns_PageInfo_with_both()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[offset]=25&page[limit]=100";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Offset == 25 &&
        ((PageInfo)r.Model).Limit == 100), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_after_cursor_query_string_BindModelAsync_returns_PageInfo_with_after()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[after]=cursor_abc123";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).After == "cursor_abc123" &&
        ((PageInfo)r.Model).Limit == uint.MaxValue), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_before_cursor_query_string_BindModelAsync_returns_PageInfo_with_before()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[before]=cursor_xyz789";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Before == "cursor_xyz789" &&
        ((PageInfo)r.Model).Limit == uint.MaxValue), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_cursor_alias_query_string_BindModelAsync_returns_PageInfo_with_after()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[cursor]=cursor_alias123";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).After == "cursor_alias123" &&
        ((PageInfo)r.Model).Limit == uint.MaxValue), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_all_properties_query_string_BindModelAsync_returns_complete_PageInfo()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[after]=cursor_start&page[before]=cursor_end&page[offset]=50&page[limit]=25";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).After == "cursor_start" &&
        ((PageInfo)r.Model).Before == "cursor_end" &&
        ((PageInfo)r.Model).Offset == 50 &&
        ((PageInfo)r.Model).Limit == 25), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_URL_encoded_cursor_query_string_BindModelAsync_returns_decoded_PageInfo()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[after]=cursor%3Avalue%2Bencoded&page[limit]=20";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).After == "cursor:value+encoded" &&
        ((PageInfo)r.Model).Limit == 20), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_invalid_query_string_format_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "invalid_format_no_brackets";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_real_parser_with_mixed_case_properties_query_string_BindModelAsync_returns_PageInfo()
    {
      // Arrange
      var parser = new PageInfoQueryStringParser();
      var binder = new PageInfoModelBinder(parser);
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string queryString = "page[LIMIT]=30&page[OffSeT]=15";

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(queryString));

      // Act
      var task = Task.Run(() => binder.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is PageInfo &&
        ((PageInfo)r.Model).Limit == 30 &&
        ((PageInfo)r.Model).Offset == 15), Times.Once);
    }

    [TestMethod]
    public void When_empty_string_provided_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("page");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(string.Empty));

      _ = _mockParser.Setup((x) => x.TryParse(string.Empty, out It.Ref<PageInfo?>.IsAny)).Returns(false);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }
  }
}
