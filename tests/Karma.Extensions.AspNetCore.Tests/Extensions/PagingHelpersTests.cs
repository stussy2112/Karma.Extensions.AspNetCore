// -----------------------------------------------------------------------
// <copyright file="PagingHelpersTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Karma.Extensions.AspNetCore.Tests.Extensions
{
  [ExcludeFromCodeCoverage]
  [TestClass]
  public class PagingHelpersTests
  {
    private sealed class SimpleEntity
    {
      public string? Name { get; set; }
    }

    private sealed class FieldEntity
    {
      public int Count;
    }

    private sealed class Inner
    {
      public string? Value { get; set; }
    }

    private sealed class Outer
    {
      public Inner? Inner { get; set; }
    }

    [TestMethod]
    public void When_PropertyName_IsNullOrWhitespace_GetPropertySelectorExpression_ReturnsNull()
    {
      Expression<Func<object, object?>>? expr1 = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>(null!);
      Assert.IsNull(expr1);

      Expression<Func<object, object?>>? expr2 = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>(string.Empty);
      Assert.IsNull(expr2);

      Expression<Func<object, object?>>? expr3 = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("   ");
      Assert.IsNull(expr3);
    }

    [TestMethod]
    public void When_PropertyExists_GetPropertySelectorExpression_ReturnsSelectorThatReadsProperty()
    {
      Expression<Func<object, object?>>? expr = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("Name");
      Assert.IsNotNull(expr);

      Func<object, object?> func = expr!.Compile();
      var input = new SimpleEntity { Name = "Alice" };
      object? result = func(input);
      Assert.AreEqual("Alice", result);
    }

    [TestMethod]
    public void When_PropertyName_CaseInsensitive_GetPropertySelectorExpression_LooksUpSuccessfully()
    {
      Expression<Func<object, object?>>? exprLower = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("name");
      Expression<Func<object, object?>>? exprMixed = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("NaMe");

      Assert.IsNotNull(exprLower);
      Assert.IsNotNull(exprMixed);

      Func<object, object?> func = exprLower!.Compile();
      var input = new SimpleEntity { Name = "Bob" };
      Assert.AreEqual("Bob", func(input));
    }

    [TestMethod]
    public void When_FieldExists_GetPropertySelectorExpression_ReturnsSelectorThatReadsField()
    {
      Expression<Func<object, object?>>? expr = PagingHelpers.GetPropertySelectorExpression<FieldEntity>("Count");
      Assert.IsNotNull(expr);

      Func<object, object?> func = expr!.Compile();
      var input = new FieldEntity { Count = 42 };
      object? result = func(input);
      Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void When_NestedPropertyExists_GetPropertySelectorExpression_ReturnsSelectorThatReadsNestedValue()
    {
      Expression<Func<object, object?>>? expr = PagingHelpers.GetPropertySelectorExpression<Outer>("Inner.Value");
      Assert.IsNotNull(expr);

      Func<object, object?> func = expr!.Compile();
      var input = new Outer { Inner = new Inner { Value = "X" } };
      object? result = func(input);
      Assert.AreEqual("X", result);
    }

    [TestMethod]
    public void When_PropertyPathContainsWhitespace_GetPropertySelectorExpression_TrimsSegmentsAndResolves()
    {
      Expression<Func<object, object?>>? expr = PagingHelpers.GetPropertySelectorExpression<Outer>("  Inner . Value  ");
      Assert.IsNotNull(expr);

      Func<object, object?> func = expr!.Compile();
      var input = new Outer { Inner = new Inner { Value = "Y" } };
      object? result = func(input);
      Assert.AreEqual("Y", result);
    }

    [TestMethod]
    public void When_PropertyDoesNotExist_GetPropertySelectorExpression_ReturnsNull()
    {
      Expression<Func<object, object?>>? expr = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("DoesNotExist");
      Assert.IsNull(expr);
    }

    [TestMethod]
    public void When_CalledMultipleTimes_GetPropertySelectorExpression_ReturnsSameCachedInstance()
    {
      Expression<Func<object, object?>>? expr1 = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("Name");
      Expression<Func<object, object?>>? expr2 = PagingHelpers.GetPropertySelectorExpression<SimpleEntity>("Name");

      Assert.IsNotNull(expr1);
      Assert.IsNotNull(expr2);
      Assert.IsTrue(object.ReferenceEquals(expr1, expr2));
    }
  }
}
