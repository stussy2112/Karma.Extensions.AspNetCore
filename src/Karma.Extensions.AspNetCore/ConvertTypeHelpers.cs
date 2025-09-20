// -----------------------------------------------------------------------
// <copyright file="ConvertTypeHelpers.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;

namespace Karma.Extensions.AspNetCore
{
  internal static class ConvertTypeHelpers
  {
    /// <summary>
    /// Converts a value to the target type, handling nullable types and Guid conversion appropriately.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when null values are provided for non-nullable value types in specific contexts.</exception>
    /// <exception cref="FormatException">Thrown when the value cannot be converted to the target type due to format issues.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside the range of the target type.</exception>
    public static object? ConvertToTargetType(object value, Type targetType)
    {
      // If the value is already the correct type, return it as-is
      if (value.GetType() == targetType)
      {
        return value;
      }

      // If target type is nullable, get the underlying type and convert to that first
      Type? underlyingType = Nullable.GetUnderlyingType(targetType);
      if (underlyingType is not null)
      {
        // Convert to the underlying type first, then wrap in nullable
        object? convertedValue = ConvertToTargetType(value, underlyingType);
        return convertedValue;
      }

      // Handle specific type conversions using pattern matching
      return targetType switch
      {
        Type t when t == typeof(Guid) => ConvertToGuid(value),
        Type t when t.IsEnum => ConvertToEnum(value, targetType),
        Type t when t == typeof(string) => value.ToString() ?? string.Empty,
        Type t when t == typeof(DateTime) => ConvertToDateTime(value),
        Type t when t == typeof(TimeSpan) => ConvertToTimeSpan(value),
        _ => Convert.ChangeType(value, targetType)
      };
    }

    /// <summary>
    /// Converts a value to a DateTime.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted DateTime value.</returns>
    /// <exception cref="FormatException">Thrown when the string value cannot be converted to DateTime.</exception>
    private static DateTime ConvertToDateTime(object value)
    {
      if (value is string dateString)
      {
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult))
        {
          return dateResult;
        }

        throw new FormatException($"String '{dateString}' is not a valid DateTime format.");
      }

      // For non-string values, fall back to standard conversion
      return (DateTime)Convert.ChangeType(value, typeof(DateTime));
    }

    /// <summary>
    /// Converts a value to an enum type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target enum type.</param>
    /// <returns>The converted enum value.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be converted to the enum type.</exception>
    private static object ConvertToEnum(object value, Type targetType)
    {
      if (value is string stringValue)
      {
        if (Enum.TryParse(targetType, stringValue, true, out object? enumResult))
        {
          return enumResult;
        }

        throw new FormatException($"String '{stringValue}' is not a valid value for enum '{targetType.Name}'.");
      }

      // Try to convert numeric values to enum
      try
      {
        return Enum.ToObject(targetType, value);
      }
      catch (ArgumentException ex)
      {
        throw new FormatException($"Value '{value}' cannot be converted to enum '{targetType.Name}'.", ex);
      }
    }

    /// <summary>
    /// Converts a value to a Guid.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted Guid value.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be converted to a Guid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the value type is not supported for Guid conversion.</exception>
    private static Guid ConvertToGuid(object value) => value switch
    {
      string stringValue when Guid.TryParse(stringValue, out Guid guidResult) => guidResult,
      string stringValue => throw new FormatException($"String '{stringValue}' is not a valid GUID format."),
      Guid guidValue => guidValue,
      byte[] byteArray when byteArray.Length == 16 => new Guid(byteArray),
      _ => throw new InvalidOperationException($"Cannot convert value of type '{value.GetType().Name}' to Guid.")
    };

    /// <summary>
    /// Converts a value to a TimeSpan.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted TimeSpan value.</returns>
    /// <exception cref="FormatException">Thrown when the string value cannot be converted to TimeSpan.</exception>
    private static TimeSpan ConvertToTimeSpan(object value)
    {
      if (value is string timeString)
      {
        if (TimeSpan.TryParse(timeString, CultureInfo.InvariantCulture, out TimeSpan timeResult))
        {
          return timeResult;
        }

        throw new FormatException($"String '{timeString}' is not a valid TimeSpan format.");
      }

      // For non-string values, fall back to standard conversion
      return (TimeSpan)Convert.ChangeType(value, typeof(TimeSpan));
    }
  }
}