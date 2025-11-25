// -----------------------------------------------------------------------
// <copyright file="SortInfoModelBinderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class SortInfoModelBinderTests
  {
    private Mock<IParseStrategy<IEnumerable<SortInfo>>> _mockParser = null!;
    private SortInfoModelBinder _sut = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
      _mockParser = new Mock<IParseStrategy<IEnumerable<SortInfo>>>();
      _ = _mockParser.Setup((x) => x.ParameterKey).Returns("sort");
      _sut = new SortInfoModelBinder(_mockParser.Object);
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
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new SortInfoModelBinder(null!));

    [TestMethod]
    public void When_parser_is_valid_constructor_succeeds()
    {
      // Arrange
      var parser = new Mock<IParseStrategy<IEnumerable<SortInfo>>>();

      // Act
      var binder = new SortInfoModelBinder(parser.Object);

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_created_with_valid_parser_SortInfoModelBinder_implements_IModelBinder()
    {
      // Act
      var binder = new SortInfoModelBinder(_mockParser.Object);

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinder>(binder);
    }

    [TestMethod]
    public void When_bindingContext_is_null_BindModelAsync_throws_ArgumentNullException()
    {
      // Arrange
      var binder = new SortInfoModelBinder(_mockParser.Object);

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

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(ValueProviderResult.None);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_returns_null_BindModelAsync_returns_failed_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns((IEnumerable<SortInfo>)null!);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = ModelBindingResult.Failed(), Times.Once);
    }

    [TestMethod]
    public void When_parser_returns_valid_sortInfo_BindModelAsync_returns_success_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet), Times.Once);
    }

    [TestMethod]
    public void When_ModelName_is_null_BindModelAsync_uses_FieldName()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.FieldName).Returns("sortField");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue("sortField")).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("sortField"), Times.Once);
    }

    [TestMethod]
    public void When_valid_sort_string_provided_BindModelAsync_calls_parser_with_correct_input()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string sortString = "name,age:desc";
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(sortString));

      _ = _mockParser.Setup((x) => x.Parse(sortString)).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.Parse(sortString), Times.Once);
    }

    [TestMethod]
    public void When_ValueProviderResult_FirstValue_is_empty_string_BindModelAsync_calls_parser_with_empty_string()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(string.Empty));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.Parse(string.Empty), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_List_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(List<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is List<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_array_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(SortInfo[]));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is SortInfo[]), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_ICollection_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(ICollection<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is ICollection<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_complex_sort_string_provided_BindModelAsync_processes_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string complexSortString = "name,age:desc,status:asc";
      List<SortInfo> parsedSortInfo =
      [
        new SortInfo("Name", ListSortDirection.Ascending),
        new SortInfo("Age", ListSortDirection.Descending),
        new SortInfo("Status", ListSortDirection.Ascending)
      ];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(complexSortString));

      _ = _mockParser.Setup((x) => x.Parse(complexSortString)).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet), Times.Once);
      _mockParser.Verify((x) => x.Parse(complexSortString), Times.Once);
    }

    [TestMethod]
    public void When_BindModelAsync_completes_task_is_completed_successfully()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
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
      var binder1 = new SortInfoModelBinder(_mockParser.Object);
      var binder2 = new SortInfoModelBinder(_mockParser.Object);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_parser_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new SortInfoModelBinder(null!));
      Assert.AreEqual("parser", exception.ParamName);
    }

    [TestMethod]
    public void When_parser_returns_empty_collection_BindModelAsync_returns_success_with_empty_result()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> emptySortInfo = [];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(""));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(emptySortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is IEnumerable<SortInfo> && !((IEnumerable<SortInfo>)r.Model).Any()), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyCollection_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IReadOnlyCollection<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet && r.Model is IReadOnlyCollection<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_parser_is_called_Parse_method_is_invoked_not_TryParse()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      _mockParser.Verify((x) => x.Parse(It.IsAny<string>()), Times.Once);
      _mockParser.Verify((x) => x.TryParse(It.IsAny<string>(), out It.Ref<IEnumerable<SortInfo>?>.IsAny), Times.Never);
    }

    [TestMethod]
    public void When_ModelName_and_FieldName_are_both_null_BindModelAsync_uses_empty_string()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.FieldName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public void When_multiple_sort_fields_provided_BindModelAsync_parses_all_correctly()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      string multiSortString = "name:asc,age:desc,salary:asc";
      List<SortInfo> parsedSortInfo =
      [
        new SortInfo("Name", ListSortDirection.Ascending),
        new SortInfo("Age", ListSortDirection.Descending),
        new SortInfo("Salary", ListSortDirection.Ascending)
      ];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(multiSortString));

      _ = _mockParser.Setup((x) => x.Parse(multiSortString)).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 3), Times.Once);
    }

    [TestMethod]
    public void When_ConvertEnumerable_returns_null_BindModelAsync_sets_result_with_null_model()
    {
      // Arrange
      var mockContext = new Mock<ModelBindingContext>();
      var mockValueProvider = new Mock<IValueProvider>();
      List<SortInfo> parsedSortInfo = [new SortInfo("Name", ListSortDirection.Ascending)];

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(InvalidEnumerableType));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      _ = _mockParser.Setup((x) => x.Parse(It.IsAny<string>())).Returns(parsedSortInfo);

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) => r.IsModelSet), Times.Once);
    }
  }

  [ExcludeFromCodeCoverage]
  public class InvalidEnumerableType
  {
    public string Value { get; set; } = string.Empty;
  }
}
