// -----------------------------------------------------------------------
// <copyright file="CompleteKeyedQueryStringValueProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding.Tests
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class CompleteKeyedQueryStringValueProviderTests
  {
    private IQueryCollection _emptyQueryCollection = null!;
    private IQueryCollection _basicQueryCollection = null!;
    private IQueryCollection _keyedQueryCollection = null!;
    private IQueryCollection _multiValueQueryCollection = null!;

    [TestInitialize]
    public void TestInitialize()
    {
      _emptyQueryCollection = new QueryCollection();

      _basicQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter"] = new StringValues("name=john"),
        ["other"] = new StringValues("value")
      });

      _keyedQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[name]"] = new StringValues("john"),
        ["filter[age]"] = new StringValues("25"),
        ["filter[active]"] = new StringValues("true"),
        ["sort[name]"] = new StringValues("asc"),
        ["other"] = new StringValues("ignore")
      });

      _multiValueQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[categories]"] = new StringValues(["electronics", "books"]),
        ["filter[tags]"] = new StringValues("red,blue"),
        ["filter"] = new StringValues("basic"),
        ["other"] = new StringValues("ignore")
      });
    }

    [TestMethod]
    public void When_queryCollection_is_null_Constructor_throws_ArgumentNullException() =>
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = new CompleteKeyedQueryStringValueProvider(null!, "filter");
      });

    [TestMethod]
    public void When_parameterKey_is_null_Constructor_throws_ArgumentNullException() =>
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, null!));

    [TestMethod]
    public void When_parameterKey_is_empty_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, string.Empty));

    [TestMethod]
    public void When_parameterKey_is_whitespace_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, "   "));

    [TestMethod]
    public void When_valid_parameters_Constructor_creates_instance_successfully()
    {
      // Act
      var provider = new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, "filter");

      // Assert
      Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void When_key_is_null_GetValue_throws_ArgumentNullException()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, "filter");

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = provider.GetValue(null!);
      });
    }

    [TestMethod]
    public void When_key_does_not_match_parameterKey_GetValue_returns_None()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("sort");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_exact_match_GetValue_returns_aggregated_string()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual("filter=name=john", result.FirstValue);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_keyed_parameters_GetValue_returns_aggregated_string()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[name]=john", resultValue);
      Assert.Contains("filter[age]=25", resultValue);
      Assert.Contains("filter[active]=true", resultValue);
      Assert.DoesNotContain("sort[name]=asc", resultValue);
      Assert.DoesNotContain("other=ignore", resultValue);
    }

    [TestMethod]
    public void When_multiple_values_per_key_GetValue_aggregates_all_values()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_multiValueQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[categories]=electronics", resultValue);
      Assert.Contains("filter[categories]=books", resultValue);
      Assert.Contains("filter[tags]=red,blue", resultValue);
      Assert.Contains("filter=basic", resultValue);
      Assert.DoesNotContain("other=ignore", resultValue);
    }

    [TestMethod]
    public void When_empty_values_exist_GetValue_handles_gracefully()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[empty]"] = new StringValues(""),
        ["filter[name]"] = new StringValues("john")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[empty]", resultValue);
      Assert.DoesNotContain("filter[empty]=", resultValue);
      Assert.Contains("filter[name]=john", resultValue);
    }

    [TestMethod]
    public void When_whitespace_only_values_exist_GetValue_handles_gracefully()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[whitespace]"] = new StringValues("   "),
        ["filter[name]"] = new StringValues("john")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[whitespace]", resultValue);
      Assert.DoesNotContain("filter[whitespace]=", resultValue);
      Assert.Contains("filter[name]=john", resultValue);
    }

    [TestMethod]
    public void When_prefix_matches_parameterKey_ContainsPrefix_returns_true()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");

      // Act
      bool result = provider.ContainsPrefix("filter");

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_prefix_does_not_match_parameterKey_ContainsPrefix_returns_false()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");

      // Act
      bool result = provider.ContainsPrefix("nonexistent");

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_key_is_empty_GetValue_returns_None()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue(string.Empty);

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_parameter_key_casing_differs_in_query_ContainsPrefix_matches_case_sensitive()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["FILTER[name]"] = new StringValues("john")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      bool result = provider.ContainsPrefix("filter");

      // Assert
      Assert.IsFalse(result, "ContainsPrefix should use case-sensitive comparison");
    }

    [TestMethod]
    public void When_get_value_key_casing_differs_GetValue_uses_exact_case()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("FILTER");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result, "GetValue should use exact case comparison");
    }

    [TestMethod]
    public void When_complex_query_string_aggregation_GetValue_maintains_proper_format()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[name]"] = new StringValues("john doe"),
        ["filter[age]"] = new StringValues("25"),
        ["filter[tags]"] = new StringValues(["red", "blue", "green"]),
        ["filter"] = new StringValues("basic"),
        ["other"] = new StringValues("ignore")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;

      // Should contain all filter parameters but not others
      Assert.Contains("filter[name]=john doe", resultValue);
      Assert.Contains("filter[age]=25", resultValue);
      Assert.Contains("filter[tags]=red", resultValue);
      Assert.Contains("filter[tags]=blue", resultValue);
      Assert.Contains("filter[tags]=green", resultValue);
      Assert.Contains("filter=basic", resultValue);
      Assert.DoesNotContain("other=ignore", resultValue);

      // Should be properly separated by ampersands
      string[] parts = resultValue.Split('&');
      Assert.IsGreaterThan(1, parts.Length);
    }

    [TestMethod]
    public void When_special_characters_in_values_GetValue_preserves_special_characters()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[special]"] = new StringValues("value@1#2$3"),
        ["filter[encoded]"] = new StringValues("value%20with%20spaces")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[special]=value@1#2$3", resultValue);
      Assert.Contains("filter[encoded]=value%20with%20spaces", resultValue);
    }

    [TestMethod]
    public void When_unicode_characters_in_values_GetValue_preserves_unicode()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[unicode]"] = new StringValues("??"),
        ["filter[cyrillic]"] = new StringValues("????")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[unicode]=??", resultValue);
      Assert.Contains("filter[cyrillic]=????", resultValue);
    }

    [TestMethod]
    public void When_value_is_null_in_collection_GetValue_handles_gracefully()
    {
      // Arrange
      var stringValues = new StringValues([null!, "value2"]);
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[mixed]"] = stringValues
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[mixed]", resultValue);
      Assert.Contains("filter[mixed]=value2", resultValue);
    }

    [TestMethod]
    public void When_large_number_of_matching_parameters_GetValue_processes_efficiently()
    {
      // Arrange
      var queryDictionary = new Dictionary<string, StringValues>();
      for (int i = 0; i < 100; i++)
      {
        queryDictionary[$"filter[param{i}]"] = new StringValues($"value{i}");
      }

      var queryCollection = new QueryCollection(queryDictionary);
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      DateTime startTime = DateTime.UtcNow;
      ValueProviderResult result = provider.GetValue("filter");
      TimeSpan duration = DateTime.UtcNow - startTime;

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.IsLessThan(50, duration.TotalMilliseconds, $"Operation took {duration.TotalMilliseconds}ms, expected < 50ms");

      string resultValue = result.FirstValue!;
      Assert.Contains("filter[param0]=value0", resultValue);
      Assert.Contains("filter[param99]=value99", resultValue);
    }

    [TestMethod]
    public void When_very_long_parameter_values_GetValue_handles_correctly()
    {
      // Arrange
      string longValue = new string('A', 1000);
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["filter[long]"] = new StringValues(longValue),
        ["filter[short]"] = new StringValues("short")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains($"filter[long]={longValue}", resultValue);
      Assert.Contains("filter[short]=short", resultValue);
    }

    [TestMethod]
    [DataRow("normalKey")]
    [DataRow("key-with-dashes")]
    [DataRow("key_with_underscores")]
    [DataRow("KeyWithCaps")]
    [DataRow("key.with.dots")]
    [DataRow("key@with!special#chars")]
    [DataRow("???")]
    [DataRow("????")]
    public void When_various_parameter_keys_used_Constructor_creates_instance_successfully(string parameterKey)
    {
      // Arrange & Act
      var provider = new CompleteKeyedQueryStringValueProvider(_basicQueryCollection, parameterKey);

      // Assert
      Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void When_cast_to_base_type_behavior_remains_consistent()
    {
      // Arrange
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
      IValueProvider provider = new CompleteKeyedQueryStringValueProvider(_keyedQueryCollection, "filter");
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      string resultValue = result.FirstValue!;
      Assert.Contains("filter[name]=john", resultValue);
    }

    [TestMethod]
    public void When_empty_query_collection_GetValue_returns_None()
    {
      // Arrange
      var provider = new CompleteKeyedQueryStringValueProvider(_emptyQueryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_query_collection_has_no_matching_keys_GetValue_returns_None()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["sort[name]"] = new StringValues("asc"),
        ["page"] = new StringValues("1")
      });
      var provider = new CompleteKeyedQueryStringValueProvider(queryCollection, "filter");

      // Act
      ValueProviderResult result = provider.GetValue("filter");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }
  }
}