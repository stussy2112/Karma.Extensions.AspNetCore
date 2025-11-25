// -----------------------------------------------------------------------
// <copyright file="RegExExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore.Tests.Extensions
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class RegExExtensionsTests
  {
    [TestMethod]
    public void When_groupCollection_is_null_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      GroupCollection? groupCollection = null;
      string key = "test";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_groupCollection_is_null_and_defaultValue_is_null_GetGroupCollectionValue_returns_null()
    {
      // Arrange
      GroupCollection? groupCollection = null;
      string key = "test";
      string? defaultValue = null;

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_groupCollection_is_null_and_defaultValue_is_empty_GetGroupCollectionValue_returns_empty_string()
    {
      // Arrange
      GroupCollection? groupCollection = null;
      string key = "test";
      string defaultValue = string.Empty;

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void When_group_does_not_exist_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<name>\\w+)");
      Match match = regex.Match("test");
      GroupCollection groupCollection = match.Groups;
      string key = "nonexistent";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_exists_but_not_successful_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<name>\\w+)?");
      Match match = regex.Match("");
      GroupCollection groupCollection = match.Groups;
      string key = "name";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_value_is_whitespace_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<name>\\s+)");
      Match match = regex.Match("   ");
      GroupCollection groupCollection = match.Groups;
      string key = "name";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_value_is_empty_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<name>)");
      Match match = regex.Match("test");
      GroupCollection groupCollection = match.Groups;
      string key = "name";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_has_valid_value_GetGroupCollectionValue_returns_unescaped_value()
    {
      // Arrange
      var regex = new Regex("(?<name>\\w+)");
      Match match = regex.Match("test");
      GroupCollection groupCollection = match.Groups;
      string key = "name";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("test", result);
    }

    [TestMethod]
    public void When_group_has_url_encoded_value_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<value>.+)");
      Match match = regex.Match("Hello%20World");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void When_group_has_special_characters_encoded_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<value>.+)");
      Match match = regex.Match("test%2Bvalue%3D123");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("test+value=123", result);
    }

    [TestMethod]
    public void When_group_has_percent_encoded_special_chars_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<value>.+)");
      Match match = regex.Match("name%26age%3D25");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("name&age=25", result);
    }

    [TestMethod]
    public void When_group_has_complex_url_encoded_value_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<query>.+)");
      Match match = regex.Match("filter%5Bname%5D%3Deq%3AJohn%20Doe");
      GroupCollection groupCollection = match.Groups;
      string key = "query";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("filter[name]=eq:John Doe", result);
    }

    [TestMethod]
    public void When_multiple_groups_exist_GetGroupCollectionValue_returns_correct_group_value()
    {
      // Arrange
      var regex = new Regex("(?<first>\\w+)\\s+(?<second>\\w+)");
      Match match = regex.Match("Hello World");
      GroupCollection groupCollection = match.Groups;

      // Act
      string? firstResult = groupCollection.GetGroupCollectionValue("first");
      string? secondResult = groupCollection.GetGroupCollectionValue("second");

      // Assert
      Assert.AreEqual("Hello", firstResult);
      Assert.AreEqual("World", secondResult);
    }

    [TestMethod]
    public void When_default_parameter_not_provided_GetGroupCollectionValue_uses_empty_string_as_default()
    {
      // Arrange
      var regex = new Regex("(?<name>\\w+)");
      Match match = regex.Match("");
      GroupCollection groupCollection = match.Groups;
      string key = "name";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void When_group_value_contains_only_tabs_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<value>\\t+)");
      Match match = regex.Match("\t\t\t");
      GroupCollection groupCollection = match.Groups;
      string key = "value";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_value_contains_newlines_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<value>[\\r\\n]+)");
      Match match = regex.Match("\r\n");
      GroupCollection groupCollection = match.Groups;
      string key = "value";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_has_numeric_value_GetGroupCollectionValue_returns_value_as_string()
    {
      // Arrange
      var regex = new Regex("(?<number>\\d+)");
      Match match = regex.Match("12345");
      GroupCollection groupCollection = match.Groups;
      string key = "number";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("12345", result);
    }

    [TestMethod]
    public void When_group_has_unicode_characters_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<value>.+)");
      Match match = regex.Match("%C3%A9%C3%A0%C3%B1");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("éàñ", result);
    }

    [TestMethod]
    public void When_group_has_mixed_encoded_and_plain_text_GetGroupCollectionValue_returns_decoded_value()
    {
      // Arrange
      var regex = new Regex("(?<value>.+)");
      Match match = regex.Match("plain%20text%2Bmore");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("plain text+more", result);
    }

    [TestMethod]
    public void When_group_has_single_character_GetGroupCollectionValue_returns_character()
    {
      // Arrange
      var regex = new Regex("(?<char>\\w)");
      Match match = regex.Match("A");
      GroupCollection groupCollection = match.Groups;
      string key = "char";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("A", result);
    }

    [TestMethod]
    public void When_requesting_nonexistent_group_with_custom_defaultValue_GetGroupCollectionValue_returns_custom_default()
    {
      // Arrange
      var regex = new Regex("(?<existing>\\w+)");
      Match match = regex.Match("test");
      GroupCollection groupCollection = match.Groups;
      string key = "missing";
      string customDefault = "CUSTOM_DEFAULT";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, customDefault);

      // Assert
      Assert.AreEqual(customDefault, result);
    }

    [TestMethod]
    public void When_group_value_is_single_space_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<value> )");
      Match match = regex.Match(" ");
      GroupCollection groupCollection = match.Groups;
      string key = "value";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_has_value_with_leading_trailing_spaces_but_middle_content_GetGroupCollectionValue_returns_defaultValue()
    {
      // Arrange
      var regex = new Regex("(?<value>\\s+)");
      Match match = regex.Match("  ");
      GroupCollection groupCollection = match.Groups;
      string key = "value";
      string defaultValue = "default";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key, defaultValue);

      // Assert
      Assert.AreEqual(defaultValue, result);
    }

    [TestMethod]
    public void When_group_has_hyphenated_value_GetGroupCollectionValue_returns_value()
    {
      // Arrange
      var regex = new Regex("(?<value>[\\w-]+)");
      Match match = regex.Match("test-value-123");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("test-value-123", result);
    }

    [TestMethod]
    public void When_group_has_underscore_value_GetGroupCollectionValue_returns_value()
    {
      // Arrange
      var regex = new Regex("(?<value>\\w+)");
      Match match = regex.Match("test_value_123");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("test_value_123", result);
    }

    [TestMethod]
    public void When_group_has_dot_separated_value_GetGroupCollectionValue_returns_value()
    {
      // Arrange
      var regex = new Regex("(?<value>[\\w.]+)");
      Match match = regex.Match("test.value.123");
      GroupCollection groupCollection = match.Groups;
      string key = "value";

      // Act
      string? result = groupCollection.GetGroupCollectionValue(key);

      // Assert
      Assert.AreEqual("test.value.123", result);
    }
  }
}
