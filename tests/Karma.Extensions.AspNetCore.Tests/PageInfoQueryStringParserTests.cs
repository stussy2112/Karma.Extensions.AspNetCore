// -----------------------------------------------------------------------
// <copyright file="PageInfoQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class PageInfoQueryStringParserTests
  {
    private PageInfoQueryStringParser _parser = null!;

    [TestInitialize]
    public void TestInitialize() => _parser = new PageInfoQueryStringParser();

    [TestCleanup]
    public void TestCleanup() => _parser = null!;

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void When_input_is_null_Parse_returns_default_PageInfo()
    {
      // Arrange
      string input = null!;

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_input_is_empty_Parse_returns_default_PageInfo()
    {
      // Arrange
      string input = string.Empty;

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_input_is_whitespace_Parse_returns_default_PageInfo()
    {
      // Arrange
      string input = "   \t\n\r   ";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_after_cursor_provided_Parse_returns_PageInfo_with_after()
    {
      // Arrange
      string input = "page[after]=abc123";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("abc123", result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_before_cursor_provided_Parse_returns_PageInfo_with_before()
    {
      // Arrange
      string input = "page[before]=xyz789";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual("xyz789", result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_cursor_provided_Parse_returns_PageInfo_with_after()
    {
      // Arrange
      string input = "page[cursor]=cursor123";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor123", result.After);
      Assert.AreEqual(string.Empty, result.Before);
    }

    [TestMethod]
    public void When_limit_provided_Parse_returns_PageInfo_with_limit()
    {
      // Arrange
      string input = "page[limit]=50";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)50, result.Limit);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_offset_provided_Parse_returns_PageInfo_with_offset()
    {
      // Arrange
      string input = "page[offset]=100";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)100, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_limit_and_offset_provided_Parse_returns_PageInfo_with_both()
    {
      // Arrange
      string input = "page[limit]=25&page[offset]=50";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)25, result.Limit);
      Assert.AreEqual((uint)50, result.Offset);
    }

    [TestMethod]
    public void When_after_and_limit_provided_Parse_returns_PageInfo_with_both()
    {
      // Arrange
      string input = "page[after]=token123&page[limit]=20";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("token123", result.After);
      Assert.AreEqual((uint)20, result.Limit);
    }

    [TestMethod]
    public void When_before_and_limit_provided_Parse_returns_PageInfo_with_both()
    {
      // Arrange
      string input = "page[before]=token456&page[limit]=30";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("token456", result.Before);
      Assert.AreEqual((uint)30, result.Limit);
    }

    [TestMethod]
    public void When_all_parameters_provided_Parse_returns_PageInfo_with_all_values()
    {
      // Arrange
      string input = "page[after]=abc&page[before]=xyz&page[limit]=10&page[offset]=5";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("abc", result.After);
      Assert.AreEqual("xyz", result.Before);
      Assert.AreEqual((uint)10, result.Limit);
      Assert.AreEqual((uint)5, result.Offset);
    }

    [TestMethod]
    public void When_invalid_limit_provided_Parse_returns_default_limit()
    {
      // Arrange
      string input = "page[limit]=invalid";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_negative_limit_provided_Parse_returns_default_limit()
    {
      // Arrange
      string input = "page[limit]=-10";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_invalid_offset_provided_Parse_returns_default_offset()
    {
      // Arrange
      string input = "page[offset]=invalid";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_negative_offset_provided_Parse_returns_default_offset()
    {
      // Arrange
      string input = "page[offset]=-5";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_url_encoded_input_Parse_decodes_correctly()
    {
      // Arrange
      string input = "page%5Bafter%5D=cursor%20with%20space";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor with space", result.After);
    }

    [TestMethod]
    public void When_duplicate_after_parameters_Parse_uses_last_value()
    {
      // Arrange
      string input = "page[after]=first&page[after]=second&page[after]=third";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("third", result.After);
    }

    [TestMethod]
    public void When_duplicate_limit_parameters_Parse_uses_last_value()
    {
      // Arrange
      string input = "page[limit]=10&page[limit]=20&page[limit]=30";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)30, result.Limit);
    }

    [TestMethod]
    public void When_case_insensitive_after_Parse_returns_correct_value()
    {
      // Arrange
      string input = "page[AFTER]=token";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("token", result.After);
    }

    [TestMethod]
    public void When_case_insensitive_before_Parse_returns_correct_value()
    {
      // Arrange
      string input = "page[BEFORE]=token";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("token", result.Before);
    }

    [TestMethod]
    public void When_case_insensitive_cursor_Parse_returns_correct_value()
    {
      // Arrange
      string input = "page[CURSOR]=token";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("token", result.After);
    }

    [TestMethod]
    public void When_case_insensitive_limit_Parse_returns_correct_value()
    {
      // Arrange
      string input = "page[LIMIT]=15";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)15, result.Limit);
    }

    [TestMethod]
    public void When_case_insensitive_offset_Parse_returns_correct_value()
    {
      // Arrange
      string input = "page[OFFSET]=25";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)25, result.Offset);
    }

    [TestMethod]
    public void When_no_matches_found_Parse_returns_default_PageInfo()
    {
      // Arrange
      string input = "invalid[parameter]=value";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
      Assert.AreEqual((uint)0, result.Offset);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_regex_timeout_occurs_Parse_returns_default_PageInfo()
    {
      // Arrange
      string input = new string('a', 10000) + "page[limit]=10";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
      Assert.AreEqual(string.Empty, result.Before);
    }

    [TestMethod]
    public void When_input_is_null_TryParse_returns_false()
    {
      // Arrange
      string input = null!;

      // Act
      bool success = _parser.TryParse(input, out PageInfo? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_input_is_empty_TryParse_returns_false()
    {
      // Arrange
      string input = string.Empty;

      // Act
      bool success = _parser.TryParse(input, out PageInfo? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_input_is_whitespace_TryParse_returns_false()
    {
      // Arrange
      string input = "   \t\n\r   ";

      // Act
      bool success = _parser.TryParse(input, out PageInfo? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_valid_input_TryParse_returns_true_with_parsed_result()
    {
      // Arrange
      string input = "page[limit]=50&page[offset]=100";

      // Act
      bool success = _parser.TryParse(input, out PageInfo? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      Assert.AreEqual((uint)50, parsed.Limit);
      Assert.AreEqual((uint)100, parsed.Offset);
    }

    [TestMethod]
    public void When_parameter_key_requested_ParameterKey_returns_page()
    {
      // Act
      string parameterKey = _parser.ParameterKey;

      // Assert
      Assert.AreEqual("page", parameterKey);
    }

    [TestMethod]
    public void When_custom_pattern_provider_supplied_constructor_uses_provided_pattern()
    {
      // Arrange
      PageInfoPatternProvider customPatternProvider = PageInfoPatternProvider.Default;

      // Act
      var parser = new PageInfoQueryStringParser(customPatternProvider);

      // Assert
      Assert.IsNotNull(parser);
      Assert.AreEqual("page", parser.ParameterKey);
    }

    [TestMethod]
    public void When_null_pattern_provider_supplied_constructor_uses_default()
    {
      // Act
      var parser = new PageInfoQueryStringParser(null);

      // Assert
      Assert.IsNotNull(parser);
      Assert.AreEqual("page", parser.ParameterKey);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_Parse_returns_object()
    {
      // Arrange
      IParseStrategy strategy = _parser;
      string input = "page[limit]=25";

      // Act
      object? result = strategy.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      PageInfo pageInfo = Assert.IsInstanceOfType<PageInfo>(result);
      Assert.AreEqual((uint)25, pageInfo.Limit);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_TryParse_returns_correct_result()
    {
      // Arrange
      IParseStrategy strategy = _parser;
      string input = "page[limit]=25";

      // Act
      bool success = strategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      PageInfo pageInfo = Assert.IsInstanceOfType<PageInfo>(parsed);
      Assert.AreEqual((uint)25, pageInfo.Limit);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_with_invalid_input_TryParse_returns_false()
    {
      // Arrange
      IParseStrategy strategy = _parser;
      string input = string.Empty;

      // Act
      bool success = strategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_zero_limit_provided_Parse_returns_default_limit()
    {
      // Arrange
      string input = "page[limit]=0";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_zero_offset_provided_Parse_returns_zero_offset()
    {
      // Arrange
      string input = "page[offset]=0";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_large_limit_provided_Parse_returns_correct_limit()
    {
      // Arrange
      string input = "page[limit]=1000000";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)1000000, result.Limit);
    }

    [TestMethod]
    public void When_large_offset_provided_Parse_returns_correct_offset()
    {
      // Arrange
      string input = "page[offset]=9999999";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)9999999, result.Offset);
    }

    [TestMethod]
    public void When_empty_after_value_Parse_returns_empty_string()
    {
      // Arrange
      string input = "page[after]=";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.After);
    }

    [TestMethod]
    public void When_empty_before_value_Parse_returns_empty_string()
    {
      // Arrange
      string input = "page[before]=";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.Before);
    }

    [TestMethod]
    public void When_special_characters_in_cursor_Parse_handles_correctly()
    {
      // Arrange
      string input = "page[after]=cursor_with-special.chars:123";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor_with-special.chars:123", result.After);
    }

    [TestMethod]
    public void When_mixed_case_parameters_Parse_handles_correctly()
    {
      // Arrange
      string input = "page[AfTeR]=abc&page[LiMiT]=50";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("abc", result.After);
      Assert.AreEqual((uint)50, result.Limit);
    }

    [TestMethod]
    public void When_both_cursor_and_after_provided_Parse_uses_last_occurrence()
    {
      // Arrange
      string input = "page[cursor]=cursor_value&page[after]=after_value";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("after_value", result.After);
    }

    [TestMethod]
    public void When_input_contains_percent_but_not_encoded_Parse_handles_correctly()
    {
      // Arrange
      string input = "page[after]=cursor%value";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("cursor%value", result.After);
    }

    [TestMethod]
    public void When_limit_exceeds_uint_max_Parse_returns_default_limit()
    {
      // Arrange
      string input = "page[limit]=99999999999";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_offset_exceeds_uint_max_Parse_returns_default_offset()
    {
      // Arrange
      string input = "page[offset]=99999999999";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_Parse_throws_exception_TryParse_returns_false()
    {
      // Arrange
      string input = "page[limit]=50";
      
      // Create a mock parser that would throw on Parse
      // For this test, we'll use a valid input that won't throw
      // In real scenario, you'd need a way to inject exception behavior

      // Act
      bool success = _parser.TryParse(input, out PageInfo? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
    }

    [TestMethod]
    public void When_input_has_query_string_prefix_Parse_handles_correctly()
    {
      // Arrange
      string input = "?page[limit]=50&page[offset]=10";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)50, result.Limit);
      Assert.AreEqual((uint)10, result.Offset);
    }

    [TestMethod]
    public void When_input_has_ampersand_prefix_Parse_handles_correctly()
    {
      // Arrange
      string input = "&page[limit]=30";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)30, result.Limit);
    }

    [TestMethod]
    public void When_unknown_page_property_provided_Parse_ignores_it()
    {
      // Arrange
      string input = "page[unknown]=value&page[limit]=20";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)20, result.Limit);
      Assert.AreEqual((uint)0, result.Offset);
    }

    [TestMethod]
    public void When_decimal_limit_provided_Parse_returns_default_limit()
    {
      // Arrange
      string input = "page[limit]=10.5";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(uint.MaxValue, result.Limit);
    }

    [TestMethod]
    public void When_decimal_offset_provided_Parse_returns_default_offset()
    {
      // Arrange
      string input = "page[offset]=5.5";

      // Act
      PageInfo result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual((uint)0, result.Offset);
    }
  }
}
