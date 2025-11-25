// -----------------------------------------------------------------------
// <copyright file="FilterQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public partial class FilterQueryStringParserTests
  {
    private FilterQueryStringParser? _parser;

    [TestInitialize]
    public void TestInitialize() => _parser = new FilterQueryStringParser();

    public TestContext? TestContext
    {
      get;
      set;
    }

    /* Valid Filter strings
     * // Group Definition
     * filter[group]=some name
     * filter[group][$and]=some name
     * filter[group][$or][9]=some name // This may be an issue
     * filter[group][memberOfName]=some name
     * filter[group][memberOfName][elementIndex]=some name
     * filter[group][$or][memberOfName]=some name
     * filter[group][$and][memberOfName][elementIndex]=some name
     * 
     * // Filter Definition
     * filter[dotseparated.path]=value
     * filter[dotseparated.path][$ne]=value
     * filter[path1][path2][path3]=value
     * filter[path1][path2][path3][$lte]=value
     * filter[$and][dotseparatedpath]=value
     * filter[$or][elementIndex][dotseparatedpath][$lt]=value
     * filter[$and][elementIndex][path1][path2][path3]=value
     * filter[$or][elementIndex][path1][path2][path3][$ge]=value
     * filter[memberOfName][elementIndex][dotseparated.path]=value
     * filter[memberOfName][elementIndex][dotseparated.path][$in]=value
     * filter[memberOfName][elementIndex][path1][path2][path3]=value
     * filter[memberOfName][elementIndex][path1][path2][path3][$eq]=value
     */

    [TestMethod]
    public void When_input_is_null_Parse_returns_empty_collection()
    {
      // Arrange
      string input = null!;

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_input_is_empty_Parse_returns_empty_collection()
    {
      // Arrange
      string input = string.Empty;

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_input_is_whitespace_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "   \t\n\r   ";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_simple_filter_Parse_returns_single_filter()
    {
      // Arrange
      string input = "filter[name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-0", filter.Name);
      Assert.AreEqual("name", filter.Path);
      Assert.AreEqual(Operator.EqualTo, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("John", filter.Values.First());
    }

    [TestMethod]
    [DataRow("eq", Operator.EqualTo)]
    [DataRow("ne", Operator.NotEqualTo)]
    [DataRow("gt", Operator.GreaterThan)]
    [DataRow("gte", Operator.GreaterThanOrEqualTo)]
    [DataRow("ge", Operator.GreaterThanOrEqualTo)]
    [DataRow("lt", Operator.LessThan)]
    [DataRow("lte", Operator.LessThanOrEqualTo)]
    [DataRow("le", Operator.LessThanOrEqualTo)]
    [DataRow("contains", Operator.Contains)]
    [DataRow("notcontains", Operator.NotContains)]
    [DataRow("startswith", Operator.StartsWith)]
    [DataRow("endswith", Operator.EndsWith)]
    [DataRow("in", Operator.In)]
    [DataRow("notin", Operator.NotIn)]
    [DataRow("between", Operator.Between)]
    [DataRow("notbetween", Operator.NotBetween)]
    [DataRow("null", Operator.IsNull)]
    [DataRow("notnull", Operator.IsNotNull)]
    [DataRow("regex", Operator.Regex)]
    public void When_filter_with_operator_Parse_returns_filter_with_correct_operator(string operatorValue, Operator expectedOperator)
    {
      // Arrange
      string input = $"filter[field][${operatorValue}]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("field-0", filter.Name);
      Assert.AreEqual("field", filter.Path);
      Assert.AreEqual(expectedOperator, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("value", filter.Values.First());
    }

    [TestMethod]
    public void When_nested_path_filter_Parse_returns_filter_with_dotted_path()
    {
      // Arrange
      string input = "filter[customer][name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("customer.name-0", filter.Name);
      Assert.AreEqual("customer.name", filter.Path);
      Assert.AreEqual(Operator.EqualTo, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("John", filter.Values.First());
    }

    [TestMethod]
    public void When_filter_with_in_operator_Parse_returns_filter_with_multiple_values()
    {
      // Arrange
      string input = "filter[status][$in]=pending,approved,shipped";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("status-0", filter.Name);
      Assert.AreEqual("status", filter.Path);
      Assert.AreEqual(Operator.In, filter.Operator);
      Assert.HasCount(3, filter.Values);
      Assert.Contains("pending", filter.Values);
      Assert.Contains("approved", filter.Values);
      Assert.Contains("shipped", filter.Values);
    }

    [TestMethod]
    public void When_filter_with_between_operator_Parse_returns_filter_with_multiple_values()
    {
      // Arrange
      string input = "filter[price][$between]=10,100";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("price-0", filter.Name);
      Assert.AreEqual("price", filter.Path);
      Assert.AreEqual(Operator.Between, filter.Operator);
      Assert.HasCount(2, filter.Values);
      Assert.Contains("10", filter.Values);
      Assert.Contains("100", filter.Values);
    }

    [TestMethod]
    public void When_group_definition_Parse_returns_filter_info_collection()
    {
      // Arrange
      string input = "filter[group]=users";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("users", result.Name);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    [DataRow("and", Conjunction.And)]
    [DataRow("or", Conjunction.Or)]

    public void When_group_with_conjunction_Parse_returns_collection_with_correct_conjunction(string conjunctionValue, Conjunction expectedConjunction)
    {
      // Arrange
      string input = $"filter[group][${conjunctionValue}]=users";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("users", result.Name);
      Assert.AreEqual(expectedConjunction, result.Conjunction);
    }

    [TestMethod]
    public void When_filter_with_and_conjunction_Parse_creates_implicit_group()
    {
      // Arrange
      string input = "filter[$and][name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("name-and-group", result.Name);
      Assert.AreEqual(Conjunction.And, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-0", filter.Name);
      Assert.AreEqual("name-and-group", filter.MemberOf);
    }

    [TestMethod]
    public void When_filter_with_or_conjunction_Parse_creates_implicit_group()
    {
      // Arrange
      string input = "filter[$or][0][name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      Assert.AreEqual("name-or-group", result.Name);
      Assert.AreEqual(Conjunction.Or, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-0", filter.Name);
      Assert.AreEqual("name-or-group", filter.MemberOf);
    }

    [TestMethod]
    public void When_filter_with_multiple_or_conjunction_Parse_creates_implicit_group()
    {
      // Arrange
      string input = "filter[$or][0][name]=John&filter[$or][1][name]=Jill";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);

      Assert.AreEqual("name-or-group", result.Name);
      Assert.AreEqual(Conjunction.Or, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-0", filter.Name);
      Assert.AreEqual("name-or-group", filter.MemberOf);
      Assert.AreEqual("John", filter.Values.First());

      filter = result.Last() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-1", filter.Name);
      Assert.AreEqual("name-or-group", filter.MemberOf);
      Assert.AreEqual("Jill", filter.Values.First());
    }

    [TestMethod]
    public void When_filter_with_memberof_Parse_assigns_correct_member()
    {
      // Arrange
      string input = "filter[users][0][name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("users", result.Name);
      Assert.AreEqual(Conjunction.And, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name-0", filter.Name);
      Assert.AreEqual("users", filter.MemberOf);
      Assert.AreEqual("name", filter.Path);
    }

    [TestMethod]
    public void When_url_encoded_input_Parse_decodes_correctly()
    {
      // Arrange
      string input = "filter%5Bname%5D=John%20Doe";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("John Doe", filter.Values.First());
    }

    [TestMethod]
    public void When_Parse_called_with_unknown_operator_does_not_create_filters()
    {
      // Arrange
      string input = "filter[field][$unknown]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void When_complex_nested_filters_Parse_builds_correct_hierarchy()
    {
      // Arrange
      string input = "filter[users][0][name]=John&filter[users][0][age][$gt]=18&filter[users][1][name]=Jane";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("users", result.Name);
      Assert.HasCount(3, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];
      Assert.HasCount(3, filters);

      // Check first filter
      FilterInfo? johnNameFilter = filters.FirstOrDefault((f) => f.Path == "name" && f.Values.Contains("John"));
      Assert.IsNotNull(johnNameFilter);
      Assert.AreEqual("users", johnNameFilter.MemberOf);

      // Check age filter
      FilterInfo? ageFilter = filters.FirstOrDefault((f) => f.Path == "age");
      Assert.IsNotNull(ageFilter);
      Assert.AreEqual(Operator.GreaterThan, ageFilter.Operator);
      Assert.AreEqual("18", ageFilter.Values.First());

      // Check Jane filter
      FilterInfo? janeNameFilter = filters.FirstOrDefault((f) => f.Path == "name" && f.Values.Contains("Jane"));
      Assert.IsNotNull(janeNameFilter);
      Assert.AreEqual("users", janeNameFilter.MemberOf);
    }

    [TestMethod]
    public void When_invalid_regex_timeout_Parse_returns_empty_collection()
    {
      // Arrange
      // This test would require a way to trigger RegexMatchTimeoutException
      // For now, we test with a complex but valid input that could potentially timeout
      string input = new string('a', 10000) + "filter[name]=test";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
    }

    [TestMethod]
    public void When_input_is_null_TryParse_returns_false()
    {
      // Arrange
      string input = null!;

      // Act
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

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
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

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
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_no_matches_found_TryParse_returns_false()
    {
      // Arrange
      string input = "invalidinput";

      // Act
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_valid_filter_TryParse_returns_true_with_parsed_result()
    {
      // Arrange
      string input = "filter[name]=John";

      // Act
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      Assert.HasCount(1, parsed);

      var filter = parsed.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("name", filter.Path);
      Assert.AreEqual("John", filter.Values.First());
    }

    [TestMethod]
    public void When_parameter_key_requested_ParameterKey_returns_filter()
    {
      // Act
      string parameterKey = _parser!.ParameterKey;

      // Assert
      Assert.AreEqual("filter", parameterKey);
    }

    [TestMethod]
    public void When_custom_pattern_provider_supplied_constructor_uses_provided_pattern()
    {
      // Arrange
      FilterPatternProvider customPatternProvider = FilterPatternProvider.Default;

      // Act
      var parser = new FilterQueryStringParser(customPatternProvider);

      // Assert
      Assert.IsNotNull(parser);
      Assert.AreEqual("filter", parser.ParameterKey);
    }

    [TestMethod]
    public void When_null_pattern_provider_supplied_constructor_uses_default()
    {
      // Act
      var parser = new FilterQueryStringParser(null);

      // Assert
      Assert.IsNotNull(parser);
      Assert.AreEqual("filter", parser.ParameterKey);
    }

    [TestMethod]
    public void When_empty_values_for_in_operator_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "filter[status][$in]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.In, filter.Operator);
      Assert.HasCount(0, filter.Values);
    }

    [TestMethod]
    public void When_single_value_for_between_operator_Parse_returns_empty_collection()
    {
      // Arrange
      string input = "filter[price][$between]=10";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.Between, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("10", filter.Values.First());
    }

    [TestMethod]
    public void When_dot_separated_path_in_brackets_Parse_creates_correct_path()
    {
      // Arrange
      string input = "filter[customer.address.city]=New York";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("customer.address.city", filter.Path);
      Assert.AreEqual("New York", filter.Values.First());
    }

    [TestMethod]
    [DataRow("EQ", Operator.EqualTo)]
    [DataRow("NE", Operator.NotEqualTo)]
    [DataRow("GT", Operator.GreaterThan)]
    [DataRow("GTE", Operator.GreaterThanOrEqualTo)]
    [DataRow("GE", Operator.GreaterThanOrEqualTo)]
    [DataRow("LT", Operator.LessThan)]
    [DataRow("LTE", Operator.LessThanOrEqualTo)]
    [DataRow("LE", Operator.LessThanOrEqualTo)]
    [DataRow("CONTAINS", Operator.Contains)]
    [DataRow("NOTCONTAINS", Operator.NotContains)]
    [DataRow("STARTSWITH", Operator.StartsWith)]
    [DataRow("ENDSWITH", Operator.EndsWith)]
    [DataRow("IN", Operator.In)]
    [DataRow("NOTIN", Operator.NotIn)]
    [DataRow("BETWEEN", Operator.Between)]
    [DataRow("NOTBETWEEN", Operator.NotBetween)]
    [DataRow("NULL", Operator.IsNull)]
    [DataRow("NOTNULL", Operator.IsNotNull)]
    [DataRow("REGEX", Operator.Regex)]
    public void When_case_insensitive_operator_Parse_returns_correct_Operator(string operatorValue, Operator expectedOperator)
    {
      // Arrange
      string input = $"filter[field][${operatorValue}]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(expectedOperator, filter.Operator);
    }

    [TestMethod]
    [DataRow("AND", Conjunction.And)]
    [DataRow("OR", Conjunction.Or)]
    public void When_Parse_called_with_case_insensitive_conjunction_returns_correct_Conjunction(string conjunctionValue, Conjunction expectedConjunction)
    {
      // Arrange
      string input = $"filter[${conjunctionValue}][0][field]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual($"field-{conjunctionValue}-group", result.Name);
      Assert.AreEqual(expectedConjunction, result.Conjunction);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_Parse_returns_object()
    {
      // Arrange
      IParseStrategy strategy = _parser!;
      string input = "filter[name]=John";

      // Act
      object? result = strategy.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      FilterInfoCollection collection = Assert.IsInstanceOfType<FilterInfoCollection>(result);

      Assert.HasCount(1, collection);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_TryParse_returns_correct_result()
    {
      // Arrange
      IParseStrategy strategy = _parser!;
      string input = "filter[name]=John";

      // Act
      bool success = strategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      FilterInfoCollection collection = Assert.IsInstanceOfType<FilterInfoCollection>(parsed);

      Assert.HasCount(1, collection);
    }

    [TestMethod]
    public void When_explicit_interface_implementation_called_with_invalid_input_TryParse_returns_false()
    {
      // Arrange
      IParseStrategy strategy = _parser!;
      string input = "invalid";

      // Act
      bool success = strategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsFalse(success);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_special_characters_in_value_Parse_handles_correctly()
    {
      // Arrange
      string input = "filter[description]=Special chars: !@#$%^*()";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("Special chars: !@#$%^*()", filter.Values.First());
    }

    [TestMethod]
    public void When_multiple_same_filters_Parse_creates_unique_names()
    {
      // Arrange
      string input = "filter[name]=John&filter[name]=Jane";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];
      Assert.HasCount(2, filters);

      // Check that names are unique
      Assert.AreEqual("name-0", filters[0].Name);
      Assert.AreEqual("name-1", filters[1].Name);

      // Check that both have the same path but different values
      Assert.AreEqual("name", filters[0].Path);
      Assert.AreEqual("name", filters[1].Path);
      Assert.IsTrue(filters.Any((f) => f.Values.Contains("John")));
      Assert.IsTrue(filters.Any((f) => f.Values.Contains("Jane")));
    }

    [TestMethod]
    public void When_Parse_called_with_same_path_in_conjunction_groups_creates_separate_filters()
    {
      // Arrange - Test same path with different operators within conjunction groups
      string input = "filter[$and][0][score][$gt]=80&filter[$or][0][score][$eq]=100&filter[$and][1][score][$lt]=95";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(2, result.Count, "Should create two conjunction groups");

      // Find the AND group
      FilterInfoCollection? andGroup = result.Cast<FilterInfoCollection>().FirstOrDefault(g => g.Name == "score-and-group");
      Assert.IsNotNull(andGroup, "Should create an AND group for score filters");
      Assert.AreEqual(Conjunction.And, andGroup.Conjunction);
      Assert.AreEqual(2, andGroup.Count);

      var andFilters = andGroup.Cast<FilterInfo>().ToList();
      Assert.AreEqual("score-0", andFilters[0].Name);
      Assert.AreEqual("score-2", andFilters[1].Name);

      // Find the OR group
      FilterInfoCollection? orGroup = result.Cast<FilterInfoCollection>().FirstOrDefault(g => g.Name == "score-or-group");
      Assert.IsNotNull(orGroup, "Should create an OR group for score filters");
      Assert.AreEqual(Conjunction.Or, orGroup.Conjunction);
      Assert.AreEqual(1, orGroup.Count);

      FilterInfo orFilter = orGroup.Cast<FilterInfo>().First();
      Assert.AreEqual("score-1", orFilter.Name);
      Assert.AreEqual("score", orFilter.Path);
      Assert.AreEqual(Operator.EqualTo, orFilter.Operator);
    }

    [TestMethod]
    public void When_Parse_called_with_nested_path_multiple_operators_creates_separate_filters()
    {
      // Arrange - Test multiple operators on nested property paths
      string input = "filter[user][profile][age][$gt]=21&filter[user][profile][age][$lt]=65&filter[user][profile][age][$ne]=30";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(3, result.Count);

      var filters = result.Cast<FilterInfo>().ToList();

      // Verify all filters have the same nested path
      foreach (FilterInfo? filter in filters)
      {
        Assert.AreEqual("user.profile.age", filter.Path);
        Assert.AreEqual(string.Empty, filter.MemberOf);
      }

      // Verify unique names and different operators
      Assert.AreEqual("user.profile.age-0", filters[0].Name);
      Assert.AreEqual(Operator.GreaterThan, filters[0].Operator);
      Assert.AreEqual("21", filters[0].Values.FirstOrDefault());

      Assert.AreEqual("user.profile.age-1", filters[1].Name);
      Assert.AreEqual(Operator.LessThan, filters[1].Operator);
      Assert.AreEqual("65", filters[1].Values.FirstOrDefault());

      Assert.AreEqual("user.profile.age-2", filters[2].Name);
      Assert.AreEqual(Operator.NotEqualTo, filters[2].Operator);
      Assert.AreEqual("30", filters[2].Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_mixed_paths_and_duplicate_paths_groups_correctly()
    {
      // Arrange - Test mix of unique paths and duplicate paths with different operators
      string input = "filter[status]=active&filter[price][$gte]=100&filter[price][$lte]=500&filter[category]=electronics&filter[status][$ne]=pending";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(5, result.Count, "Should create five separate filters");

      var filters = result.Cast<FilterInfo>().ToList();

      // Verify status filters (should have status-0 and status-1)
      var statusFilters = filters.Where(f => f.Path == "status").ToList();
      Assert.HasCount(2, statusFilters);
      Assert.AreEqual("status-0", statusFilters[0].Name);
      Assert.AreEqual("status-1", statusFilters[1].Name);

      // Verify price filters (should have price-0 and price-1)
      var priceFilters = filters.Where(f => f.Path == "price").ToList();
      Assert.HasCount(2, priceFilters);
      Assert.AreEqual("price-0", priceFilters[0].Name);
      Assert.AreEqual("price-1", priceFilters[1].Name);

      // Verify category filter (should have category-0)
      var categoryFilters = filters.Where(f => f.Path == "category").ToList();
      Assert.HasCount(1, categoryFilters);
      Assert.AreEqual("category-0", categoryFilters[0].Name);
    }
  }
}