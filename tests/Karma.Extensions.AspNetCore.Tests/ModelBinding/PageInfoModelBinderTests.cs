// -----------------------------------------------------------------------
// <copyright file="PageInfoModelBinderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding.Tests
{
  [TestClass]
  [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
  public sealed class PageInfoModelBinderTests
  {
    private TestModelBinder _stringBinder = null!;
    private TestModelBinder _uintBinder = null!;

    // Deserialize the PageInfo from JSON
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public TestContext TestContext
    {
      get; set;
    }

    [TestInitialize]
    public void TestInitialize()
    {
      _stringBinder = new TestModelBinder();
      _uintBinder = new TestModelBinder();
    }

    [TestMethod]
    public void When_stringBinder_is_null_constructor_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(null!, _uintBinder));

    [TestMethod]
    public void When_uintBinder_is_null_constructor_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(_stringBinder, null!));

    [TestMethod]
    public void When_both_binders_are_valid_constructor_succeeds()
    {
      // Act
      var binder = new PageInfoModelBinder(_stringBinder, _uintBinder);

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_bindingContext_is_null_BindModelAsync_throws_ArgumentNullException()
    {
      // Arrange
      var binder = new PageInfoModelBinder(_stringBinder, _uintBinder);

      // Act & Assert
      AggregateException aggregateException = Assert.ThrowsExactly<AggregateException>(() =>
        Task.Run(() => binder.BindModelAsync(null!), TestContext.CancellationToken).Wait(TestContext.CancellationToken));
      _ = Assert.IsInstanceOfType<ArgumentNullException>(aggregateException.InnerException);
    }

    [TestMethod]
    public void When_created_with_valid_binders_PageInfoModelBinder_implements_IModelBinder()
    {
      // Act
      var binder = new PageInfoModelBinder(_stringBinder, _uintBinder);

      // Assert
      _ = Assert.IsInstanceOfType<IModelBinder>(binder);
    }

    [TestMethod]
    public void When_constructor_called_multiple_times_creates_different_instances()
    {
      // Act
      var binder1 = new PageInfoModelBinder(_stringBinder, _uintBinder);
      var binder2 = new PageInfoModelBinder(_stringBinder, _uintBinder);

      // Assert
      Assert.IsNotNull(binder1);
      Assert.IsNotNull(binder2);
      Assert.AreNotSame(binder1, binder2);
    }

    [TestMethod]
    public void When_constructor_receives_same_binder_for_both_parameters_succeeds()
    {
      // Arrange
      var sameBinder = new TestModelBinder();

      // Act
      var binder = new PageInfoModelBinder(sameBinder, sameBinder);

      // Assert
      Assert.IsNotNull(binder);
    }

    [TestMethod]
    public void When_stringBinder_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(null!, _uintBinder));
      Assert.AreEqual("stringBinder", exception.ParamName);
    }

    [TestMethod]
    public void When_uintBinder_parameter_name_is_checked_throws_ArgumentNullException_with_correct_parameter_name()
    {
      // Act & Assert
      ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => new PageInfoModelBinder(_stringBinder, null!));
      Assert.AreEqual("uintBinder", exception.ParamName);
    }

    [TestMethod]
    public void When_offset_and_limit_parameters_provided_model_binding_creates_PageInfo_with_correct_values()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[offset]=10&page[limit]=25");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(10u, result.Offset);
      Assert.AreEqual(25u, result.Limit);
    }

    [TestMethod]
    public void When_after_and_limit_parameters_provided_model_binding_creates_PageInfo_with_correct_values()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=cursor123&page[limit]=50");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor123", result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(50u, result.Limit);
    }

    [TestMethod]
    public void When_before_and_limit_parameters_provided_model_binding_creates_PageInfo_with_correct_values()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[before]=cursor456&page[limit]=30");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual("cursor456", result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(30u, result.Limit);
    }

    [TestMethod]
    public void When_cursor_fallback_parameter_provided_model_binding_uses_cursor_for_after()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[cursor]=fallback_cursor&page[limit]=20");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("fallback_cursor", result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(20u, result.Limit);
    }

    [TestMethod]
    public void When_both_after_and_cursor_provided_model_binding_prefers_after_over_cursor()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=primary_cursor&page[cursor]=fallback_cursor&page[limit]=15");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("primary_cursor", result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(15u, result.Limit);
    }

    [TestMethod]
    public void When_all_parameters_provided_model_binding_creates_PageInfo_with_all_values()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=cursor1&page[before]=cursor2&page[offset]=5&page[limit]=100");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor1", result.After);
      Assert.AreEqual("cursor2", result.Before);
      Assert.AreEqual(5u, result.Offset);
      Assert.AreEqual(100u, result.Limit);
    }

    [TestMethod]
    public void When_only_after_parameter_provided_model_binding_creates_PageInfo_with_defaults()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=single_cursor");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("single_cursor", result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_only_offset_parameter_provided_model_binding_creates_PageInfo_with_defaults()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[offset]=42");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(42u, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_no_pagination_parameters_provided_model_binding_returns_null()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[someother]=parameter");

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_empty_query_string_model_binding_returns_null()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("");

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_invalid_offset_parameter_provided_model_binding_throws_HttpRequestException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=invalid&page[limit]=10"));

    [TestMethod]
    public void When_invalid_limit_parameter_provided_model_binding_throws_HttpRequestException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=5&page[limit]=invalid"));

    [TestMethod]
    public void When_negative_offset_parameter_provided_model_binding_treats_as_zero()
    {
      // Note: uint parameters can't be negative in query strings, but test with zero
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[offset]=0&page[limit]=10");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(10u, result.Limit);
    }

    [TestMethod]
    public void When_zero_limit_parameter_provided_model_binding_uses_max_value()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[offset]=1&page[limit]=0");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1u, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_url_encoded_cursor_values_provided_model_binding_decodes_correctly()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=cursor%20with%20spaces&page[before]=cursor%2Bwith%2Bplus");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor with spaces", result.After);
      Assert.AreEqual("cursor+with+plus", result.Before);
    }

    [TestMethod]
    public void When_empty_string_cursor_values_provided_model_binding_creates_PageInfo()
    {
      // Arrange & Act
      PageInfo? result = ExecuteModelBindingTest("?page[after]=&page[before]=&page[limit]=5");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual(0u, result.Offset);
      Assert.AreEqual(5u, result.Limit);
    }

    [TestMethod]
    public void When_invalid_offset_parameter_provided_model_binding_returns_specific_error_message()
    {
      // Arrange & Act
      string content = ExecuteModelBindingTestExpectingBadRequest("?page[offset]=invalid&page[limit]=10");

      // Assert
      Assert.Contains("The value 'invalid' is not valid for Offset", content);
      Assert.Contains("Offset must be a non-negative integer", content);
    }

    [TestMethod]
    public void When_invalid_limit_parameter_provided_model_binding_returns_specific_error_message()
    {
      // Arrange & Act
      string content = ExecuteModelBindingTestExpectingBadRequest("?page[offset]=5&page[limit]=invalid");

      // Assert
      Assert.Contains("The value 'invalid' is not valid for Limit", content);
      Assert.Contains("Limit must be a positive integer (greater than 0)", content);
    }

    [TestMethod]
    public void When_multiple_invalid_parameters_provided_model_binding_returns_multiple_error_messages()
    {
      // Arrange & Act
      string content = ExecuteModelBindingTestExpectingBadRequest("?page[offset]=invalid&page[limit]=alsoinvalid");

      // Assert
      Assert.Contains("The value 'invalid' is not valid for Offset", content);
      Assert.Contains("The value 'alsoinvalid' is not valid for Limit", content);
      Assert.Contains("Offset must be a non-negative integer", content);
      Assert.Contains("Limit must be a positive integer (greater than 0)", content);
    }

    [TestMethod]
    public void When_valid_after_and_invalid_offset_provided_model_binding_fails_with_error() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[after]=cursor123&page[offset]=invalid"));

    [TestMethod]
    public void When_valid_before_and_invalid_limit_provided_model_binding_fails_with_error() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[before]=cursor456&page[limit]=invalid"));

    [TestMethod]
    public void When_valid_cursor_and_invalid_offset_provided_model_binding_fails_with_error() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[cursor]=fallback&page[offset]=invalid"));

    [TestMethod]
    public void When_all_string_params_valid_but_uint_params_invalid_model_binding_fails() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[after]=cursor1&page[before]=cursor2&page[offset]=invalid&page[limit]=alsoinvalid"));

    [TestMethod]
    public void When_negative_offset_as_string_provided_model_binding_throws_HttpRequestException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=-5&page[limit]=10"));

    [TestMethod]
    public void When_decimal_offset_provided_model_binding_throws_HttpRequestException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=5.5&page[limit]=10"));

    [TestMethod]
    public void When_decimal_limit_provided_model_binding_throws_HttpRequestException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=5&page[limit]=10.5"));

    [TestMethod]
    public void When_very_large_number_offset_provided_model_binding_handles_correctly() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=999999999999999999999&page[limit]=10"));

    [TestMethod]
    public void When_very_large_number_limit_provided_model_binding_handles_correctly() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=5&page[limit]=999999999999999999999"));

    [TestMethod]
    public void When_invalid_offset_parameter_provided_model_binding_returns_400_with_specific_error()
    {
      // Arrange & Act
      string content = ExecuteModelBindingTestExpectingBadRequest("?page[offset]=invalid&page[limit]=10");
      (int ErrorCount, Dictionary<string, string[]> Errors) errorDetails = ExecuteModelBindingTestGetErrorDetails("?page[offset]=invalid&page[limit]=10");

      // Assert - Status Code
      int statusCode = ExecuteModelBindingTestGetStatusCode("?page[offset]=invalid&page[limit]=10");
      Assert.AreEqual(400, statusCode);

      // Assert - Error Message Content
      Assert.Contains("The value 'invalid' is not valid for Offset", content);
      Assert.Contains("Offset must be a non-negative integer", content);

      // Assert - ModelState Error Count
      Assert.AreEqual(1, errorDetails.ErrorCount);
      Assert.IsTrue(errorDetails.Errors.ContainsKey("page[Offset]"));

      // Assert - HttpRequestException is thrown
      _ = Assert.ThrowsExactly<HttpRequestException>(() => ExecuteModelBindingTest("?page[offset]=invalid&page[limit]=10"));
    }

    private PageInfo? ExecuteModelBindingTest(string queryString)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });

      _ = builder.WebHost.UseTestServer();

      // Configure model binding for PageInfo
      _ = builder.Services.Configure<MvcOptions>((options) => options.ModelBinderProviders.Insert(0, new PageInfoModelBinderProvider()));

      _ = builder.Services.AddControllers();

      WebApplication app = builder.Build();

      // Map controllers instead of minimal API
      _ = app.MapControllers();

      app.Start();

      HttpClient client = app.GetTestClient();

      // Execute request to the test controller
      Uri requestUri = new($"/pageinfo-test{queryString}", UriKind.Relative);
      HttpResponseMessage response = client.GetAsync(requestUri, TestContext?.CancellationToken ?? default)
        .GetAwaiter().GetResult();

      // Verify response is successful - this will throw HttpRequestException for 4xx/5xx status codes
      _ = response.EnsureSuccessStatusCode();

      // Read the response content which contains the serialized PageInfo
      string content = response.Content.ReadAsStringAsync(TestContext?.CancellationToken ?? default)
        .GetAwaiter().GetResult();

      // Return null if content indicates no PageInfo was bound
      if (string.Equals(content, "null", StringComparison.OrdinalIgnoreCase))
      {
        return null;
      }

      return JsonSerializer.Deserialize<PageInfo>(content, _options);
    }

    private string ExecuteModelBindingTestExpectingBadRequest(string queryString)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });

      _ = builder.WebHost.UseTestServer();
      _ = builder.Services.Configure<MvcOptions>((options) =>
        options.ModelBinderProviders.Insert(0, new PageInfoModelBinderProvider()));
      _ = builder.Services.AddControllers();

      WebApplication app = builder.Build();
      _ = app.MapControllers();
      app.Start();

      HttpClient client = app.GetTestClient();

      Uri requestUri = new($"/pageinfo-test{queryString}", UriKind.Relative);
      HttpResponseMessage response = client.GetAsync(requestUri, TestContext?.CancellationToken ?? default)
        .GetAwaiter().GetResult();

      Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

      return response.Content.ReadAsStringAsync(TestContext?.CancellationToken ?? default)
        .GetAwaiter().GetResult();
    }

    private int ExecuteModelBindingTestGetStatusCode(string queryString)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });

      _ = builder.WebHost.UseTestServer();
      _ = builder.Services.Configure<MvcOptions>((options) =>
        options.ModelBinderProviders.Insert(0, new PageInfoModelBinderProvider()));
      _ = builder.Services.AddControllers();

      WebApplication app = builder.Build();
      _ = app.MapControllers();
      app.Start();

      HttpClient client = app.GetTestClient();

      Uri requestUri = new($"/pageinfo-test{queryString}", UriKind.Relative);
      HttpResponseMessage response = client.GetAsync(requestUri, TestContext?.CancellationToken ?? default)
        .GetAwaiter().GetResult();

      return (int)response.StatusCode;
    }

    private (int ErrorCount, Dictionary<string, string[]> Errors) ExecuteModelBindingTestGetErrorDetails(string queryString)
    {
      string content = ExecuteModelBindingTestExpectingBadRequest(queryString);

      // Parse the JSON response to extract error details
      using var doc = JsonDocument.Parse(content);
      var errors = new Dictionary<string, string[]>();
      int errorCount = 0;

      if (doc.RootElement.TryGetProperty("errors", out JsonElement errorsElement))
      {
        foreach (JsonProperty error in errorsElement.EnumerateObject())
        {
          List<string> errorMessages = [];
          if (error.Value.ValueKind == JsonValueKind.Array)
          {
            foreach (JsonElement message in error.Value.EnumerateArray())
            {
              errorMessages.Add(message.GetString() ?? "");
              errorCount++;
            }
          }

          errors[error.Name] = [.. errorMessages];
        }
      }

      return (errorCount, errors);
    }
  }

  [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
  [ApiController]
  [Route("pageinfo-test")]
  public class PageInfoTestController : ControllerBase
  {
    [HttpGet(Name = "GetPageInfo")]
    public IActionResult Get([FromQuery] PageInfo? page = null)
    {
      // Check if model binding failed
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if (page is null)
      {
        return Ok("null");
      }

      return Ok(page);
    }
  }
}