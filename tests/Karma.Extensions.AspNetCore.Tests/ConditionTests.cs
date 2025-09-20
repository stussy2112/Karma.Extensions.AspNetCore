using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class ConditionTests
  {
    [TestMethod]
    public void When_different_values_adds_all_condition()
    {
      string path = nameof(GenericParameterHelper.Data);
      var target = new ConditionCollection
      {
        new Condition(nameof(GenericParameterHelper.Data), path, 1),
        new Condition("otherProp", path, 1)
      };

      Assert.AreEqual(2, target.Count);
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_existing_condition_group_adds_to_existing_group()
    {
      string path = nameof(GenericParameterHelper.Data);
      var target = new ConditionCollection("TestGroup")
      {
        new Condition(nameof(GenericParameterHelper.Data), path, 1),
        new Condition("otherProp", path, 1),
        new ConditionCollection("TestGroup")
        {
          new Condition(nameof(GenericParameterHelper.Data), path, 2),
          new Condition(nameof(GenericParameterHelper.Data), path, 3)
        }
      };

      Assert.AreEqual(4, target.Count);
      Assert.AreEqual(0, target.OfType<ConditionCollection>().Count());
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_existing_condition_sub_group_adds_to_existing_group()
    {
      string path = nameof(GenericParameterHelper.Data);
      var subGroup = new ConditionCollection("TestGroup");

      var target = new ConditionCollection
      {
        new Condition(nameof(GenericParameterHelper.Data), path, 1),
        new Condition("otherProp", path, 1),
        subGroup,
        new ConditionCollection("TestGroup")
      {
        new Condition(nameof(GenericParameterHelper.Data), path, values: 2),
        new Condition(nameof(GenericParameterHelper.Data), path, values: 3)
      }
      };

      Assert.AreEqual(3, target.Count);
      Assert.AreEqual(2, subGroup.Count);
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_multiple_defined_ConditionCollections_filter_parses_condition_group()
    {
      /* NOTE: filter[first-name-filter][condition][path]=field_first_name
               &filter[first-name-filter][condition][operator]=%3D  <- encoded "="
               &filter[first-name-filter][condition][value]=Janis
      */

      string filter = @"filter[first-name-Janis-filter][condition][path]=field_first_name
              &filter[first-name-Janis-filter][condition][operator]=%3C%3E
              &filter[first-name-Janis-filter][condition][value]=Janis
              &filter[last-name-Joplin-filter][condition][path]=field_last_name
              &filter[last-name-Joplin-filter][condition][operator]=startswith
              &filter[last-name-Joplin-filter][condition][value]=Joplin
              &filter[last-name-Joplin-filter][condition][memberOf]=name-group
              &filter[first-group][group][conjunction]=OR
              &filter[name-group][group][conjunction]=AND";

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);

      Assert.AreEqual(3, actual.Count());
      Assert.AreEqual("first-group", actual.OfType<ConditionCollection>().First().Name);
      Assert.AreEqual(Conjunction.Or, actual.OfType<ConditionCollection>().First().Conjunction);
      Assert.AreEqual("name-group", actual.OfType<ConditionCollection>().Last().Name);
      Assert.AreEqual(Conjunction.And, actual.OfType<ConditionCollection>().Last().Conjunction);

      Assert.AreEqual("first-name-Janis-filter", actual.Last().Name);
      Assert.AreEqual("field_first_name", actual.Last().Path);
      Assert.AreEqual(Operator.NotEqualTo, actual.Last().Operator);
      Assert.AreEqual(1, actual.Last().Value.Count);
      CollectionAssert.Contains(actual.Last().Value.ToList(), "Janis");

      Assert.AreEqual("last-name-Joplin-filter", actual.OfType<ConditionCollection>().Last().First().Name);
      Assert.AreEqual("field_last_name", actual.OfType<ConditionCollection>().Last().First().Path);
      Assert.AreEqual(Operator.StartsWith, actual.OfType<ConditionCollection>().Last().First().Operator);
      Assert.AreEqual(1, actual.OfType<ConditionCollection>().Last().First().Value.Count);
      CollectionAssert.Contains(actual.OfType<ConditionCollection>().Last().First().Value.ToList(), "Joplin");
    }

    [TestMethod]
    public void When_multiple_defined_nested_ConditionCollections_filter_parses_condition_group()
    {
      /* NOTE: filter[first-name-filter][condition][path]=field_first_name
               &filter[first-name-filter][condition][operator]=%3D  <- encoded "="
               &filter[first-name-filter][condition][value]=Janis
      */

      // 2 conditions, 3 groups
      string filter = @"filter[first-name-Janis-filter][condition][path]=field_first_name
              &filter[first-name-Janis-filter][condition][operator]=%3C%3E
              &filter[first-name-Janis-filter][condition][value]=Janis
              &filter[last-name-Joplin-filter][condition][path]=field_last_name
              &filter[last-name-Joplin-filter][condition][operator]=startswith
              &filter[last-name-Joplin-filter][condition][value]=Joplin
              &filter[last-name-Joplin-filter][condition][memberOf]=name-group
              &filter[nameSub-group][group][memberOf]=first-group
              &filter[first-group][group][conjunction]=AND
              &filter[first-group][group][memberOf]=name-group
              &filter[name-group][group][conjunction]=OR";

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);

      // "root" should contain 1 condition group and 1 condition
      Assert.AreEqual(2, actual.Count());
      Assert.IsInstanceOfType(actual, typeof(ConditionCollection));
      Assert.AreEqual("root", ((ConditionCollection)actual).Name);
      Assert.AreEqual(Conjunction.And, ((ConditionCollection)actual).Conjunction);

      Assert.AreEqual("first-name-Janis-filter", actual.Skip(1).First().Name);
      Assert.AreEqual("root", actual.Skip(1).First().MemberOf);
      Assert.AreEqual("field_first_name", actual.Skip(1).First().Path);
      Assert.AreEqual(Operator.NotEqualTo, actual.Skip(1).First().Operator);
      Assert.AreEqual(1, actual.Skip(1).First().Value.Count);
      CollectionAssert.Contains(actual.Skip(1).First().Value.ToList(), "Janis");

      // the sub group should contain 2 conditions
      ConditionCollection subGroup = actual.OfType<ConditionCollection>().FirstOrDefault();

      Assert.IsNotNull(subGroup);
      Assert.AreEqual(2, subGroup.Count);
      Assert.AreEqual("name-group", subGroup.Name);
      Assert.AreEqual(Conjunction.Or, subGroup.Conjunction);

      Assert.AreEqual("first-group", subGroup.OfType<ConditionCollection>().First().Name);
      Assert.AreEqual(Conjunction.And, subGroup.OfType<ConditionCollection>().First().Conjunction);

      Assert.AreEqual("last-name-Joplin-filter", subGroup.Skip(1).First().Name);
      Assert.AreEqual("field_last_name", subGroup.Skip(1).First().Path);
      Assert.AreEqual(Operator.StartsWith, subGroup.Skip(1).First().Operator);
      Assert.AreEqual(1, subGroup.Skip(1).First().Value.Count);
      CollectionAssert.Contains(subGroup.Skip(1).First().Value.ToList(), "Joplin");
    }

    [TestMethod]
    public void When_multiple_short_filter_parses_conditions()
    {
      string filter = "filter[field_first_name]=Janis&filter[field_last_name]=Smith";

      //var filter = "filter[field_first_name][value]=Janis"; // Create a condition with no name, field name, equals operator and value

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);
      Assert.AreEqual(2, actual.Count());
      Assert.AreEqual("field_first_name", actual.First().Name);
      Assert.AreEqual("field_first_name", actual.First().Path);
      Assert.AreEqual(Operator.EqualTo, actual.First().Operator);
      Assert.AreEqual(1, actual.First().Value.Count);
      CollectionAssert.Contains(actual.First().Value.ToList(), "Janis");
      Assert.AreEqual("field_last_name", actual.Last().Name);
      Assert.AreEqual("field_last_name", actual.Last().Path);
      Assert.AreEqual(Operator.EqualTo, actual.Last().Operator);
      Assert.AreEqual(1, actual.Last().Value.Count);
      CollectionAssert.Contains(actual.Last().Value.ToList(), "Smith");
    }

    [TestMethod]
    public void When_multiple_short_filters_parses_condition_group()
    {
      // Group = name 2 conditions condition[0] = first -> FirstName == John condition[1] = last -> LastName == Smith
      string filter = "filter[name][group][conjunction]=Or&filter[first-name][FirstName]=John&filter[first-name][memberOf]=name&filter[last-name][LastName]=Smith&filter[last-name][memberOf]=name";

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);

      Assert.IsNotNull(actual);
      Assert.AreEqual(1, actual.Count());
      Assert.IsInstanceOfType(actual, typeof(ConditionCollection));
      Assert.AreEqual("root", ((ConditionCollection)actual).Name);
      Assert.AreEqual(Conjunction.And, ((ConditionCollection)actual).Conjunction);

      ConditionCollection nameGroup = actual.OfType<ConditionCollection>().FirstOrDefault();

      Assert.IsNotNull(nameGroup);
      Assert.AreEqual(2, nameGroup.Count);
      Assert.AreEqual("name", nameGroup.Name);

      Assert.AreEqual("first-name", nameGroup.First().Name);
      Assert.AreEqual("name", nameGroup.First().MemberOf);
      Assert.AreEqual(Operator.EqualTo, nameGroup.First().Operator);
      CollectionAssert.Contains(nameGroup.First().Value.ToList(), "John");

      Assert.AreEqual("last-name", nameGroup.Last().Name);
      Assert.AreEqual("name", nameGroup.Last().MemberOf);
      Assert.AreEqual(Operator.EqualTo, nameGroup.Last().Operator);
      CollectionAssert.Contains(nameGroup.Last().Value.ToList(), "Smith");
    }

    [TestMethod]
    public void When_no_condition_group_adds_to_new_group()
    {
      string path = nameof(GenericParameterHelper.Data);
      var target = new ConditionCollection("TestGroup")
      {
        new Condition(nameof(GenericParameterHelper.Data), path, 1),
        new Condition("otherProp", path, 1),
        new Condition(nameof(GenericParameterHelper.Data), path, values: 2) { MemberOf = "NewTestGroup" }
      };

      Assert.AreEqual(3, target.Count);
      Assert.AreEqual(1, target.OfType<ConditionCollection>().Count());
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_same_memberof_adds_to_same_group()
    {
      string path = nameof(GenericParameterHelper.Data);
      var target = new ConditionCollection("TestGroup")
      {
        new Condition(nameof(GenericParameterHelper.Data), path, 1),
        new Condition("otherProp", path, 1),
        new Condition(nameof(GenericParameterHelper.Data), path, values: 2) { MemberOf = "TestGroup" }
      };

      Assert.AreEqual(3, target.Count);
      Assert.AreEqual(0, target.OfType<ConditionCollection>().Count());
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_same_values_adds_one_condition()
    {
      string path = nameof(GenericParameterHelper.Data);
      var target = new ConditionCollection
      {
        new Condition(nameof(GenericParameterHelper.Data), path, new [] { new GenericParameterHelper(1) }),
        new Condition(nameof(GenericParameterHelper.Data), path, new GenericParameterHelper(1))
      };

      Assert.AreEqual(1, target.Count);
      Assert.IsTrue(target.All((c) => target.Name.Equals(c.MemberOf, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void When_short_condition_filter_parses_condition_group()
    {
      string filter = "filter[field_first_name][operator]=in&filter[field_first_name]=Janis,James";

      //var filter = "filter[field_first_name][value]=Janis"; // Create a condition with no name, field name, equals operator and value

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);

      Assert.AreEqual(1, actual.Count());
      Assert.IsInstanceOfType(actual, typeof(ConditionCollection));
      Assert.AreEqual("field_first_name", actual.First().Name);
      Assert.AreEqual(Operator.In, actual.First().Operator);
      Assert.AreEqual(2, actual.Last().Value.Count);
      CollectionAssert.Contains(actual.First().Value.ToList(), "Janis");
      CollectionAssert.Contains(actual.First().Value.ToList(), "James");
    }

    [TestMethod]
    public void When_valid_condition_filter_parses_condition_group()
    {
      string filter = "filter[field_first_name][operator]=%3C%3E&filter[field_first_name][value]=Janis";

      //var filter = "filter[field_first_name][value]=Janis"; // Create a condition with no name, field name, equals operator and value

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);
      Assert.AreEqual(1, actual.Count());
      Assert.AreEqual("field_first_name", actual.First().Name);
      Assert.AreEqual("field_first_name", actual.First().Path);
      Assert.AreEqual(Operator.NotEqualTo, actual.First().Operator);
      Assert.AreEqual(1, actual.First().Value.Count);
      CollectionAssert.Contains(actual.First().Value.ToList(), "Janis");
    }

    [TestMethod]
    public void When_valid_multiple_condition_filter_parses_condition_group()
    {
      // Group = and-group 2 conditions condition[0] = first -> FirstName == John condition[1] = last -> LastName == Smith
      string filter = @"filter[first][condition][FirstName]=John&filter[first][condition][memberOf]=and-group&filter[last][condition][LastName]=Smith&filter[last][condition][memberOf]=and-group";

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);
      Assert.IsNotNull(actual);
      Assert.AreEqual(1, actual.Count());
      Assert.IsInstanceOfType(actual, typeof(ConditionCollection));
      Assert.AreEqual("root", ((ConditionCollection)actual).Name);
      Assert.AreEqual(Conjunction.And, ((ConditionCollection)actual).Conjunction);

      ConditionCollection subGroup = actual.OfType<ConditionCollection>().FirstOrDefault();

      Assert.IsNotNull(subGroup);
      Assert.AreEqual(2, subGroup.Count);
      Assert.AreEqual("and-group", subGroup.Name);

      Assert.AreEqual("first", subGroup.First().Name);
      Assert.AreEqual("and-group", subGroup.First().MemberOf);
      Assert.AreEqual(Operator.EqualTo, subGroup.First().Operator);
      CollectionAssert.Contains(subGroup.First().Value.ToList(), "John");

      Assert.AreEqual("last", subGroup.Last().Name);
      Assert.AreEqual("and-group", subGroup.Last().MemberOf);
      Assert.AreEqual(Operator.EqualTo, subGroup.Last().Operator);
      CollectionAssert.Contains(subGroup.Last().Value.ToList(), "Smith");
    }

    [TestMethod]
    public void When_valid_short_condition_filter_parses_condition_group()
    {
      string filter = "filter[field_first_name]=Janis";

      //var filter = "filter[field_first_name][value]=Janis"; // Create a condition with no name, field name, equals operator and value

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);
      Assert.AreEqual(1, actual.Count());
      Assert.AreEqual("field_first_name", actual.First().Name);
      Assert.AreEqual("field_first_name", actual.First().Path);
      Assert.AreEqual(Operator.EqualTo, actual.First().Operator);
      Assert.AreEqual(1, actual.First().Value.Count);
      CollectionAssert.Contains(actual.First().Value.ToList(), "Janis");
    }

    [TestMethod]
    public void When_valid_short_condition_filter_with_operator_parses_condition_group()
    {
      string filter = "filter[field_first_name]=Janis&filter[field_first_name][operator]=%3C%3E";

      //var filter = "filter[field_first_name][value]=Janis"; // Create a condition with no name, field name, equals operator and value

      var target = new FilterQueryStringParser();
      IEnumerable<Condition> actual = target.Parse(filter);
      Assert.AreEqual(1, actual.Count());
      Assert.AreEqual("field_first_name", actual.First().Name);
      Assert.AreEqual(Operator.NotEqualTo, actual.First().Operator);
      Assert.AreEqual("field_first_name", actual.First().Path);
      Assert.AreEqual(1, actual.First().Value.Count);
      CollectionAssert.Contains(actual.First().Value.ToList(), "Janis");
    }
  }
}