// -----------------------------------------------------------------------
// <copyright file="DelimitedQueryStringValueProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class DelimitedQueryStringValueProviderTests
  {
    private IQueryCollection _emptyQueryCollection = null!;
    private IQueryCollection _basicQueryCollection = null!;
    private IQueryCollection _delimitedQueryCollection = null!;
    private IQueryCollection _multiValueQueryCollection = null!;

    public TestContext TestContext
    {
      get;
      set;
    }

    [TestInitialize]
    public void TestInitialize()
    {
      _emptyQueryCollection = new QueryCollection();

      _basicQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["singleValue"] = new StringValues("test"),
        ["anotherKey"] = new StringValues("anotherValue")
      });

      _delimitedQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["ids"] = new StringValues("1,2,3,4,5"),
        ["names"] = new StringValues("John,Jane,Bob"),
        ["flags"] = new StringValues("true,false,true"),
        ["mixed"] = new StringValues("value1,value2")
      });

      _multiValueQueryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["categories"] = new StringValues(["electronics,books", "toys,games"]),
        ["tags"] = new StringValues("red,blue,green")
      });
    }

    [TestMethod]
    public void When_values_is_null_Constructor_throws_ArgumentNullException() =>
      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = new DelimitedQueryStringValueProvider(null!, "testKey");
      });

    [TestMethod]
    public void When_valid_parameters_Constructor_creates_instance_successfully()
    {
      // Act
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "testKey");

      // Assert
      Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void When_valid_parameters_with_custom_separator_Constructor_creates_instance_successfully()
    {
      // Act
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "testKey", ';');

      // Assert
      Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void When_key_does_not_match_parameterKey_GetValue_returns_None()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "ids");

      // Act
      ValueProviderResult result = provider.GetValue("singleValue");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_but_no_value_exists_GetValue_returns_None()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_emptyQueryCollection, "nonExistentKey");

      // Act
      ValueProviderResult result = provider.GetValue("nonExistentKey");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_single_value_GetValue_returns_single_value()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "singleValue");

      // Act
      ValueProviderResult result = provider.GetValue("singleValue");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual("test", result.FirstValue);
      Assert.AreEqual(1, result.Values.Count);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_delimited_values_GetValue_splits_values()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act
      ValueProviderResult result = provider.GetValue("ids");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(5, result.Values.Count);
      Assert.AreEqual("1", result.Values[0]);
      Assert.AreEqual("2", result.Values[1]);
      Assert.AreEqual("3", result.Values[2]);
      Assert.AreEqual("4", result.Values[3]);
      Assert.AreEqual("5", result.Values[4]);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_string_values_GetValue_splits_correctly()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "names");

      // Act
      ValueProviderResult result = provider.GetValue("names");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("John", result.Values[0]);
      Assert.AreEqual("Jane", result.Values[1]);
      Assert.AreEqual("Bob", result.Values[2]);
    }

    [TestMethod]
    public void When_key_matches_parameterKey_with_boolean_values_GetValue_splits_correctly()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "flags");

      // Act
      ValueProviderResult result = provider.GetValue("flags");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("true", result.Values[0]);
      Assert.AreEqual("false", result.Values[1]);
      Assert.AreEqual("true", result.Values[2]);
    }

    [TestMethod]
    public void When_custom_separator_provided_GetValue_splits_using_custom_separator()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["data"] = new StringValues("item1;item2;item3;item4")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "data", ';');

      // Act
      ValueProviderResult result = provider.GetValue("data");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(4, result.Values.Count);
      Assert.AreEqual("item1", result.Values[0]);
      Assert.AreEqual("item2", result.Values[1]);
      Assert.AreEqual("item3", result.Values[2]);
      Assert.AreEqual("item4", result.Values[3]);
    }

    [TestMethod]
    public void When_pipe_separator_provided_GetValue_splits_using_pipe_separator()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["categories"] = new StringValues("tech|business|health|sports")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "categories", '|');

      // Act
      ValueProviderResult result = provider.GetValue("categories");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(4, result.Values.Count);
      Assert.AreEqual("tech", result.Values[0]);
      Assert.AreEqual("business", result.Values[1]);
      Assert.AreEqual("health", result.Values[2]);
      Assert.AreEqual("sports", result.Values[3]);
    }

    [TestMethod]
    public void When_value_contains_empty_segments_GetValue_includes_empty_strings()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["data"] = new StringValues("value1,,value3,")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "data");

      // Act
      ValueProviderResult result = provider.GetValue("data");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(4, result.Values.Count);
      Assert.AreEqual("value1", result.Values[0]);
      Assert.AreEqual(string.Empty, result.Values[1]);
      Assert.AreEqual("value3", result.Values[2]);
      Assert.AreEqual(string.Empty, result.Values[3]);
    }

    [TestMethod]
    public void When_value_has_no_separator_GetValue_returns_original_value()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["singleItem"] = new StringValues("onlyOneValue")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "singleItem");

      // Act
      ValueProviderResult result = provider.GetValue("singleItem");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(1, result.Values.Count);
      Assert.AreEqual("onlyOneValue", result.FirstValue);
    }

    [TestMethod]
    public void When_value_is_empty_string_GetValue_returns_original_empty_value()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["emptyValue"] = new StringValues(string.Empty)
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "emptyValue");

      // Act
      ValueProviderResult result = provider.GetValue("emptyValue");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(1, result.Values.Count);
      Assert.AreEqual(string.Empty, result.FirstValue);
    }

    [TestMethod]
    public void When_value_contains_only_separators_GetValue_returns_empty_segments()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["onlySeparators"] = new StringValues(",,,")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "onlySeparators");

      // Act
      ValueProviderResult result = provider.GetValue("onlySeparators");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(4, result.Values.Count);
      Assert.IsTrue(result.Values.All(string.IsNullOrEmpty));
    }

    [TestMethod]
    public void When_multiple_query_values_exist_GetValue_processes_all_values()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_multiValueQueryCollection, "categories");

      // Act
      ValueProviderResult result = provider.GetValue("categories");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(4, result.Values.Count);
      Assert.AreEqual("electronics", result.Values[0]);
      Assert.AreEqual("books", result.Values[1]);
      Assert.AreEqual("toys", result.Values[2]);
      Assert.AreEqual("games", result.Values[3]);
    }

    [TestMethod]
    public void When_value_has_separator_at_position_zero_GetValue_processes_all_values()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["edgeCase"] = new StringValues(",startsWithComma")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "edgeCase");

      // Act
      ValueProviderResult result = provider.GetValue("edgeCase");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual(string.Empty, result.Values[0]);
      Assert.AreEqual("startsWithComma", result.Values[1]);
    }

    [TestMethod]
    public void When_value_has_separator_at_position_one_GetValue_processes_all_values()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["edgeCase"] = new StringValues("a,secondValue")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "edgeCase");

      // Act
      ValueProviderResult result = provider.GetValue("edgeCase");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual("a", result.Values[0]);
      Assert.AreEqual("secondValue", result.Values[1]);
    }

    [TestMethod]
    public void When_culture_is_invariant_GetValue_preserves_culture()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act
      ValueProviderResult result = provider.GetValue("ids");

      // Assert
      Assert.AreEqual(CultureInfo.InvariantCulture, result.Culture);
    }

    [TestMethod]
    [DataRow("IDS", "ids")]
    [DataRow("ids", "IDS")]
    public void When_key_casing_differs_GetValue_uses_ordinal_comparison(string constructorValue, string keyValue)
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, constructorValue);

      // Act
      ValueProviderResult result = provider.GetValue(keyValue);

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_whitespace_values_exist_GetValue_preserves_whitespace()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["whitespace"] = new StringValues("  value1  ,  value2  ,value3")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "whitespace");

      // Act
      ValueProviderResult result = provider.GetValue("whitespace");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("  value1  ", result.Values[0]);
      Assert.AreEqual("  value2  ", result.Values[1]);
      Assert.AreEqual("value3", result.Values[2]);
    }

    [TestMethod]
    public void When_special_characters_in_values_GetValue_preserves_special_characters()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["special"] = new StringValues("value@1,value#2,value$3")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "special");

      // Act
      ValueProviderResult result = provider.GetValue("special");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("value@1", result.Values[0]);
      Assert.AreEqual("value#2", result.Values[1]);
      Assert.AreEqual("value$3", result.Values[2]);
    }

    [TestMethod]
    public void When_unicode_characters_in_values_GetValue_preserves_unicode()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["unicode"] = new StringValues("测试,тест,テスト")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "unicode");

      // Act
      ValueProviderResult result = provider.GetValue("unicode");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("测试", result.Values[0]);
      Assert.AreEqual("тест", result.Values[1]);
      Assert.AreEqual("テスト", result.Values[2]);
    }

    [TestMethod]
    public void When_parameterKey_is_null_Constructor_throws_ArgumentNullException() =>
      _ = Assert.ThrowsExactly<ArgumentNullException>(() => new DelimitedQueryStringValueProvider(_basicQueryCollection, null!));

    [TestMethod]
    public void When_parameterKey_is_empty_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new DelimitedQueryStringValueProvider(_basicQueryCollection, string.Empty));

    [TestMethod]
    public void When_parameterKey_is_whitespace_Constructor_throws_ArgumentException() =>
      _ = Assert.ThrowsExactly<ArgumentException>(() => new DelimitedQueryStringValueProvider(_basicQueryCollection, "   "));

    [TestMethod]
    public void When_value_contains_only_whitespace_GetValue_does_not_split()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["whitespaceOnly"] = new StringValues("   ")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "whitespaceOnly");

      // Act
      ValueProviderResult result = provider.GetValue("whitespaceOnly");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(1, result.Values.Count);
      Assert.AreEqual("   ", result.FirstValue);
    }

    [TestMethod]
    public void When_value_contains_separator_with_whitespace_around_GetValue_splits_correctly()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["spaceAroundSeparator"] = new StringValues("value1 , value2")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "spaceAroundSeparator");

      // Act
      ValueProviderResult result = provider.GetValue("spaceAroundSeparator");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual("value1 ", result.Values[0]);
      Assert.AreEqual(" value2", result.Values[1]);
    }

    [TestMethod]
    public void When_value_is_null_in_collection_GetValue_handles_gracefully()
    {
      // Arrange
      var stringValues = new StringValues([null!, "value2,value3"]);
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["mixedNulls"] = stringValues
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "mixedNulls");

      // Act
      ValueProviderResult result = provider.GetValue("mixedNulls");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual("value2", result.Values[0]);
      Assert.AreEqual("value3", result.Values[1]);
    }

    [TestMethod]
    public void When_all_values_are_whitespace_only_GetValue_does_not_split()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["allWhitespace"] = new StringValues(["   ", "\t", "\n"])
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "allWhitespace");

      // Act
      ValueProviderResult result = provider.GetValue("allWhitespace");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("   ", result.Values[0]);
      Assert.AreEqual("\t", result.Values[1]);
      Assert.AreEqual("\n", result.Values[2]);
    }

    [TestMethod]
    public void When_prefix_matches_parameterKey_ContainsPrefix_returns_true()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act
      bool result = provider.ContainsPrefix("ids");

      // Assert
      Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_prefix_does_not_match_parameterKey_ContainsPrefix_returns_false()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act
      bool result = provider.ContainsPrefix("nonexistent");

      // Assert
      Assert.IsFalse(result);
    }

    [TestMethod]
    public void When_prefix_is_empty_ContainsPrefix_handles_correctly()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act
      bool result = provider.ContainsPrefix(string.Empty);

      // Assert
      Assert.IsTrue(result); // QueryStringValueProvider typically returns true for empty prefix
    }

    [TestMethod]
    public void When_prefix_is_null_ContainsPrefix_handles_correctly()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = provider.ContainsPrefix(null!);
      });
    }

    [TestMethod]
    public void When_key_is_null_GetValue_throws_ArgumentNullException()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "testKey");

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = provider.GetValue(null!);
      });
    }

    [TestMethod]
    public void When_key_is_empty_GetValue_handles_correctly()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, "testKey");

      // Act
      ValueProviderResult result = provider.GetValue(string.Empty);

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_separator_appears_in_complex_patterns_GetValue_splits_correctly()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["complex"] = new StringValues(",,item1,,item2,,")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "complex");

      // Act
      ValueProviderResult result = provider.GetValue("complex");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(7, result.Values.Count);
      Assert.AreEqual(string.Empty, result.Values[0]);
      Assert.AreEqual(string.Empty, result.Values[1]);
      Assert.AreEqual("item1", result.Values[2]);
      Assert.AreEqual(string.Empty, result.Values[3]);
      Assert.AreEqual("item2", result.Values[4]);
      Assert.AreEqual(string.Empty, result.Values[5]);
      Assert.AreEqual(string.Empty, result.Values[6]);
    }

    [TestMethod]
    public void When_large_number_of_values_GetValue_processes_efficiently()
    {
      // Arrange
      List<string> largeValueList = [];
      for (int i = 0; i < 1000; i++)
      {
        largeValueList.Add($"value{i}");
      }

      string largeValue = string.Join(",", largeValueList);
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["largeSet"] = new StringValues(largeValue)
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "largeSet");

      // Act
      ValueProviderResult result = provider.GetValue("largeSet");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(1000, result.Values.Count);
      Assert.AreEqual("value0", result.Values[0]);
      Assert.AreEqual("value999", result.Values[999]);
    }

    [TestMethod]
    public void When_very_long_individual_values_GetValue_handles_correctly()
    {
      // Arrange
      string longValue1 = new string('A', 1000);
      string longValue2 = new string('B', 1000);
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["longValues"] = new StringValues($"{longValue1},{longValue2}")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "longValues");

      // Act
      ValueProviderResult result = provider.GetValue("longValues");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual(longValue1, result.Values[0]);
      Assert.AreEqual(longValue2, result.Values[1]);
    }

    [TestMethod]
    public void When_values_contain_url_encoded_characters_GetValue_preserves_encoding()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["encoded"] = new StringValues("value%20with%20spaces,another%2Bvalue")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "encoded");

      // Act
      ValueProviderResult result = provider.GetValue("encoded");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(2, result.Values.Count);
      Assert.AreEqual("value%20with%20spaces", result.Values[0]);
      Assert.AreEqual("another%2Bvalue", result.Values[1]);
    }

    [TestMethod]
    public void When_separator_is_control_character_GetValue_splits_correctly()
    {
      // Arrange
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["controlSep"] = new StringValues("value1\tvalue2\tvalue3")
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "controlSep", '\t');

      // Act
      ValueProviderResult result = provider.GetValue("controlSep");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("value1", result.Values[0]);
      Assert.AreEqual("value2", result.Values[1]);
      Assert.AreEqual("value3", result.Values[2]);
    }

    [TestMethod]
    [DataRow(',', "comma")]
    [DataRow(';', "semicolon")]
    [DataRow('|', "pipe")]
    [DataRow('\t', "tab")]
    [DataRow(' ', "space")]
    [DataRow(':', "colon")]
    [DataRow('-', "dash")]
    [DataRow('_', "underscore")]
    [DataRow('\n', "newline")]
    [DataRow('\r', "carriage return")]
    public void When_various_separators_used_GetValue_splits_correctly(char separator, string description)
    {
      // Arrange
      string testValue = $"item1{separator}item2{separator}item3";
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["testData"] = new StringValues(testValue)
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "testData", separator);

      // Act
      ValueProviderResult result = provider.GetValue("testData");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result, $"{description}");
      Assert.AreEqual(3, result.Values.Count);
      Assert.AreEqual("item1", result.Values[0], $"{description}");
      Assert.AreEqual("item2", result.Values[1], $"{description}");
      Assert.AreEqual("item3", result.Values[2], $"{description}");
    }

    [TestMethod]
    [DataRow("normalKey")]
    [DataRow("key-with-dashes")]
    [DataRow("key_with_underscores")]
    [DataRow("KeyWithCaps")]
    [DataRow("key.with.dots")]
    [DataRow("key@with!special#chars")]
    [DataRow("测试键")]
    [DataRow("ключ")]
    public void When_various_parameter_keys_used_Constructor_creates_instance_successfully(string parameterKey)
    {
      // Arrange & Act
      var provider = new DelimitedQueryStringValueProvider(_basicQueryCollection, parameterKey);

      // Assert
      Assert.IsNotNull(provider);
    }

    [TestMethod]
    public void When_cast_to_base_type_behavior_remains_consistent()
    {
      // Arrange
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
      IValueProvider provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

      // Act
      ValueProviderResult result = provider.GetValue("ids");

      // Assert
      Assert.AreNotEqual(ValueProviderResult.None, result);
      Assert.AreEqual(5, result.Values.Count);
    }

    [TestMethod]
    public void When_query_collection_is_corrupted_GetValue_handles_gracefully()
    {
      // Arrange
      var corruptedValues = new StringValues([]);  // Empty array
      var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
      {
        ["empty"] = corruptedValues
      });
      var provider = new DelimitedQueryStringValueProvider(queryCollection, "empty");

      // Act
      ValueProviderResult result = provider.GetValue("empty");

      // Assert
      Assert.AreEqual(ValueProviderResult.None, result);
    }

    [TestMethod]
    public void When_concurrent_access_GetValue_thread_safe()
    {
      // Arrange
      var provider = new DelimitedQueryStringValueProvider(_delimitedQueryCollection, "ids");
      var results = new ValueProviderResult[10];
      var tasks = new Task[10];

      // Act
      for (int i = 0; i < 10; i++)
      {
        int index = i;
        tasks[i] = Task.Run(() =>
        {
          results[index] = provider.GetValue("ids");
        }, TestContext.CancellationToken);
      }

      Task.WaitAll(tasks, TestContext.CancellationToken);

      // Assert
      foreach (ValueProviderResult result in results)
      {
        Assert.AreNotEqual(ValueProviderResult.None, result);
        Assert.AreEqual(5, result.Values.Count);
      }
    }
  }
}