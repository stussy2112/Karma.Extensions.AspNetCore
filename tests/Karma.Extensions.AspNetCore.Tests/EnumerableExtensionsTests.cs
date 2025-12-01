// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensionsTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  /// <summary>
  /// Unit tests for the EnumerableExtensions class, covering FilterByQuery and CreateGroupDictionary methods.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [TestClass]
  public partial class EnumerableExtensionsTests
  {
    private List<TestEntity> _testData = null!;

    [TestInitialize]
    public void Setup() => _testData = [
        new TestEntity { Id = 1, Name = "Alice", Value = 10.5 },
        new TestEntity { Id = 2, Name = "Bob", Value = 20.0 },
        new TestEntity { Id = 3, Name = "Charlie", Value = 15.0 }
      ];

    // ========== CreateGroupDictionary Tests ==========

    [TestMethod]
    public void When_source_is_null_CreateGroupDictionary_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity>? source = null;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source!.CreateGroupDictionary(keySelector);
      });
    }

    [TestMethod]
    public void When_keySelector_is_null_CreateGroupDictionary_throws_ArgumentNullException()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string>? keySelector = null;

      // Act & Assert
      _ = Assert.ThrowsExactly<ArgumentNullException>(() =>
      {
        _ = source.CreateGroupDictionary(keySelector!);
      });
    }

    [TestMethod]
    public void When_source_is_empty_CreateGroupDictionary_returns_empty_dictionary()
    {
      // Arrange
      IEnumerable<TestEntity> source = [];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(0, result);
    }

    [TestMethod]
    public void When_source_has_unique_keys_CreateGroupDictionary_creates_single_item_groups()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsTrue(result.ContainsKey("Bob"));
      Assert.IsTrue(result.ContainsKey("Charlie"));

      Assert.HasCount(1, result["Alice"]);
      Assert.HasCount(1, result["Bob"]);
      Assert.HasCount(1, result["Charlie"]);

      Assert.AreEqual(1, result["Alice"][0].Id);
      Assert.AreEqual(2, result["Bob"][0].Id);
      Assert.AreEqual(3, result["Charlie"][0].Id);
    }

    [TestMethod]
    public void When_source_has_duplicate_keys_CreateGroupDictionary_groups_items_correctly()
    {
      // Arrange
      List<TestEntity> sourceWithDuplicates = [
        new TestEntity { Id = 1, Name = "Group1", Value = 10.0 },
        new TestEntity { Id = 2, Name = "Group2", Value = 20.0 },
        new TestEntity { Id = 3, Name = "Group1", Value = 30.0 },
        new TestEntity { Id = 4, Name = "Group2", Value = 40.0 },
        new TestEntity { Id = 5, Name = "Group1", Value = 50.0 }
      ];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = sourceWithDuplicates.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey("Group1"));
      Assert.IsTrue(result.ContainsKey("Group2"));

      Assert.HasCount(3, result["Group1"]);
      Assert.HasCount(2, result["Group2"]);

      var group1Ids = result["Group1"].Select((entity) => entity.Id).ToList();
      var group2Ids = result["Group2"].Select((entity) => entity.Id).ToList();

      int[] expected = [1, 3, 5];
      int[] expected1 = [2, 4];
      CollectionAssert.AreEquivalent(expected, group1Ids);
      CollectionAssert.AreEquivalent(expected1, group2Ids);
    }

    [TestMethod]
    public void When_using_default_equality_comparer_CreateGroupDictionary_uses_default_comparison()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsFalse(result.ContainsKey("alice")); // Case sensitive by default
    }

    [TestMethod]
    public void When_using_custom_equality_comparer_CreateGroupDictionary_uses_custom_comparison()
    {
      // Arrange
      List<TestEntity> sourceWithCaseVariations = [
        new TestEntity { Id = 1, Name = "Alice", Value = 10.0 },
        new TestEntity { Id = 2, Name = "alice", Value = 20.0 },
        new TestEntity { Id = 3, Name = "ALICE", Value = 30.0 }
      ];
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = sourceWithCaseVariations.CreateGroupDictionary(
        keySelector,
        StringComparer.OrdinalIgnoreCase);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(1, result); // All variations should be grouped together

      // The key will be the first one encountered
      string actualKey = result.Keys.First();
      Assert.AreEqual("Alice", actualKey);
      Assert.HasCount(3, result[actualKey]);

      var allIds = result[actualKey].Select((entity) => entity.Id).ToList();
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEquivalent(expected, allIds);
    }

    [TestMethod]
    public void When_using_null_equality_comparer_CreateGroupDictionary_uses_default_comparer()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = source.CreateGroupDictionary(keySelector, null);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.IsTrue(result.ContainsKey("Alice"));
      Assert.IsFalse(result.ContainsKey("alice")); // Case sensitive by default
    }

    [TestMethod]
    public void When_key_selector_returns_different_types_CreateGroupDictionary_works_correctly()
    {
      // Arrange
      IEnumerable<TestEntity> source = _testData;
      Func<TestEntity, int> keySelector = (entity) => entity.Id % 2; // Even/odd grouping

      // Act
      Dictionary<int, List<TestEntity>> result = source.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey(0)); // Even
      Assert.IsTrue(result.ContainsKey(1)); // Odd

      Assert.HasCount(1, result[0]); // Only Bob (Id=2)
      Assert.HasCount(2, result[1]); // Alice (Id=1) and Charlie (Id=3)

      Assert.AreEqual("Bob", result[0][0].Name);
      var oddNames = result[1].Select((entity) => entity.Name).ToList();
      string[] expected = ["Alice", "Charlie"];
      CollectionAssert.AreEquivalent(expected, oddNames);
    }

    [TestMethod]
    public void When_key_selector_returns_null_values_CreateGroupDictionary_handles_gracefully()
    {
      // Arrange
      List<TestEntityWithNullableString> sourceWithNulls = [
        new TestEntityWithNullableString { Id = 1, NullableName = "Group1" },
        new TestEntityWithNullableString { Id = 2, NullableName = null },
        new TestEntityWithNullableString { Id = 3, NullableName = "Group1" },
        new TestEntityWithNullableString { Id = 4, NullableName = null }
      ];

      // Use a non-null constraint on the key type
      Func<TestEntityWithNullableString, string> keySelector = (entity) => entity.NullableName ?? "NULL_GROUP";

      // Act
      Dictionary<string, List<TestEntityWithNullableString>> result = sourceWithNulls.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(2, result);
      Assert.IsTrue(result.ContainsKey("Group1"));
      Assert.IsTrue(result.ContainsKey("NULL_GROUP"));

      Assert.HasCount(2, result["Group1"]);
      Assert.HasCount(2, result["NULL_GROUP"]);
    }

    [TestMethod]
    public void When_source_contains_many_items_CreateGroupDictionary_maintains_insertion_order_within_groups()
    {
      // Arrange
      List<TestEntity> largeSource = [];
      for (int i = 1; i <= 100; i++)
      {
        largeSource.Add(new TestEntity { Id = i, Name = $"Group{i % 10}", Value = i });
      }

      Func<TestEntity, string> keySelector = (entity) => entity.Name;

      // Act
      Dictionary<string, List<TestEntity>> result = largeSource.CreateGroupDictionary(keySelector);

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(10, result); // Group0 through Group9

      // Verify each group has 10 items
      foreach (List<TestEntity> group in result.Values)
      {
        Assert.HasCount(10, group);
      }

      // Verify insertion order within groups (first item should have lower Id than last)
      foreach (List<TestEntity> group in result.Values)
      {
        for (int i = 1; i < group.Count; i++)
        {
          Assert.IsLessThan(group[i].Id, group[i - 1].Id);
        }
      }
    }

    [TestMethod]
    public void When_items_is_null_ConvertEnumerable_generic_returns_default()
    {
      // Arrange
      IEnumerable? items = null;

      // Act
      List<string>? result = items.ConvertEnumerable<List<string>>();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_converting_to_list_ConvertEnumerable_generic_returns_list()
    {
      // Arrange
      string[] items = ["a", "b", "c"];

      // Act
      List<string>? result = items.ConvertEnumerable<List<string>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<string>>(result);
      string[] expected = ["a", "b", "c"];
      CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void When_converting_to_array_ConvertEnumerable_generic_returns_array()
    {
      // Arrange
      List<int> items = [1, 2, 3];

      // Act
      int[]? result = items.ConvertEnumerable<int[]>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<int[]>(result);
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void When_converting_to_ienumerable_ConvertEnumerable_generic_returns_list()
    {
      // Arrange
      int[] items = [1, 2, 3];

      // Act
      IEnumerable<int>? result = items.ConvertEnumerable<IEnumerable<int>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<int>>(result);
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, result.ToArray());
    }

    [TestMethod]
    public void When_converting_to_ilist_ConvertEnumerable_generic_returns_list()
    {
      // Arrange
      string[] items = ["x", "y", "z"];

      // Act
      IList<string>? result = items.ConvertEnumerable<IList<string>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<string>>(result);
      string[] expected = ["x", "y", "z"];
      CollectionAssert.AreEqual(expected, result.ToArray());
    }

    [TestMethod]
    public void When_converting_empty_enumerable_ConvertEnumerable_generic_returns_empty_collection()
    {
      // Arrange
      string[] items = [];

      // Act
      List<string>? result = items.ConvertEnumerable<List<string>>();

      // Assert
      Assert.IsNotNull(result);
      Assert.IsEmpty(result);
    }

    [TestMethod]
    public void When_converting_to_non_enumerable_type_ConvertEnumerable_generic_returns_default()
    {
      // Arrange
      string[] items = ["a", "b"];

      // Act
      string? result = items.ConvertEnumerable<string>();

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_converting_enumerable_with_null_items_ConvertEnumerable_generic_preserves_nulls()
    {
      // Arrange
      string?[] items = ["a", null, "c"];

      // Act
      List<string?>? result = items.ConvertEnumerable<List<string?>>();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("a", result[0]);
      Assert.IsNull(result[1]);
      Assert.AreEqual("c", result[2]);
    }

    [TestMethod]
    public void When_converting_to_hashset_ConvertEnumerable_generic_returns_hashset_implementation()
    {
      // Arrange
      int[] items = [1, 2, 3, 2, 1]; // Duplicates

      // Act
      HashSet<int>? result = items.ConvertEnumerable<HashSet<int>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<HashSet<int>>(result);
      Assert.HasCount(3, result); // Should preserve duplicates since it uses Add method
    }

    // ========== ConvertEnumerable(IEnumerable, Type) Tests ==========

    [TestMethod]
    public void When_items_is_null_ConvertEnumerable_non_generic_returns_null()
    {
      // Arrange
      IEnumerable? items = null;
      Type targetType = typeof(List<string>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_enumerableType_is_not_enumerable_ConvertEnumerable_non_generic_returns_null()
    {
      // Arrange
      string[] items = ["a", "b"];
      Type targetType = typeof(string);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_converting_to_list_type_ConvertEnumerable_non_generic_returns_list()
    {
      // Arrange
      string[] items = ["a", "b", "c"];
      Type targetType = typeof(List<string>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<string>>(result);
      string[] expected = ["a", "b", "c"];
      CollectionAssert.AreEqual(expected, ((List<string>)result).ToArray());
    }

    [TestMethod]
    public void When_converting_to_array_type_ConvertEnumerable_non_generic_returns_array()
    {
      // Arrange
      List<int> items = [1, 2, 3];
      Type targetType = typeof(int[]);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<int[]>(result);
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, (int[])result);
    }

    [TestMethod]
    public void When_converting_to_interface_type_ConvertEnumerable_non_generic_returns_list()
    {
      // Arrange
      int[] items = [1, 2, 3];
      Type targetType = typeof(IList<int>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<int>>(result);
      int[] expected = [1, 2, 3];
      CollectionAssert.AreEqual(expected, ((List<int>)result).ToArray());
    }

    [TestMethod]
    public void When_converting_to_abstract_type_ConvertEnumerable_non_generic_returns_list()
    {
      // Arrange
      string[] items = ["x", "y"];
      Type targetType = typeof(IEnumerable<string>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<IEnumerable<string>>(result);
      string[] expected = ["x", "y"];
      CollectionAssert.AreEqual(expected, ((List<string>)result).ToArray());
    }

    [TestMethod]
    public void When_converting_empty_enumerable_ConvertEnumerable_non_generic_returns_empty_collection()
    {
      // Arrange
      string[] items = [];
      Type targetType = typeof(List<string>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<string>>(result);
      Assert.IsEmpty((List<string>)result);
    }

    [TestMethod]
    public void When_converting_enumerable_with_mixed_types_ConvertEnumerable_non_generic_handles_correctly()
    {
      // Arrange
      object[] items = ["string", 42, true];
      Type targetType = typeof(List<object>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<object>>(result);
      var resultList = (List<object>)result;
      Assert.HasCount(3, resultList);
      Assert.AreEqual("string", resultList[0]);
      Assert.AreEqual(42, resultList[1]);
      Assert.IsTrue((bool?)resultList[2]);
    }

    [TestMethod]
    public void When_converting_large_enumerable_ConvertEnumerable_non_generic_handles_efficiently()
    {
      // Arrange
      int[] items = [.. Enumerable.Range(1, 1000)];
      Type targetType = typeof(List<int>);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<int>>(result);
      var resultList = (List<int>)result;
      Assert.HasCount(1000, resultList);
      Assert.AreEqual(1, resultList[0]);
      Assert.AreEqual(1000, resultList[999]);
    }

    // ========== ConvertEnumerable Error Handling Tests ==========

    [TestMethod]
    public void When_target_type_cannot_be_instantiated_ConvertEnumerable_non_generic_returns_list()
    {
      // Arrange
      string[] items = ["a", "b"];
      Type targetType = typeof(TestAbstractTypeBase);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<List<string>>(result);
      string[] expected = ["a", "b"];
      CollectionAssert.AreEqual(expected, ((List<string>)result).ToArray());
    }

    [TestMethod]
    public void When_target_type_has_no_add_method_ConvertEnumerable_returns_null()
    {
      // Arrange
      string[] items = ["a", "b"];
      Type targetType = typeof(TestTypeWithoutAddMethod);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_target_type_has_multi_parameter_add_ConvertEnumerable_returns_null()
    {
      // Arrange
      string[] items = ["a", "b"];
      Type targetType = typeof(TestTypeWithMultiParameterAdd);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNull(result);
    }

    [TestMethod]
    public void When_add_method_throws_exception_ConvertEnumerable_returns_null()
    {
      // Arrange
      string[] items = ["a", "b"];
      Type targetType = typeof(TestTypeWithThrowingAdd);

      // Act
      IEnumerable? result = items.ConvertEnumerable(targetType);

      // Assert
      Assert.IsNull(result);
    }

    // ========== ConvertEnumerable Edge Cases ==========

    [TestMethod]
    public void When_converting_to_queue_ConvertEnumerable_returns_queue()
    {
      // Arrange
      string[] items = ["first", "second", "third"];

      // Act
      Queue<string>? result = items.ConvertEnumerable<Queue<string>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<Queue<string>>(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("first", result.Dequeue());
      Assert.AreEqual("second", result.Dequeue());
      Assert.AreEqual("third", result.Dequeue());
    }

    [TestMethod]
    public void When_converting_to_stack_ConvertEnumerable_returns_stack()
    {
      // Arrange
      string[] items = ["first", "second", "third"];

      // Act
      Stack<string>? result = items.ConvertEnumerable<Stack<string>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<Stack<string>>(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("third", result.Pop()); // Stack is LIFO
      Assert.AreEqual("second", result.Pop());
      Assert.AreEqual("first", result.Pop());
    }

    [TestMethod]
    public void When_converting_to_linkedlist_ConvertEnumerable_returns_linkedlist()
    {
      // Arrange
      int[] items = [1, 2, 3, 4, 5];

      // Act
      LinkedList<int>? result = items.ConvertEnumerable<LinkedList<int>>();

      // Assert
      Assert.IsNotNull(result);
      _ = Assert.IsInstanceOfType<LinkedList<int>>(result);
      Assert.HasCount(5, result);
      int[] expected = [1, 2, 3, 4, 5];
      CollectionAssert.AreEqual(expected, result.ToArray());
    }

    [TestMethod]
    public void When_converting_with_incompatible_item_types_ConvertEnumerable_handles_gracefully()
    {
      // Arrange - string array to int list (should use object conversion)
      string[] items = ["1", "2", "3"];

      // Act
      List<object>? result = items.ConvertEnumerable<List<object>>();

      // Assert
      Assert.IsNotNull(result);
      Assert.HasCount(3, result);
      Assert.AreEqual("1", result[0]);
      Assert.AreEqual("2", result[1]);
      Assert.AreEqual("3", result[2]);
    }

    // ========== Test Entity Classes ==========

    [ExcludeFromCodeCoverage]
    public abstract class TestAbstractTypeBase : IEnumerable<string>
    {
      public abstract void Add(string item);
      public abstract IEnumerator<string> GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntity
    {
      public int Id
      {
        get; init;
      }

      public string Name { get; init; } = string.Empty;

      public double Value
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithNullableString
    {
      public int Id
      {
        get; init;
      }

      public string? NullableName
      {
        get; init;
      }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithDateTime
    {
      public int Id { get; init; }
      public DateTime Date { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithGuid
    {
      public int Id { get; init; }
      public Guid UniqueId { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithDecimal
    {
      public int Id { get; init; }
      public decimal Price { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithNullableInt
    {
      public int Id { get; init; }
      public int? NullableValue { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public sealed record TestEntityWithLong
    {
      public int Id { get; init; }
      public long LargeValue { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public class TestTypeWithMultiParameterAdd : IEnumerable<string>
    {
      private readonly List<string> _items = [];

      public void Add(string item1, string item2)
      {
        _items.Add(item1);
        _items.Add(item2);
      }

      public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ExcludeFromCodeCoverage]
    public class TestTypeWithThrowingAdd : IEnumerable<string>
    {
      private readonly List<string> _items = [];

      [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Test entity for field access testing")]
      public void Add(string item) => throw new InvalidOperationException("Add method always throws");

      public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ExcludeFromCodeCoverage]
    public class TestTypeWithoutAddMethod : IEnumerable<string>
    {
      private readonly List<string> _items = [];

      public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}