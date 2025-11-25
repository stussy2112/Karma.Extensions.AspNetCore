// -----------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Karma.Extensions.AspNetCore
{
  internal static class TypeExtensions
  {
    /// <summary>
    /// Gets the type contained in a <paramref name="typeInfo"/> that is an <see cref="IEnumerable"/>.
    /// </summary>
    /// <param name="typeInfo">A <see cref="Type" /> instance to check.</param>
    /// <returns>The <see cref="TypeInfo" /> contained in the <paramref name="typeInfo"/>.</returns>
    public static Type? GetEnumerableElementType(this Type typeInfo)
    {
      if (typeInfo is null)
      {
        return null;
      }

      if (typeInfo.IsArray)
      {
        return typeInfo.GetElementType();
      }

      if (typeof(string) != typeInfo && typeof(IEnumerable).IsAssignableFrom(typeInfo) && typeInfo.IsAssignableToGenericType(typeof(IEnumerable<>), out Type? found))
      {
          Type[] genericArgs = found.GetGenericArguments();
          if (genericArgs.Length > 0)
          {
            return genericArgs[0];
          }
      }

      return null;
    }

    /// <summary>
    /// Determines if the <paramref name="instance"/> is assignable to the <paramref name="genericType"/>.
    /// </summary>
    /// <param name="instance">The <see cref="Type"/> to be checked.</param>
    /// <param name="genericType">The <see cref="Type"/> against which to test the <paramref name="instance"/></param>
    /// <returns><see langword="true"/> if the <paramref name="instance"/> is assignable to the <paramref name="genericType"/>, otherwise <see langword="false"/> </returns>
    /// <remarks>http://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059</remarks>
    public static bool IsAssignableToGenericType([NotNullWhen(true)] this Type? instance, [NotNullWhen(true)] Type? genericType)
    {
      if (instance is null || genericType is null)
      {
        return false;
      }

      if (instance.IsGenericType && instance.GetGenericTypeDefinition() == genericType)
      {
        return true;
      }

      if (Array.Exists(instance.GetInterfaces(), (it) => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
      {
        return true;
      }

      return instance.BaseType?.IsAssignableToGenericType(genericType) ?? false;
    }

    /// <summary>
    /// Determines if the <paramref name="typeInfo"/> is assignable to the <paramref name="genericType"/> and returns the found type.
    /// </summary>
    /// <param name="typeInfo">The <see cref="Type"/> to be checked.</param>
    /// <param name="genericType">The <see cref="Type"/> against which to test the <paramref name="typeInfo"/>.</param>
    /// <param name="foundType">When this method returns, contains the matching generic type if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <paramref name="typeInfo"/> is assignable to the <paramref name="genericType"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsAssignableToGenericType(this Type? typeInfo, Type? genericType, [NotNullWhen(true)] out Type? foundType)
    {
      foundType = null;
      if (typeInfo is null || genericType is null)
      {
        return false;
      }

      if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == genericType)
      {
        foundType = typeInfo;
        return true;
      }

      foundType = Array.Find(
          typeInfo.GetInterfaces(),
          (t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);

      return foundType is not null;
    }

    /// <summary>
    /// Indicates that the given <paramref name="typeInfo"/> is an enumerable.
    /// </summary>
    /// <param name="typeInfo">A <see cref="Type" /> instance to check.</param>
    /// <param name="containedType">The <see cref="TypeInfo" /> contained in the enumerable.</param>
    /// <returns>true if typeInfo is an enumerable, false otherwise.</returns>
    public static bool IsEnumerable([NotNullWhen(true)] this Type? typeInfo, [NotNullWhen(true)] out Type? containedType)
    {
      containedType = typeInfo?.GetEnumerableElementType();
      return containedType != null;
    }
  }
}