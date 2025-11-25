// -----------------------------------------------------------------------
// <copyright file="FilterQueryStringParserTests.NullOperators.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class FilterQueryStringParserTests
  {
    [TestMethod]
    public void When_filter_with_null_operator_and_no_value_Parse_returns_IsNull_operator()
    {
      // Arrange
      string input = "filter[deletedAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("deletedAt-0", filter.Name);
      Assert.AreEqual("deletedAt", filter.Path);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
      Assert.HasCount(0, filter.Values);
    }

    [TestMethod]
    public void When_filter_with_null_operator_lowercase_Parse_returns_IsNull_operator()
    {
      // Arrange
      string input = "filter[field][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_NULL_operator_uppercase_Parse_returns_IsNull_operator()
    {
      // Arrange
      string input = "filter[field][$NULL]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_null_operator_mixedcase_Parse_returns_IsNull_operator()
    {
      // Arrange
      string input = "filter[field][$NuLl]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_null_operator_and_ignored_value_Parse_returns_IsNull_operator_with_value()
    {
      // Arrange
      string input = "filter[field][$null]=ignoredValue";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("ignoredValue", filter.Values.First());
    }

    [TestMethod]
    public void When_filter_with_notnull_operator_and_no_value_Parse_returns_IsNotNull_operator()
    {
      // Arrange
      string input = "filter[updatedAt][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("updatedAt-0", filter.Name);
      Assert.AreEqual("updatedAt", filter.Path);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
      Assert.HasCount(0, filter.Values);
    }

    [TestMethod]
    public void When_filter_with_notnull_operator_lowercase_Parse_returns_IsNotNull_operator()
    {
      // Arrange
      string input = "filter[field][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_NOTNULL_operator_uppercase_Parse_returns_IsNotNull_operator()
    {
      // Arrange
      string input = "filter[field][$NOTNULL]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_notnull_operator_mixedcase_Parse_returns_IsNotNull_operator()
    {
      // Arrange
      string input = "filter[field][$NoTnUlL]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_filter_with_notnull_operator_and_ignored_value_Parse_returns_IsNotNull_operator_with_value()
    {
      // Arrange
      string input = "filter[field][$notnull]=ignoredValue";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
      Assert.HasCount(1, filter.Values);
      Assert.AreEqual("ignoredValue", filter.Values.First());
    }

    [TestMethod]
    public void When_null_filter_on_nested_path_Parse_returns_correct_path_and_operator()
    {
      // Arrange
      string input = "filter[user][deletedAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("user.deletedAt-0", filter.Name);
      Assert.AreEqual("user.deletedAt", filter.Path);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_notnull_filter_on_nested_path_Parse_returns_correct_path_and_operator()
    {
      // Arrange
      string input = "filter[customer][address][zipCode][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("customer.address.zipCode-0", filter.Name);
      Assert.AreEqual("customer.address.zipCode", filter.Path);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_multiple_null_filters_Parse_creates_unique_filter_names()
    {
      // Arrange
      string input = "filter[deletedAt][$null]=&filter[archivedAt][$null]=&filter[cancelledAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];

      Assert.AreEqual("deletedAt-0", filters[0].Name);
      Assert.AreEqual("deletedAt", filters[0].Path);
      Assert.AreEqual(Operator.IsNull, filters[0].Operator);

      Assert.AreEqual("archivedAt-0", filters[1].Name);
      Assert.AreEqual("archivedAt", filters[1].Path);
      Assert.AreEqual(Operator.IsNull, filters[1].Operator);

      Assert.AreEqual("cancelledAt-0", filters[2].Name);
      Assert.AreEqual("cancelledAt", filters[2].Path);
      Assert.AreEqual(Operator.IsNull, filters[2].Operator);
    }

    [TestMethod]
    public void When_multiple_notnull_filters_Parse_creates_unique_filter_names()
    {
      // Arrange
      string input = "filter[createdAt][$notnull]=&filter[updatedAt][$notnull]=&filter[publishedAt][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];

      Assert.AreEqual("createdAt-0", filters[0].Name);
      Assert.AreEqual(Operator.IsNotNull, filters[0].Operator);

      Assert.AreEqual("updatedAt-0", filters[1].Name);
      Assert.AreEqual(Operator.IsNotNull, filters[1].Operator);

      Assert.AreEqual("publishedAt-0", filters[2].Name);
      Assert.AreEqual(Operator.IsNotNull, filters[2].Operator);
    }

    [TestMethod]
    public void When_null_and_notnull_on_same_field_Parse_creates_two_filters()
    {
      // Arrange
      string input = "filter[status][$null]=&filter[status][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];

      Assert.AreEqual("status-0", filters[0].Name);
      Assert.AreEqual("status", filters[0].Path);
      Assert.AreEqual(Operator.IsNull, filters[0].Operator);

      Assert.AreEqual("status-1", filters[1].Name);
      Assert.AreEqual("status", filters[1].Path);
      Assert.AreEqual(Operator.IsNotNull, filters[1].Operator);
    }

    [TestMethod]
    public void When_null_filter_with_and_conjunction_Parse_creates_group_with_null_operator()
    {
      // Arrange
      string input = "filter[$and][0][deletedAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("deletedAt-and-group", result.Name);
      Assert.AreEqual(Conjunction.And, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("deletedAt-0", filter.Name);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
      Assert.AreEqual("deletedAt-and-group", filter.MemberOf);
    }

    [TestMethod]
    public void When_notnull_filter_with_or_conjunction_Parse_creates_group_with_notnull_operator()
    {
      // Arrange
      string input = "filter[$or][0][email][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("email-or-group", result.Name);
      Assert.AreEqual(Conjunction.Or, result.Conjunction);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("email-0", filter.Name);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
      Assert.AreEqual("email-or-group", filter.MemberOf);
    }

    [TestMethod]
    public void When_null_filter_in_memberOf_group_Parse_assigns_correct_membership()
    {
      // Arrange
      string input = "filter[users][0][deletedAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);
      Assert.AreEqual("users", result.Name);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("deletedAt-0", filter.Name);
      Assert.AreEqual("deletedAt", filter.Path);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
      Assert.AreEqual("users", filter.MemberOf);
    }

    [TestMethod]
    public void When_notnull_filter_in_memberOf_group_Parse_assigns_correct_membership()
    {
      // Arrange
      string input = "filter[activeUsers][0][email][$notnull]=&filter[activeUsers][1][phone][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.AreEqual("activeUsers", result.Name);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];

      Assert.AreEqual("email-0", filters[0].Name);
      Assert.AreEqual(Operator.IsNotNull, filters[0].Operator);
      Assert.AreEqual("activeUsers", filters[0].MemberOf);

      Assert.AreEqual("phone-0", filters[1].Name);
      Assert.AreEqual(Operator.IsNotNull, filters[1].Operator);
      Assert.AreEqual("activeUsers", filters[1].MemberOf);
    }

    [TestMethod]
    public void When_url_encoded_null_operator_Parse_decodes_and_returns_IsNull()
    {
      // Arrange
      string input = "filter%5Bfield%5D%5B%24null%5D=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("field", filter.Path);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_url_encoded_notnull_operator_Parse_decodes_and_returns_IsNotNull()
    {
      // Arrange
      string input = "filter%5Bfield%5D%5B%24notnull%5D=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("field", filter.Path);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_combined_null_notnull_with_other_operators_Parse_creates_all_filters()
    {
      // Arrange
      string input = "filter[deletedAt][$null]=&filter[status][$eq]=active&filter[email][$notnull]=&filter[age][$gt]=18";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(4, result);

      FilterInfo[] filters = [.. result.OfType<FilterInfo>()];

      // Verify null filter
      FilterInfo? nullFilter = filters.FirstOrDefault(f => f.Operator == Operator.IsNull);
      Assert.IsNotNull(nullFilter);
      Assert.AreEqual("deletedAt", nullFilter.Path);

      // Verify equal filter
      FilterInfo? eqFilter = filters.FirstOrDefault(f => f.Operator == Operator.EqualTo);
      Assert.IsNotNull(eqFilter);
      Assert.AreEqual("status", eqFilter.Path);
      Assert.AreEqual("active", eqFilter.Values.First());

      // Verify notnull filter
      FilterInfo? notNullFilter = filters.FirstOrDefault(f => f.Operator == Operator.IsNotNull);
      Assert.IsNotNull(notNullFilter);
      Assert.AreEqual("email", notNullFilter.Path);

      // Verify greater than filter
      FilterInfo? gtFilter = filters.FirstOrDefault(f => f.Operator == Operator.GreaterThan);
      Assert.IsNotNull(gtFilter);
      Assert.AreEqual("age", gtFilter.Path);
      Assert.AreEqual("18", gtFilter.Values.First());
    }

    [TestMethod]
    public void When_null_filter_with_deeply_nested_path_Parse_returns_correct_dotted_path()
    {
      // Arrange
      string input = "filter[customer][billing][address][deletedAt][$null]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("customer.billing.address.deletedAt-0", filter.Name);
      Assert.AreEqual("customer.billing.address.deletedAt", filter.Path);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_notnull_filter_with_dot_notation_path_Parse_preserves_dots()
    {
      // Arrange
      string input = "filter[user.profile.email][$notnull]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result);

      var filter = result.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual("user.profile.email-0", filter.Name);
      Assert.AreEqual("user.profile.email", filter.Path);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }

    [TestMethod]
    public void When_null_filter_TryParse_returns_true_with_correct_filter()
    {
      // Arrange
      string input = "filter[field][$null]=";

      // Act
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      Assert.HasCount(1, parsed);

      var filter = parsed.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNull, filter.Operator);
    }

    [TestMethod]
    public void When_notnull_filter_TryParse_returns_true_with_correct_filter()
    {
      // Arrange
      string input = "filter[field][$notnull]=";

      // Act
      bool success = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsTrue(success);
      Assert.IsNotNull(parsed);
      Assert.HasCount(1, parsed);

      var filter = parsed.First() as FilterInfo;
      Assert.IsNotNull(filter);
      Assert.AreEqual(Operator.IsNotNull, filter.Operator);
    }
  }
}
