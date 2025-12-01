// -----------------------------------------------------------------------
// <copyright file="IQueryableExtensionsTests.NoInvocationExpression.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore.Tests
{
  public partial class IQueryableExtensionsTests
  {
    private sealed class Inner
    {
      public string? Value { get; set; } = string.Empty;
    }

    private sealed class Outer
    {
      public Inner? Inner { get; set; } = new Inner();
    }

    private sealed class SimpleEntity
    {
      public string? Name { get; set; } = string.Empty;
    }

    [TestMethod]
    public void GetOrCreatePropertySelectorExpression_ShouldNotContainInvocation_ForSimpleProperty()
    {
      Type targetType = typeof(Karma.Extensions.AspNetCore.IQueryableExtensions);
      MethodInfo? method = targetType.GetMethod("GetOrCreatePropertySelectorExpression", BindingFlags.NonPublic | BindingFlags.Static);
      Assert.IsNotNull(method, "GetOrCreatePropertySelectorExpression method not found via reflection");

      MethodInfo generic = method!.MakeGenericMethod(typeof(SimpleEntity));
      object? result = generic.Invoke(null, [nameof(SimpleEntity.Name)]);
      Assert.IsNotNull(result);

      var expr = result as Expression<Func<SimpleEntity, object?>>;
      Assert.IsNotNull(expr);

      bool foundInvocation = ContainsInvocation(expr.Body);
      Assert.IsFalse(foundInvocation, "Expression should not contain InvocationExpression nodes");
    }

    [TestMethod]
    public void GetOrCreatePropertySelectorExpression_ShouldNotContainInvocation_ForNestedProperty()
    {
      Type targetType = typeof(Karma.Extensions.AspNetCore.IQueryableExtensions);
      MethodInfo? method = targetType.GetMethod("GetOrCreatePropertySelectorExpression", BindingFlags.NonPublic | BindingFlags.Static);
      Assert.IsNotNull(method, "GetOrCreatePropertySelectorExpression method not found via reflection");

      MethodInfo generic = method!.MakeGenericMethod(typeof(Outer));
      object? result = generic.Invoke(null, ["Inner.Value"]);
      Assert.IsNotNull(result);

      var expr = result as Expression<Func<Outer, object?>>;
      Assert.IsNotNull(expr);

      bool foundInvocation = ContainsInvocation(expr.Body);
      Assert.IsFalse(foundInvocation, "Expression should not contain InvocationExpression nodes for nested property");
    }

    private static bool ContainsInvocation(Expression expr)
    {
      var visitor = new InvocationDetectingVisitor();
      _ = visitor.Visit(expr);
      return visitor.FoundInvocation;
    }

    private sealed class InvocationDetectingVisitor : ExpressionVisitor
    {
      public bool FoundInvocation { get; private set; }

      public override Expression? Visit(Expression? node)
      {
        if (node is null)
        {
          return null;
        }

        return base.Visit(node);
      }

      protected override Expression VisitInvocation(InvocationExpression node)
      {
        FoundInvocation = true;
        return base.VisitInvocation(node);
      }
    }
  }
}
