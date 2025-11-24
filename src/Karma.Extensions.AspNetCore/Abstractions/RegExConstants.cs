// -----------------------------------------------------------------------
// <copyright file="RegExGroupNames.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Karma.Extensions.AspNetCore
{
  internal static partial class RegExConstants
  {
    public const string Culture = "en-US";
    public const RegexOptions RegExOptions = RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Compiled;
    public const int MatchTimeoutMilliseconds = 150;
    public static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(MatchTimeoutMilliseconds);
  }
}