// -----------------------------------------------------------------------
// <copyright file="RegExExtensions.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  internal static class RegExExtensions
  {
    [return: NotNullIfNotNull(nameof(defaultValue))]
    internal static string? GetGroupCollectionValue(this GroupCollection? groupCollection, string key, string? defaultValue = "") =>
      groupCollection is null || !groupCollection[key].Success || string.IsNullOrWhiteSpace(groupCollection[key]?.Value)
        ? defaultValue
        : Uri.UnescapeDataString(groupCollection[key].Value);
  }
}