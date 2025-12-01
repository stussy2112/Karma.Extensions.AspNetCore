// -----------------------------------------------------------------------
// <copyright file="PagingHelpers.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  internal static class PagingHelpers
  {
    private const BindingFlags Binding_Attrs = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
    private static readonly ConcurrentDictionary<(Type, string), Expression<Func<object, object?>>?> _propertySelectorExpressionCache = new ();

    /// <summary>
    /// Creates a lambda expression that selects the value of a specified property from an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>The returned expression can be used to dynamically access property values for objects of type
    /// <typeparamref name="T"/>. If the property name is not found or is invalid, the method returns null. The
    /// expression is cached for performance.</remarks>
    /// <typeparam name="T">The type of the object from which the property value will be selected.</typeparam>
    /// <param name="propertyName">The name of the property to select. Cannot be null, empty, or whitespace.</param>
    /// <returns>An expression representing a function that takes an object and returns the value of the specified property as an
    /// object, or null if the property name is invalid or the property does not exist.</returns>
    internal static Expression<Func<object, object?>>? GetPropertySelectorExpression<T>(string propertyName)
    {
      (Type, string) key = (typeof(T), propertyName);
      return _propertySelectorExpressionCache.GetOrAdd(key, static (propByTypeKey) =>
      {
        (Type type, string propName) = propByTypeKey;

        if (string.IsNullOrWhiteSpace(propName))
        {
          return null;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(object), "entity");
        Expression? memberExpression = GetMemberExpression(propName, type, parameter);
        if (memberExpression is null)
        {
          return null;
        }

        // Convert value to object for a uniform key selector
        Expression converted = Expression.Convert(memberExpression, typeof(object));
        return Expression.Lambda<Func<object, object?>>(converted, parameter);
      });
    }

    internal static bool UseAfterPaging<TValue>(string? after, out TValue? afterParsed)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      afterParsed = default;
      return !string.IsNullOrWhiteSpace(after) && TValue.TryParse(after, null, out afterParsed) && afterParsed is not null;
    }

    internal static bool UseBeforePaging<TValue>(string? before, out TValue? beforeParsed)
      where TValue : IComparable<TValue>, IParsable<TValue>
    {
      beforeParsed = default;
      return !string.IsNullOrWhiteSpace(before) && TValue.TryParse(before, null, out beforeParsed) && beforeParsed is not null;
    }

    private static Expression? GetMemberExpression(string memberName, Type parentType, ParameterExpression parameterExpression)
    {
      if (string.IsNullOrWhiteSpace(memberName))
      {
        return null;
      }

      Type currentMemberType = parentType;
      Expression currentMember = Expression.Convert(parameterExpression, parentType);

      string[] parts = memberName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      foreach (string part in parts)
      {
        if (string.IsNullOrEmpty(part))
        {
          return null;
        }

        PropertyInfo? prop = currentMemberType.GetProperty(part, Binding_Attrs);
        if (prop is not null)
        {
          currentMember = Expression.Property(currentMember, prop);
          currentMemberType = prop.PropertyType;
          continue;
        }

        FieldInfo? field = currentMemberType.GetField(part, Binding_Attrs);
        if (field is not null)
        {
          currentMember = Expression.Field(currentMember, field);
          currentMemberType = field.FieldType;
          continue;
        }

        // Not found
        return null;
      }

      return currentMember;
    }
  }
}