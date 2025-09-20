// -----------------------------------------------------------------------
// <copyright file="FilterQueryStringParserCustomPatternProviderTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class FilterQueryStringParserCustomPatternProviderTests
  {
    public TestContext? TestContext
    {
      get;
      set;
    }

    [TestMethod]
    public void When_custom_pattern_provider_single_property_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      string pattern = /*lang=regex*/@"filter\[(?<path>[\p{L}\p{N}.\-_ ]+?)\]=(?<value>[^&\r\n]*)";
      var patternProvider = new FilterPatternProvider(pattern);
      var parser = new FilterQueryStringParser(patternProvider);
      string input = "filter[name]=John";

      // Act
      FilterInfoCollection result = parser.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("name-0", filterInfo.Name);
      Assert.AreEqual("name", filterInfo.Path);
      Assert.AreEqual(string.Empty, filterInfo.MemberOf);
      Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
      Assert.AreEqual("John", filterInfo.Values.FirstOrDefault());
    }
  }
}