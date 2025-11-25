// -----------------------------------------------------------------------
// <copyright file="CompleteKeyedQueryStringValueProviderFactoryTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class CompleteKeyedQueryStringValueProviderFactoryTests
  {
    [TestMethod]
    public void When_parameterKey_is_valid_Constructor_creates_instance_successfully()
    {
      // Arrange & Act
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");

      // Assert
      Assert.IsNotNull(factory);
    }

    [TestMethod]
    public void When_parameterKey_is_null_Constructor_throws_ArgumentNullException() =>
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new CompleteKeyedQueryStringValueProviderFactory(null!));

    [TestMethod]
    public void When_parameterKey_is_empty_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new CompleteKeyedQueryStringValueProviderFactory(string.Empty));

    [TestMethod]
    public void When_parameterKey_is_whitespace_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new CompleteKeyedQueryStringValueProviderFactory("   "));

    [TestMethod]
    public void When_context_is_null_CreateValueProviderAsync_throws_ArgumentNullException()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateValueProviderAsync(null!).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void When_context_HttpContext_is_null_CreateValueProviderAsync_throws_ArgumentNullException()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var actionContext = new ActionContext();
      var context = new ValueProviderFactoryContext(actionContext);

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateValueProviderAsync(context).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void When_context_HttpContext_Request_is_null_CreateValueProviderAsync_throws_ArgumentNullException()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var mockHttpContext = new Mock<HttpContext>();
      _ = mockHttpContext.SetupGet((x) => x.Request).Returns((HttpRequest)null!);
      var actionContext = new ActionContext { HttpContext = mockHttpContext.Object };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateValueProviderAsync(context).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void When_context_HttpContext_Request_Query_is_null_CreateValueProviderAsync_throws_ArgumentNullException()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");

      var mockHttpRequest = new Mock<HttpRequest>();
      _ = mockHttpRequest.SetupGet((x) => x.Query).Returns((IQueryCollection)null!);
      var mockHttpContext = new Mock<HttpContext>();
      _ = mockHttpContext.SetupGet((x) => x.Request).Returns(mockHttpRequest.Object);

      var actionContext = new ActionContext { HttpContext = mockHttpContext.Object };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => factory.CreateValueProviderAsync(context).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void When_valid_context_CreateValueProviderAsync_adds_provider_to_context()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[name]"] = new StringValues("john"),
        ["filter[age]"] = new StringValues("25")
      });
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
    }

    [TestMethod]
    public void When_valid_context_CreateValueProviderAsync_inserts_provider_at_beginning()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection();
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Add a dummy provider first
      context.ValueProviders.Add(new TestValueProvider());

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(2, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
      _ = Assert.IsInstanceOfType<TestValueProvider>(context.ValueProviders[1]);
    }

    [TestMethod]
    public void When_valid_context_CreateValueProviderAsync_inserts_at_index_zero()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection();
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Add multiple dummy providers
      context.ValueProviders.Add(new TestValueProvider());
      context.ValueProviders.Add(new TestValueProvider());
      context.ValueProviders.Add(new TestValueProvider());

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(4, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
      _ = Assert.IsInstanceOfType<TestValueProvider>(context.ValueProviders[1]);
      _ = Assert.IsInstanceOfType<TestValueProvider>(context.ValueProviders[2]);
      _ = Assert.IsInstanceOfType<TestValueProvider>(context.ValueProviders[3]);
    }

    [TestMethod]
    public void When_valid_context_CreateValueProviderAsync_returns_completed_task()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection();
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);

      // Assert
      Assert.IsTrue(task.IsCompletedSuccessfully);
      Assert.AreEqual(Task.CompletedTask, task);
    }

    [TestMethod]
    public void When_created_provider_has_correct_parameter_key_GetValue_works_correctly()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[name]"] = new StringValues("john"),
        ["filter[age]"] = new StringValues("25"),
        ["other"] = new StringValues("ignore")
      });
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      var provider = context.ValueProviders[0] as CompleteKeyedQueryStringValueProvider;
      Assert.IsNotNull(provider);

      ValueProviderResult result = provider.GetValue("filter");
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[name]=john", resultValue);
      Assert.Contains("filter[age]=25", resultValue);
      Assert.DoesNotContain("other=ignore", resultValue);
    }

    [TestMethod]
    public void When_created_provider_receives_non_matching_key_GetValue_returns_None()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["sort[name]"] = new StringValues("asc")
      });
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      var provider = context.ValueProviders[0] as CompleteKeyedQueryStringValueProvider;
      Assert.IsNotNull(provider);

      ValueProviderResult result = provider.GetValue("sort");
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    [DataRow("normalKey")]
    [DataRow("key-with-dashes")]
    [DataRow("key_with_underscores")]
    [DataRow("KeyWithCaps")]
    [DataRow("key.with.dots")]
    [DataRow("key@with!special#chars")]
    public void When_various_parameter_keys_provided_Constructor_creates_instance_successfully(string? parameterKey)
    {
      // Arrange & Act
      var factory = new CompleteKeyedQueryStringValueProviderFactory(parameterKey!);

      // Assert
      Assert.IsNotNull(factory);
    }

    [TestMethod]
    public void When_large_query_collection_CreateValueProviderAsync_performs_efficiently()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("filter");
      var queryDictionary = new Dictionary<string, StringValues>();

      // Add 1000 query parameters
      for (int i = 0; i < 1000; i++)
      {
        queryDictionary[$"param{i}"] = new StringValues($"value{i}");
      }

      var queryCollection = new QueryCollection(queryDictionary);
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      DateTime startTime = DateTime.UtcNow;
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();
      TimeSpan duration = DateTime.UtcNow - startTime;

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
      Assert.IsLessThan(100, duration.TotalMilliseconds, $"Operation took {duration.TotalMilliseconds}ms, expected < 100ms");
    }

    [TestMethod]
    public void When_parameterKey_contains_unicode_characters_CreateValueProviderAsync_creates_provider_successfully()
    {
      // Arrange
      var factory = new CompleteKeyedQueryStringValueProviderFactory("фильтр");
      var queryCollection = new QueryCollection();
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
    }

    [TestMethod]
    public void When_parameterKey_is_very_long_CreateValueProviderAsync_creates_provider_successfully()
    {
      // Arrange
      string longKey = new string('a', 1000); // 1000 character key
      var factory = new CompleteKeyedQueryStringValueProviderFactory(longKey);
      var queryCollection = new QueryCollection();
      var httpContext = new DefaultHttpContext();
      httpContext.Request.Query = queryCollection;
      var actionContext = new ActionContext { HttpContext = httpContext };
      var context = new ValueProviderFactoryContext(actionContext);

      // Act
      Task task = factory.CreateValueProviderAsync(context);
      task.GetAwaiter().GetResult();

      // Assert
      Assert.HasCount(1, context.ValueProviders);
      _ = Assert.IsInstanceOfType<CompleteKeyedQueryStringValueProvider>(context.ValueProviders[0]);
    }
  }
}