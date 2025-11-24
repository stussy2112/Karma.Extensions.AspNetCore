// -----------------------------------------------------------------------
// <copyright file="TestValueProvider.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Karma.Extensions.AspNetCore.Tests.ModelBinding
{
  [ExcludeFromCodeCoverage]
  public class TestValueProvider : IValueProvider
  {
    public bool ContainsPrefix(string prefix) => false;
    public ValueProviderResult GetValue(string key) => ValueProviderResult.None;
  }
}