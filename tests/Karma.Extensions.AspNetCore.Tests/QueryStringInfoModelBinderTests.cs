// -----------------------------------------------------------------------
// <copyright file="QueryStringInfoModelBinderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class QueryStringInfoModelBinderTests
  {
    // When no value is present in the value provider, the binder must set ModelBindingResult to failed.
    [TestMethod]
    public void When_ValueProviderReturnsNone_BindModelAsync_Sets_Failed()
    {
      // Arrange
      var parser = new FakeParseStrategy<string>((input) => (true, "ignored"));
      var binder = new QueryStringInfoModelBinder<FakeParseStrategy<string>, string>(parser);

      var valueProvider = new FakeValueProvider(ValueProviderResult.None);
      var bindingContext = new DefaultModelBindingContext
      {
        ModelName = "q",
        ValueProvider = valueProvider
      };

      // Act
      binder.BindModelAsync(bindingContext).GetAwaiter().GetResult();

      // Assert
      Assert.IsFalse(bindingContext.Result.IsModelSet);
      Assert.IsNull(bindingContext.Result.Model);
    }

    // When parsing fails (TryParse returns false) the binder must set ModelBindingResult to failed.
    [TestMethod]
    public void When_TryParseReturnsFalse_BindModelAsync_Sets_Failed()
    {
      // Arrange
      var parser = new FakeParseStrategy<string>((input) => (false, null));
      var binder = new QueryStringInfoModelBinder<FakeParseStrategy<string>, string>(parser);

      var valueProvider = new FakeValueProvider(new ValueProviderResult("some-input"));
      var bindingContext = new DefaultModelBindingContext
      {
        ModelName = "q",
        ValueProvider = valueProvider
      };

      // Act
      binder.BindModelAsync(bindingContext).GetAwaiter().GetResult();

      // Assert
      Assert.IsFalse(bindingContext.Result.IsModelSet);
      Assert.IsNull(bindingContext.Result.Model);
    }

    // When TryParse returns true but the parsed value is null, binder must treat it as failure.
    [TestMethod]
    public void When_TryParseReturnsTrue_But_ParsedIsNull_BindModelAsync_Sets_Failed()
    {
      // Arrange
      var parser = new FakeParseStrategy<string>((input) => (true, null));
      var binder = new QueryStringInfoModelBinder<FakeParseStrategy<string>, string>(parser);

      var valueProvider = new FakeValueProvider(new ValueProviderResult("some-input"));
      var bindingContext = new DefaultModelBindingContext
      {
        ModelName = "q",
        ValueProvider = valueProvider
      };

      // Act
      binder.BindModelAsync(bindingContext).GetAwaiter().GetResult();

      // Assert
      Assert.IsFalse(bindingContext.Result.IsModelSet);
      Assert.IsNull(bindingContext.Result.Model);
    }

    // When TryParse succeeds and returns a non-null parsed value, binder must set ModelBindingResult to success.
    [TestMethod]
    public void When_TryParseSucceeds_BindModelAsync_Sets_Success_With_ParsedModel()
    {
      // Arrange
      string parsedExpected = "parsed-value";
      var parser = new FakeParseStrategy<string>((input) => (true, parsedExpected));
      var binder = new QueryStringInfoModelBinder<FakeParseStrategy<string>, string>(parser);

      var valueProvider = new FakeValueProvider(new ValueProviderResult("input-value"));
      var bindingContext = new DefaultModelBindingContext
      {
        ModelName = "q",
        ValueProvider = valueProvider
      };

      // Act
      binder.BindModelAsync(bindingContext).GetAwaiter().GetResult();

      // Assert
      Assert.IsTrue(bindingContext.Result.IsModelSet);
      Assert.IsNotNull(bindingContext.Result.Model);
      Assert.IsInstanceOfType(bindingContext.Result.Model, typeof(string));
      Assert.AreEqual(parsedExpected, (string)bindingContext.Result.Model);
    }

    // When FirstValue is null the binder should pass string.Empty to the parser — verify success path.
    [TestMethod]
    public void When_FirstValueIsNull_Binder_Passes_EmptyString_To_Parser()
    {
      // Arrange
      string? capturedInput = null;
      var parser = new FakeParseStrategy<string>((input) =>
      {
        capturedInput = input;
        return (true, "ok");
      });

      var binder = new QueryStringInfoModelBinder<FakeParseStrategy<string>, string>(parser);

      // ValueProviderResult constructed with a single null entry (not ValueProviderResult.None)
      var valueProvider = new FakeValueProvider(new ValueProviderResult(new string?[] { null }));
      var bindingContext = new DefaultModelBindingContext
      {
        ModelName = "q",
        ValueProvider = valueProvider
      };

      // Act
      binder.BindModelAsync(bindingContext).GetAwaiter().GetResult();

      // Assert
      Assert.IsTrue(bindingContext.Result.IsModelSet);
      Assert.AreEqual(string.Empty, capturedInput);
    }

    // --- Test helpers (dummy implementations used to isolate the binder) ---

    [ExcludeFromCodeCoverage]
    private sealed class FakeValueProvider : IValueProvider
    {
      private readonly ValueProviderResult _result;

      public FakeValueProvider(ValueProviderResult result) => _result = result;

      public bool ContainsPrefix(string prefix) => false;

      public ValueProviderResult GetValue(string key) => _result;
    }

    [ExcludeFromCodeCoverage]
    private sealed class FakeParseStrategy<T> : IParseStrategy<T>
    {
      private readonly System.Func<string, (bool success, T? parsed)> _behavior;

      public FakeParseStrategy(System.Func<string, (bool success, T? parsed)> behavior) => _behavior = behavior;

      public string ParameterKey => "fake";

      object? IParseStrategy.Parse(string input)
      {
        (bool _, T? parsed) = _behavior(input);
        return parsed;
      }

      public T? Parse(string input)
      {
        (bool _, T? parsed) = _behavior(input);
        return parsed;
      }

      public bool TryParse(string input, out object? parsed)
      {
        (bool success, T? p) = _behavior(input);
        parsed = p;
        return success;
      }

      public bool TryParse(string input, out T? parsed)
      {
        (bool success, T? p) = _behavior(input);
        parsed = p;
        return success;
      }
    }
  }
}