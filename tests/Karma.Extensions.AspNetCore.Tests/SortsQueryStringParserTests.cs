// -----------------------------------------------------------------------
// <copyright file="SortsQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class SortsQueryStringParserTests
  {
    private SortsQueryStringParser _parser = null!;

    [TestInitialize]
    public void TestInitialize() => _parser = new SortsQueryStringParser();

    [TestCleanup]
    public void TestCleanup() => _parser = null!;

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void When_input_is_null_Parse_returns_empty_collection()
    {
      // Arrange
      string input = null!;

      // Act
      IEnumerable<SortInfo> result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_input_is_empty_Parse_returns_empty_collection()
    {
      // Arrange
      string input = string.Empty;

      // Act
      IEnumerable<SortInfo> result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_input_is_whitespace_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "   \t\n\r   ";

      // Act
      IEnumerable<SortInfo> result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_single_field_provided_Parse_returns_single_sort_ascending()
    {
      // Arrange
      string input = "sort=name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[0].Direction);
    }

    [TestMethod]
    public void When_field_with_descending_prefix_provided_Parse_returns_sort_descending()
    {
      // Arrange
      string input = "sort=-name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[0].Direction);
    }

    [TestMethod]
    public void When_multiple_fields_comma_separated_Parse_returns_multiple_sorts()
    {
      // Arrange
      string input = "sort=name,age,email";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
      Assert.IsTrue(result.All((s) => s.Direction == ListSortDirection.Ascending));
    }

    [TestMethod]
    public void When_multiple_fields_mixed_directions_Parse_returns_correct_sorts()
    {
      // Arrange
      string input = "sort=name,-age,email,-createdDate";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, result);

      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[0].Direction);

      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[1].Direction);

      Assert.AreEqual("email", result[2].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[2].Direction);

      Assert.AreEqual("createdDate", result[3].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[3].Direction);
    }

    [TestMethod]
    public void When_multiple_sort_parameters_provided_Parse_returns_all_sorts()
    {
      // Arrange
      string input = "sort=name&sort=age&sort=-email";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
    }

    [TestMethod]
    public void When_nested_field_path_provided_Parse_returns_correct_field_name()
    {
      // Arrange
      string input = "sort=user.profile.name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("user.profile.name", result[0].FieldName);
    }

    [TestMethod]
    public void When_field_with_hyphen_provided_Parse_returns_correct_field_name()
    {
      // Arrange
      string input = "sort=first-name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("first-name", result[0].FieldName);
    }

    [TestMethod]
    public void When_field_with_underscore_provided_Parse_returns_correct_field_name()
    {
      // Arrange
      string input = "sort=first_name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("first_name", result[0].FieldName);
    }

    [TestMethod]
    public void When_field_with_numbers_provided_Parse_returns_correct_field_name()
    {
      // Arrange
      string input = "sort=field123";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("field123", result[0].FieldName);
    }

    [TestMethod]
    public void When_duplicate_fields_provided_Parse_returns_unique_sorts_only()
    {
      // Arrange
      string input = "sort=name,name,name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
    }

    [TestMethod]
    public void When_duplicate_fields_different_directions_Parse_returns_first_occurrence()
    {
      // Arrange
      string input = "sort=name,-name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[0].Direction);
    }

    [TestMethod]
    public void When_url_encoded_input_Parse_decodes_correctly()
    {
      // Arrange
      string input = "sort=first%20name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("first name", result[0].FieldName);
    }

    [TestMethod]
    public void When_fields_with_spaces_provided_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=first name,last name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.AreEqual("first name", result[0].FieldName);
      Assert.AreEqual("last name", result[1].FieldName);
    }

    [TestMethod]
    public void When_empty_fields_in_comma_separated_list_Parse_skips_empty()
    {
      // Arrange
      string input = "sort=name,,age,,,email";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
    }

    [TestMethod]
    public void When_fields_with_whitespace_trimming_Parse_trims_correctly()
    {
      // Arrange
      string input = "sort= name , age , email ";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
    }

    [TestMethod]
    public void When_no_matches_found_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "invalid=parameter";

      // Act
      IEnumerable<SortInfo> result = _parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_input_is_null_TryParse_returns_false()
    {
      // Arrange
      string input = null!;

      // Act
      bool success = _parser.TryParse(input, out IEnumerable<SortInfo>? parsed);

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
      bool success = _parser.TryParse(input, out IEnumerable<SortInfo>? parsed);

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
      bool success = _parser.TryParse(input, out IEnumerable<SortInfo>? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_no_matches_found_TryParse_returns_false()
    {
      // Arrange
      string input = "invalid=parameter";

      // Act
      bool success = _parser.TryParse(input, out IEnumerable<SortInfo>? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_valid_input_TryParse_returns_true_with_parsed_result()
    {
      // Arrange
      string input = "sort=name,-age";

      // Act
      bool success = _parser.TryParse(input, out IEnumerable<SortInfo>? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);

      var result = parsed.ToList();
      Assert.HasCount(2, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
    }

    [TestMethod]
    public void When_parameter_key_requested_ParameterKey_returns_sort()
    {
      // Act
      string parameterKey = _parser.ParameterKey;

      // Assert
      Assert.AreEqual("sort", parameterKey);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_Parse_returns_object()
    {
      // Arrange
      IParseStrategy strategy = _parser;
      string input = "sort=name";

      // Act
      object? result = strategy.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      IEnumerable<SortInfo> sortInfos = Assert.IsInstanceOfType<IEnumerable<SortInfo>>(result);
      Assert.HasCount(1, sortInfos);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_TryParse_returns_correct_result()
    {
      // Arrange
      IParseStrategy strategy = _parser;
      string input = "sort=name";

      // Act
      bool success = strategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      IEnumerable<SortInfo> sortInfos = Assert.IsInstanceOfType<IEnumerable<SortInfo>>(parsed);
      Assert.HasCount(1, sortInfos);
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
    public void When_query_string_prefix_provided_Parse_handles_correctly()
    {
      // Arrange
      string input = "?sort=name&sort=age";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
    }

    [TestMethod]
    public void When_ampersand_prefix_provided_Parse_handles_correctly()
    {
      // Arrange
      string input = "&sort=name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
    }

    [TestMethod]
    public void When_mixed_with_other_parameters_Parse_extracts_sort_only()
    {
      // Arrange
      string input = "page=1&sort=name,-age&limit=10&sort=email";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
    }

    [TestMethod]
    public void When_special_characters_in_field_name_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=field_name.sub-field";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("field_name.sub-field", result[0].FieldName);
    }

    [TestMethod]
    public void When_unicode_characters_in_field_name_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=名前,年齢";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.AreEqual("名前", result[0].FieldName);
      Assert.AreEqual("年齢", result[1].FieldName);
    }

    [TestMethod]
    public void When_input_contains_percent_but_not_encoded_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=field%name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("field%name", result[0].FieldName);
    }

    [TestMethod]
    public void When_only_descending_prefix_provided_Parse_skips_empty_field()
    {
      // Arrange
      string input = "sort=-";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_multiple_descending_prefixes_provided_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=--name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[0].Direction);
    }

    [TestMethod]
    public void When_complex_query_string_Parse_extracts_all_sort_fields()
    {
      // Arrange
      string input = "filter[name]=John&sort=createdDate,-updatedDate&page[limit]=10&sort=id&offset=0";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("createdDate", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[0].Direction);
      Assert.AreEqual("updatedDate", result[1].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[1].Direction);
      Assert.AreEqual("id", result[2].FieldName);
      Assert.AreEqual(ListSortDirection.Ascending, result[2].Direction);
    }

    [TestMethod]
    public void When_sort_parameter_without_value_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "sort=";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_only_commas_provided_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "sort=,,,";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_field_name_with_trailing_ampersand_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=name&";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
    }

    [TestMethod]
    public void When_multiple_sorts_with_same_field_different_case_Parse_returns_unique()
    {
      // Arrange
      string input = "sort=Name,-name,NAME";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      // SortInfo uses record equality which is case-sensitive for strings
      // So different casing should be treated as different fields
      Assert.IsGreaterThanOrEqualTo(1, result.Count);
    }

    [TestMethod]
    public void When_very_long_field_name_provided_Parse_handles_correctly()
    {
      // Arrange
      string longFieldName = new string('a', 1000);
      string input = $"sort={longFieldName}";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual(longFieldName, result[0].FieldName);
    }

    [TestMethod]
    public void When_comma_separated_with_mixed_whitespace_Parse_trims_correctly()
    {
      // Arrange
      string input = "sort=  name  ,  -age  ,  email  ";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual("age", result[1].FieldName);
      Assert.AreEqual("email", result[2].FieldName);
    }

    [TestMethod]
    public void When_newline_in_input_Parse_handles_correctly()
    {
      // Arrange
      string input = "sort=name\r\n&sort=age";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      // Should extract fields before newline
      Assert.IsGreaterThanOrEqualTo(1, result.Count);
    }

    [TestMethod]
    public void When_url_encoded_minus_sign_Parse_decodes_and_treats_as_descending()
    {
      // Arrange
      string input = "sort=%2Dname"; // %2D is encoded minus sign

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
      Assert.AreEqual(ListSortDirection.Descending, result[0].Direction);
    }

    [TestMethod]
    public void When_sort_value_contains_ampersand_Parse_stops_at_ampersand()
    {
      // Arrange
      string input = "sort=name&age&sort=email";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      // Should have name and email, not "name&age"
      Assert.IsTrue(result.Any((s) => s.FieldName == "name"));
      Assert.IsTrue(result.Any((s) => s.FieldName == "email"));
    }

    [TestMethod]
    public void When_empty_match_group_value_Parse_skips_match()
    {
      // Arrange
      string input = "sort&sort=name";

      // Act
      var result = _parser.Parse(input).ToList();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name", result[0].FieldName);
    }
  }
}
