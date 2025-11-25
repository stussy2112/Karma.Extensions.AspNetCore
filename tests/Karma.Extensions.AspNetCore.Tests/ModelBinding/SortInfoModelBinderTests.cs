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
using Microsoft.Extensions.Primitives;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class SortInfoModelBinderTests
  {
    private SortInfoModelBinder _sut = null!;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize() => _sut = new SortInfoModelBinder();

    [TestCleanup]
    public void TestCleanup() => _sut = null!;

    [TestMethod]
    public void When_constructor_called_creates_valid_instance()
    {
      // Act
      SortInfoModelBinder binder = new();

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_created_SortInfoModelBinder_implements_IModelBinder()
    {
      // Act
      SortInfoModelBinder binder = new();

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinder>(binder);
    }

    [TestMethod]
    public void When_constructor_called_multiple_times_creates_different_instances()
    {
      // Act
      SortInfoModelBinder binder1 = new();
      SortInfoModelBinder binder2 = new();

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_bindingContext_is_null_BindModelAsync_throws_ArgumentNullException()
    {
      // Arrange
      SortInfoModelBinder binder = new();

      // Act & Assert
      AggregateException aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        Task.Run(() => binder.BindModelAsync(null!), TestContext.CancellationToken).Wait(TestContext.CancellationToken));

      _ = Assert.IsInstanceOfType<ArgumentNullException>(aggregateException.InnerException);
    }

    [TestMethod]
    public void When_ValueProviderResult_is_None_BindModelAsync_returns_failed_result()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

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
    public void When_single_field_provided_BindModelAsync_returns_success_with_single_sort()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).First().Direction == ListSortDirection.Ascending), Times.Once);
    }

    [TestMethod]
    public void When_field_with_descending_prefix_provided_BindModelAsync_returns_descending_sort()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("-name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).First().Direction == ListSortDirection.Descending), Times.Once);
    }

    [TestMethod]
    public void When_comma_separated_fields_provided_BindModelAsync_returns_multiple_sorts()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,age,email"));

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
    public void When_mixed_directions_provided_BindModelAsync_returns_correct_sorts()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,-age,email"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 3 &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(0).FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(0).Direction == ListSortDirection.Ascending &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(1).FieldName == "age" &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(1).Direction == ListSortDirection.Descending &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(2).FieldName == "email" &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(2).Direction == ListSortDirection.Ascending), Times.Once);
    }

    [TestMethod]
    public void When_multiple_StringValues_provided_BindModelAsync_processes_all_values()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();
      StringValues multipleValues = new(["name", "age", "email"]);

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(multipleValues));

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
    public void When_duplicate_fields_provided_BindModelAsync_returns_unique_sorts_only()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,name,name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "name"), Times.Once);
    }

    [TestMethod]
    public void When_duplicate_fields_different_directions_BindModelAsync_returns_first_occurrence()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,-name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).First().Direction == ListSortDirection.Ascending), Times.Once);
    }

    [TestMethod]
    public void When_empty_string_provided_BindModelAsync_returns_success_with_empty_result()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(string.Empty));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        !((IEnumerable<SortInfo>)r.Model).Any()), Times.Once);
    }

    [TestMethod]
    public void When_whitespace_only_provided_BindModelAsync_returns_success_with_empty_result()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("   \t\n   "));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        !((IEnumerable<SortInfo>)r.Model).Any()), Times.Once);
    }

    [TestMethod]
    public void When_invalid_field_provided_BindModelAsync_skips_invalid_field()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("-")); // Just a minus sign

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        !((IEnumerable<SortInfo>)r.Model).Any()), Times.Once);
    }

    [TestMethod]
    public void When_mixed_valid_and_invalid_fields_BindModelAsync_returns_only_valid_fields()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,-,age"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 2), Times.Once);
    }

    [TestMethod]
    public void When_ModelName_is_null_BindModelAsync_uses_FieldName()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.FieldName).Returns("sortField");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue("sortField")).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue("sortField"), Times.Once);
    }

    [TestMethod]
    public void When_ModelName_and_FieldName_are_both_null_BindModelAsync_uses_empty_string()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.FieldName).Returns((string)null!);
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockValueProvider.Verify((x) => x.GetValue(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_List_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(List<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is List<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_array_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(SortInfo[]));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is SortInfo[]), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_ICollection_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(ICollection<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is ICollection<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_ModelType_is_IReadOnlyCollection_of_SortInfo_BindModelAsync_converts_result_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IReadOnlyCollection<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IReadOnlyCollection<SortInfo>), Times.Once);
    }

    [TestMethod]
    public void When_fields_with_whitespace_provided_BindModelAsync_trims_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult(" name , age , email "));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 3 &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(0).FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(1).FieldName == "age" &&
        ((IEnumerable<SortInfo>)r.Model).ElementAt(2).FieldName == "email"), Times.Once);
    }

    [TestMethod]
    public void When_empty_fields_in_comma_list_BindModelAsync_skips_empty()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("name,,age,,,email"));

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
    public void When_BindModelAsync_completes_task_is_completed_successfully()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

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
    public void When_nested_field_path_provided_BindModelAsync_handles_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("user.profile.name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "user.profile.name"), Times.Once);
    }

    [TestMethod]
    public void When_multiple_descending_prefixes_provided_BindModelAsync_handles_correctly()
    {
      // Arrange
      Mock<ModelBindingContext> mockContext = new();
      Mock<IValueProvider> mockValueProvider = new();

      _ = mockContext.Setup((x) => x.ModelName).Returns("sort");
      _ = mockContext.Setup((x) => x.FieldName).Returns("sort");
      _ = mockContext.Setup((x) => x.ModelType).Returns(typeof(IEnumerable<SortInfo>));
      _ = mockContext.Setup((x) => x.ValueProvider).Returns(mockValueProvider.Object);
      _ = mockValueProvider.Setup((x) => x.GetValue(It.IsAny<string>())).Returns(new ValueProviderResult("--name"));

      // Act
      var task = Task.Run(() => _sut.BindModelAsync(mockContext.Object), TestContext.CancellationToken);
      task.GetAwaiter().GetResult();

      // Assert
      mockContext.VerifySet((x) => x.Result = It.Is<ModelBindingResult>((r) =>
        r.IsModelSet &&
        r.Model is IEnumerable<SortInfo> &&
        ((IEnumerable<SortInfo>)r.Model).Count() == 1 &&
        ((IEnumerable<SortInfo>)r.Model).First().FieldName == "name" &&
        ((IEnumerable<SortInfo>)r.Model).First().Direction == ListSortDirection.Descending), Times.Once);
    }
  }
}
