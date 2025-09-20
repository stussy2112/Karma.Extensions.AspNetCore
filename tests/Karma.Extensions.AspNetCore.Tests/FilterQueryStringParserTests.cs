// -----------------------------------------------------------------------
// <copyright file="FilterQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Karma.Extensions.AspNetCore.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class FilterQueryStringParserTests
  {
    private FilterQueryStringParser? _parser;

    [TestInitialize]
    public void TestInitialize() => _parser = new FilterQueryStringParser();

    public TestContext? TestContext
    {
      get;
      set;
    }

    /* Valid Filter strings
     * // Group Definition
     * filter[group]=some name
     * filter[group][$and]=some name
     * filter[group][$or][9]=some name // This may be an issue
     * filter[group][memberOfName]=some name
     * filter[group][memberOfName][elementIndex]=some name
     * filter[group][$or][memberOfName]=some name
     * filter[group][$and][memberOfName][elementIndex]=some name
     * 
     * // Filter Definition
     * filter[dotseparated.path]=value
     * filter[dotseparated.path][$ne]=value
     * filter[path1][path2][path3]=value
     * filter[path1][path2][path3][$lte]=value
     * filter[$and][dotseparatedpath]=value
     * filter[$or][elementIndex][dotseparatedpath][$lt]=value
     * filter[$and][elementIndex][path1][path2][path3]=value
     * filter[$or][elementIndex][path1][path2][path3][$ge]=value
     * filter[memberOfName][elementIndex][dotseparated.path]=value
     * filter[memberOfName][elementIndex][dotseparated.path][$in]=value
     * filter[memberOfName][elementIndex][path1][path2][path3]=value
     * filter[memberOfName][elementIndex][path1][path2][path3][$eq]=value
     */

    [TestMethod]
    public void When_single_simple_filter_single_property_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);

        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_na me-0", filterInfo.Name);
        Assert.AreEqual("field_na me", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?filter[field_na me]=value", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_simple_group_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("groupName", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(0, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?filter[group]=groupName", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_group_defined_conjunction_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("groupName", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(0, actual.Count);
        Assert.AreEqual(Conjunction.Or, actual.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or]=groupName";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_group_contains_filter_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("groupName", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("groupName", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group]=groupName&filter[groupName][0][field_name]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_group_with_conjunction_contains_filter_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("groupName", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(Conjunction.Or, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("groupName", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or]=groupName&filter[groupName][0][field_name]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_group_with_conjunction_and_filter_with_conjunction_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(2, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("groupName", filterInfoColl.Name);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(1, filterInfoColl.Count);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        var filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("groupName", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfoColl = actual.Last() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("field_name-or-group", filterInfoColl.Name);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(1, filterInfoColl.Count);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("anotherValue", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or]=groupName&filter[groupName][0][field_name]=value&filter[$or][0][field_name]=anotherValue";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_group_memberOf_undefined_group_conjunction_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("memberOfName", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("groupName", filterInfoColl.Name);
        Assert.AreEqual("memberOfName", filterInfoColl.MemberOf);
        Assert.AreEqual(0, filterInfoColl.Count);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or][memberOfName]=groupName";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_simple_groups_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(2, actual.Count);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("groupName", filterInfoColl.Name);
        Assert.AreEqual(0, filterInfoColl.Count);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.And, filterInfoColl.Conjunction);

        filterInfoColl = actual.Last() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("secondGroupName", filterInfoColl.Name);
        Assert.AreEqual(0, filterInfoColl.Count);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.And, filterInfoColl.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group]=groupName&filter[group]=secondGroupName";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    /*
    Suggested Edge Cases to Test
    2.	Duplicate or Conflicting Groups
        •	Multiple groups with the same name but different conjunctions:
          filter[group][$and]=g&filter[group][$or]=g
        •	Multiple filters with the same path and group index.
     */
    [TestMethod]
    public void When_multiple_simple_groups_same_name_same_conjunction_parses_as_first_group_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual($"g", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);
        Assert.AreEqual(0, actual.Count);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$and]=g&filter[group][$and]=g";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_simple_groups_same_name_different_conjunction_parses_as_first_group_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual($"g", actual.Name);
        Assert.AreEqual(Conjunction.Or, actual.Conjunction);
        Assert.AreEqual(0, actual.Count);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or]=g&filter[group][$and]=g";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_groups_defined_conjunction_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(2, actual.Count);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("groupName", filterInfoColl.Name);
        Assert.AreEqual(0, filterInfoColl.Count);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        filterInfoColl = actual.Last() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("secondGroupName", filterInfoColl.Name);
        Assert.AreEqual(0, filterInfoColl.Count);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.And, filterInfoColl.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$or]=groupName&filter[group][$and]=secondGroupName";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_filter_same_property_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.LessThan, filterInfo.Operator);
        Assert.AreEqual("value2", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[field_name][$gt]=value&filter[field_name][$lt]=value2";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_filters_for_different_properties_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(4, actual.Count);

        Assert.IsNotNull(actual);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Skip(1).First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(Operator.LessThan, filterInfo.Operator);
        Assert.AreEqual("value2", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Skip(2).First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("another_field_name-0", filterInfo.Name);
        Assert.AreEqual("another_field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Contains, filterInfo.Operator);
        Assert.AreEqual("containedValue", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("another_field_name-1", filterInfo.Name);
        Assert.AreEqual("another_field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.LessThanOrEqualTo, filterInfo.Operator);
        Assert.AreEqual("bigvalue", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[field_name][$gt]=value&filter[field_name][$lt]=value2&filter[another_field_name][$contains]=containedValue&filter[another_field_name][$le]=bigvalue";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_deeply_nested_group_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("g1", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("g2", filterInfoColl.Name);
        Assert.AreEqual(1, filterInfoColl.Count);
        Assert.AreEqual("g1", filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        filterInfoColl = filterInfoColl.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("g3", filterInfoColl.Name);
        Assert.AreEqual(1, filterInfoColl.Count);
        Assert.AreEqual("g2", filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.And, filterInfoColl.Conjunction);

        var filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("g3", filterInfo.MemberOf);
        Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[group][$and]=g1&filter[group][$or][g1]=g2&filter[group][$and][g2]=g3&filter[g3][0][field_name][$gt]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_filters_for_different_properties_with_conjunctions_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(2, actual.Count);

        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfoColl = actual.First() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("field_name-and-group", filterInfoColl.Name);
        Assert.AreEqual(2, filterInfoColl.Count);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.And, filterInfoColl.Conjunction);

        var filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-and-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.LessThan, filterInfo.Operator);
        Assert.AreEqual("value2", filterInfo.Values.FirstOrDefault());

        filterInfo = filterInfoColl.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-and-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfoColl = actual.Last() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual("another_field_name-or-group", filterInfoColl.Name);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(2, filterInfoColl.Count);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("another_field_name-0", filterInfo.Name);
        Assert.AreEqual("another_field_name", filterInfo.Path);
        Assert.AreEqual("another_field_name-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.Contains, filterInfo.Operator);
        Assert.AreEqual("containedValue", filterInfo.Values.FirstOrDefault());

        filterInfo = filterInfoColl.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("another_field_name-1", filterInfo.Name);
        Assert.AreEqual("another_field_name", filterInfo.Path);
        Assert.AreEqual("another_field_name-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.LessThanOrEqualTo, filterInfo.Operator);
        Assert.AreEqual("bigvalue", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[$and][1][field_name][$gt]=value&filter[$and][0][field_name][$lt]=value2&filter[$or][1][another_field_name][$contains]=containedValue&filter[$or][1][another_field_name][$le]=bigvalue";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_nested_properties_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);
        Assert.AreEqual(1, actual.Count);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name.nested_field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name.nested_field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[field_name][nested_field_name]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    [DataRow("$eq", Operator.EqualTo)]
    [DataRow("$EQ", Operator.EqualTo)]
    [DataRow("$ge", Operator.GreaterThanOrEqualTo)]
    [DataRow("$GE", Operator.GreaterThanOrEqualTo)]
    [DataRow("$gt", Operator.GreaterThan)]
    [DataRow("$GT", Operator.GreaterThan)]
    [DataRow("$le", Operator.LessThanOrEqualTo)]
    [DataRow("$LE", Operator.LessThanOrEqualTo)]
    [DataRow("$lt", Operator.LessThan)]
    [DataRow("$LT", Operator.LessThan)]
    [DataRow("$ne", Operator.NotEqualTo)]
    [DataRow("$NE", Operator.NotEqualTo)]
    [DataRow("$between", Operator.Between)]
    [DataRow("$BETWEEN", Operator.Between)]
    [DataRow("$notbetween", Operator.NotBetween)]
    [DataRow("$NOTBETWEEN", Operator.NotBetween)]
    [DataRow("$contains", Operator.Contains)]
    [DataRow("$CONTAINS", Operator.Contains)]
    [DataRow("$notcontains", Operator.NotContains)]
    [DataRow("$NOTCONTAINS", Operator.NotContains)]
    [DataRow("$endswith", Operator.EndsWith)]
    [DataRow("$ENDSWITH", Operator.EndsWith)]
    [DataRow("$null", Operator.IsNull)]
    [DataRow("$NULL", Operator.IsNull)]
    [DataRow("$notnull", Operator.IsNotNull)]
    [DataRow("$NOTNULL", Operator.IsNotNull)]
    [DataRow("$startswith", Operator.StartsWith)]
    [DataRow("$STARTSWITH", Operator.StartsWith)]
    [DataRow("$regex", Operator.Regex)]
    [DataRow("$REGEX", Operator.Regex)]
    public void When_single_filter_single_property_with_operator_parses_as_root_FilterInfoCollection(string @operator, Operator expectedOperator)
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.IsNotNull(actual);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(expectedOperator, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = $"filter[field_name][{@operator}]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    [DataRow("and", Conjunction.And)]
    [DataRow("or", Conjunction.Or)]
    public void When_single_filter_single_property_with_conjunction_parses_as_named_FilterInfoCollection(string conjunction, Conjunction expectedConjunction)
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);

        Assert.IsNotNull(actual);
        Assert.AreEqual($"field_name-{conjunction}-group", actual.Name);
        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual(expectedConjunction, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual($"field_name-{conjunction}-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = $@"filter[${conjunction}][0][field_name]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    [DataRow("$ItBroke")]
    [DataRow("$NOTconjunction")]
    [DataRow("$INVALID")]
    public void When_single_filter_single_property_with_invalid_conjunction_parses_as_empty_root_FilterInfoCollection(string conjunction)
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual($"root", actual.Name);
        Assert.AreEqual(0, actual.Count);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = $@"filter[{conjunction}][0][field_name]=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_filters_same_property_with_conjunction_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(2, actual.Count);

        Assert.IsNotNull(actual);
        Assert.AreEqual($"field_name-and-group", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-and-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-and-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value2", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = $@"filter[$and][0][field_name]=value&filter[$and][1][field_name]=value2";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_filters_single_properties_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(2, actual.Count);
        Assert.IsNotNull(actual);
        Assert.AreEqual($"root", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("another_field_name-0", filterInfo.Name);
        Assert.AreEqual("another_field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
        Assert.AreEqual("secondValue", filterInfo.Values.FirstOrDefault());

        var filterInfoColl = actual.Last() as FilterInfoCollection;
        Assert.IsNotNull(filterInfoColl);
        Assert.AreEqual($"field_name-or-group", filterInfoColl.Name);
        Assert.AreEqual(string.Empty, filterInfoColl.MemberOf);
        Assert.AreEqual(Conjunction.Or, filterInfoColl.Conjunction);

        filterInfo = filterInfoColl.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());

        filterInfo = filterInfoColl.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-1", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual("field_name-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.Contains, filterInfo.Operator);
        Assert.AreEqual("value2", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = $@"filter[$or][0][field_name][$eq]=value
&filter[$or][1][field_name][$contains]=value2
&filter[another_field_name][$gt]=secondValue";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    /*
    1.	Empty or Malformed Input
        •	filter[]= (empty brackets)
        •	filter[field_name] (missing =value)
        •	filter[field_name]= (empty value)
        •	filter==value(missing brackets)
    */
    [TestMethod]
    public void When_filter_empty_brackets_parses_as_empty_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual($"root", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(0, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = "filter[]=";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    /*    
    5.	Operator Edge Cases
        •	Unknown or unsupported operator:
          filter[field_name][$unknown]= value
        •	Operator with missing value:
          filter[field_name][$gt]=
     */
    [TestMethod]
    [DataRow("filter[field_name]")]
    [DataRow("filter[field_name][$eq]")]
    [DataRow("filter[field_name][$ge]")]
    [DataRow("filter[field_name][$gt]")]
    [DataRow("filter[field_name][$le]")]
    [DataRow("filter[field_name][$lt]")]
    [DataRow("filter[field_name][$ne]")]
    [DataRow("filter[field_name][$between]")]
    [DataRow("filter[field_name][$contains]")]
    [DataRow("filter[field_name][$endswith]")]
    [DataRow("filter[field_name][$isnull]")]
    [DataRow("filter[field_name][$startswith]")]
    [DataRow("filter[field_name][$unknown]")]
    public void When_filter_missing_value_parses_as_empty_root_FilterInfoCollection(string input)
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual($"root", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(0, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_filter_missing_brackets_parses_as_empty_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);
        Assert.AreEqual($"root", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(0, actual.Count);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = "filter=value";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    /*
    3.	Unusual Characters in Property Names
        •	Property names with special or unicode characters:
          filter[na!me]=value, filter[字段]=value
    4.	Multiple Values
        •	Comma-separated values with spaces or empty entries:
          filter[field_name]=a,,b, ,c
    6.Deeply Nested Groups
        •	More than three levels of nested groups to test recursion and stack safety.
    7.	Group Index Gaps or Non-Sequential Indices
        •	filter[$and][0][field]= a & filter[$and][2][field] = b(missing index 1)
    8.	Case Sensitivity
        •	Mixed-case conjunctions and operators:
          filter[$and][0][field]= a, filter[field_name][$Gt]= b
    9.	Multiple Filters with Same Path but Different Operators
        •	filter[field_name][$gt]= 1 & filter[field_name][$lt] = 10
    10.	Whitespace Handling
        •	Extra spaces in property names, values, or between brackets:
          filter[field_name] = value 
    11.	Group with No Name
        •	filter[group]= (group with empty name)
    12.	Multiple MemberOf References
        •	Filters referencing a group that is not defined in the same query string.
    13.	Very Large Query Strings
        •	Stress test with hundreds or thousands of filters/groups to check performance and memory usage.
    */

    [TestMethod]
    public void When_single_filter_regex_operator_basic_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("email-0", filterInfo.Name);
        Assert.AreEqual("email", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_phone_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("phone-0", filterInfo.Name);
        Assert.AreEqual("phone", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^\(\d{3}\)\s\d{3}-\d{4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[phone][$regex]=^\(\d{3}\)\s\d{3}-\d{4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_product_code_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("productCode-0", filterInfo.Name);
        Assert.AreEqual("productCode", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[A-Z]{3}-\d{3}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[productCode][$regex]=^[A-Z]{3}-\d{3}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_alphanumeric_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("username-0", filterInfo.Name);
        Assert.AreEqual("username", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9]+$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[username][$regex]=^[a-zA-Z0-9]+$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_case_sensitive_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("name-0", filterInfo.Name);
        Assert.AreEqual("name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^Test.*", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[name][$regex]=^Test.*";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_date_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("dateString-0", filterInfo.Name);
        Assert.AreEqual("dateString", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^\d{4}-\d{2}-\d{2}|\d{2}\/\d{2}\/\d{4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[dateString][$regex]=^\d{4}-\d{2}-\d{2}|\d{2}\/\d{2}\/\d{4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_single_filter_regex_operator_url_validation_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("website-0", filterInfo.Name);
        Assert.AreEqual("website", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~?//=]*)$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      //no #, & in regex
      string input = @"filter[website][$regex]=^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~?//=]*)$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_regex_filters_different_properties_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(2, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("email-0", filterInfo.Name);
        Assert.AreEqual("email", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9._%+-]+@company\.com$", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("employeeId-0", filterInfo.Name);
        Assert.AreEqual("employeeId", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^EMP-\d{4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[email][$regex]=^[a-zA-Z0-9._%+-]+@company\.com$&filter[employeeId][$regex]=^EMP-\d{4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_conjunction_group_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("email-and-group", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("email-0", filterInfo.Name);
        Assert.AreEqual("email", filterInfo.Path);
        Assert.AreEqual("email-and-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[$and][0][email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_or_conjunction_group_parses_as_named_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("phone-or-group", actual.Name);
        Assert.AreEqual(string.Empty, actual.MemberOf);
        Assert.AreEqual(Conjunction.Or, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("phone-0", filterInfo.Name);
        Assert.AreEqual("phone", filterInfo.Path);
        Assert.AreEqual("phone-or-group", filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^\(\d{3}\)\s\d{3}-\d{4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[$or][0][phone][$regex]=^\(\d{3}\)\s\d{3}-\d{4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_case_insensitive_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("email-0", filterInfo.Name);
        Assert.AreEqual("email", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"(?i)gmail\.com$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[email][$regex]=(?i)gmail\.com$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_word_boundary_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("content-0", filterInfo.Name);
        Assert.AreEqual("content", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"\bimportant\b", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[content][$regex]=\bimportant\b";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_quantifier_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("code-0", filterInfo.Name);
        Assert.AreEqual("code", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^\d{2,4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[code][$regex]=^\d{2,4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_character_class_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("description-0", filterInfo.Name);
        Assert.AreEqual("description", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[^0-9]+$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[description][$regex]=^[^0-9]+$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_nested_property_path_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("user.email-0", filterInfo.Name);
        Assert.AreEqual("user.email", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", filterInfo.Values.FirstOrDefault());
        //Assert.AreEqual failed. Expected:<^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$>. Actual:<^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2>.
        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[user][email][$regex]=^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_complex_mixed_regex_and_other_operators_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(3, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("email-0", filterInfo.Name);
        Assert.AreEqual("email", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^[a-zA-Z0-9._%+-]+@company\.com$", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Skip(1).First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("status-0", filterInfo.Name);
        Assert.AreEqual("status", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
        Assert.AreEqual("active", filterInfo.Values.FirstOrDefault());

        filterInfo = actual.Last() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("employeeId-0", filterInfo.Name);
        Assert.AreEqual("employeeId", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.AreEqual(@"^EMP-\d{4}$", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[email][$regex]=^[a-zA-Z0-9._%+-]+@company\.com$&filter[status]=active&filter[employeeId][$regex]=^EMP-\d{4}$";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_empty_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field_name-0", filterInfo.Name);
        Assert.AreEqual("field_name", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        Assert.IsEmpty(filterInfo.Values);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      string input = @"filter[field_name][$regex]=";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_regex_filter_with_url_encoded_pattern_parses_as_root_FilterInfoCollection()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringFilters();

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var actual = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(actual);

        Assert.AreEqual(1, actual.Count);
        Assert.AreEqual("root", actual.Name);
        Assert.AreEqual(Conjunction.And, actual.Conjunction);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("pattern-0", filterInfo.Name);
        Assert.AreEqual("pattern", filterInfo.Path);
        Assert.AreEqual(string.Empty, filterInfo.MemberOf);
        Assert.AreEqual(Operator.Regex, filterInfo.Operator);
        // URL encoded \d+ should decode to \d+
        Assert.AreEqual(@"\d+", filterInfo.Values.FirstOrDefault());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act - URL encoded version of \d+ pattern
      string input = @"filter[pattern][$regex]=%5Cd%2B";
      HttpResponseMessage response = client.GetAsync(new Uri($"/?{input}", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_ParameterKey_accessed_returns_filter()
    {
      // Act
      string result = _parser!.ParameterKey;

      // Assert
      Assert.AreEqual("filter", result);
    }

    [TestMethod]
    public void When_Parse_called_with_null_returns_empty_FilterInfoCollection()
    {
      // Act
      FilterInfoCollection result = _parser!.Parse(null!);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(0, result.Count);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_empty_string_returns_empty_FilterInfoCollection()
    {
      // Act
      FilterInfoCollection result = _parser!.Parse(string.Empty);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(0, result.Count);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_whitespace_returns_empty_FilterInfoCollection()
    {
      // Act
      FilterInfoCollection result = _parser!.Parse("   ");

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(0, result.Count);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_simple_filter_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

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

    [TestMethod]
    public void When_Parse_called_with_single_group_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[group]=users";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("users", result.Name);
      Assert.AreEqual(string.Empty, result.MemberOf);
      Assert.AreEqual(0, result.Count);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_operator_filter_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[age][$gt]=18";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("age-0", filterInfo.Name);
      Assert.AreEqual("age", filterInfo.Path);
      Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
      Assert.AreEqual("18", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_nested_property_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[user][profile][name]=John";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("user.profile.name-0", filterInfo.Name);
      Assert.AreEqual("user.profile.name", filterInfo.Path);
      Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
      Assert.AreEqual("John", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_conjunction_filter_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[$and][0][name]=John&filter[$and][1][name][$contains]=Doe";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("name-and-group", result.Name);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(Conjunction.And, result.Conjunction);

      var firstFilter = result.First() as FilterInfo;
      Assert.IsNotNull(firstFilter);
      Assert.AreEqual("name-0", firstFilter.Name);
      Assert.AreEqual("name", firstFilter.Path);
      Assert.AreEqual("name-and-group", firstFilter.MemberOf);
      Assert.AreEqual(Operator.EqualTo, firstFilter.Operator);
      Assert.AreEqual("John", firstFilter.Values.FirstOrDefault());

      var secondFilter = result.Last() as FilterInfo;
      Assert.IsNotNull(secondFilter);
      Assert.AreEqual("name-1", secondFilter.Name);
      Assert.AreEqual("name", secondFilter.Path);
      Assert.AreEqual("name-and-group", secondFilter.MemberOf);
      Assert.AreEqual(Operator.Contains, secondFilter.Operator);
      Assert.AreEqual("Doe", secondFilter.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_url_encoded_values_returns_decoded_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[email]=%20test%40example.com%20";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("email-0", filterInfo.Name);
      Assert.AreEqual("email", filterInfo.Path);
      Assert.AreEqual(Operator.EqualTo, filterInfo.Operator);
      Assert.AreEqual(" test@example.com ", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    [DataRow("filter[tags][$in]=red,blue,green", new[] { "red", "blue", "green" })]
    [DataRow("filter[score][$between]=10,90", new[] { "10", "90" })]
    [DataRow("filter[categories][$notin]=draft,archived", new[] { "draft", "archived" })]
    [DataRow("filter[range][$notbetween]=5,15", new[] { "5", "15" })]
    public void When_Parse_called_with_comma_separated_values_returns_array_values(string input, string[] expectedValues)
    {
      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual(expectedValues?.Length, filterInfo.Values.Count);

      for (int i = 0; i < expectedValues?.Length; i++)
      {
        Assert.AreEqual(expectedValues[i], filterInfo.Values.ElementAt(i));
      }
    }

    [TestMethod]
    public void When_Parse_called_with_special_characters_in_property_name_returns_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[field_with-special.chars]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("field_with-special.chars-0", filterInfo.Name);
      Assert.AreEqual("field_with-special.chars", filterInfo.Path);
      Assert.AreEqual("value", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_empty_value_returns_FilterInfoCollection_with_empty_values()
    {
      // Arrange
      string input = "filter[name]=";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("name-0", filterInfo.Name);
      Assert.AreEqual("name", filterInfo.Path);
      Assert.IsTrue(filterInfo.Values.Count == 0 || string.IsNullOrEmpty(filterInfo.Values.FirstOrDefault()?.ToString()));
    }

    [TestMethod]
    public void When_Parse_called_with_invalid_regex_pattern_returns_empty_FilterInfoCollection()
    {
      // Arrange
      string input = "filter[invalidpattern]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
    }

    [TestMethod]
    public void When_TryParse_called_with_null_returns_false()
    {
      // Act
      bool result = _parser!.TryParse(null!, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_TryParse_called_with_empty_string_returns_false()
    {
      // Act
      bool result = _parser!.TryParse(string.Empty, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_TryParse_called_with_whitespace_returns_false()
    {
      // Act
      bool result = _parser!.TryParse("   ", out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_TryParse_called_with_valid_filter_returns_true()
    {
      // Arrange
      string input = "filter[name]=John";

      // Act
      bool result = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(parsed);
      Assert.AreEqual(1, parsed.Count);

      var filterInfo = parsed.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("name-0", filterInfo.Name);
      Assert.AreEqual("John", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_TryParse_called_with_invalid_filter_returns_false()
    {
      // Arrange
      string input = "notafilter[name]=John";

      // Act
      bool result = _parser!.TryParse(input, out FilterInfoCollection? parsed);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    public void When_IParseStrategy_Parse_called_returns_object()
    {
      // Arrange
      IParseStrategy parseStrategy = _parser!;
      string input = "filter[name]=John";

      // Act
      object? result = parseStrategy.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsInstanceOfType(result, typeof(FilterInfoCollection));

      var filterCollection = result as FilterInfoCollection;
      Assert.IsNotNull(filterCollection);
      Assert.AreEqual(1, filterCollection.Count);
    }

    [TestMethod]
    public void When_IParseStrategy_TryParse_called_with_valid_input_returns_true()
    {
      // Arrange
      IParseStrategy parseStrategy = _parser!;
      string input = "filter[name]=John";

      // Act
      bool result = parseStrategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsTrue(result);
      Assert.IsNotNull(parsed);
      Assert.IsInstanceOfType(parsed, typeof(FilterInfoCollection));
    }

    [TestMethod]
    public void When_IParseStrategy_TryParse_called_with_invalid_input_returns_false()
    {
      // Arrange
      IParseStrategy parseStrategy = _parser!;
      string input = "notafilter";

      // Act
      bool result = parseStrategy.TryParse(input, out object? parsed);

      // Assert
      Assert.IsFalse(result);
      Assert.IsNull(parsed);
    }

    [TestMethod]
    [DataRow("$eq", Operator.EqualTo)]
    [DataRow("$ne", Operator.NotEqualTo)]
    [DataRow("$gt", Operator.GreaterThan)]
    [DataRow("$gte", Operator.GreaterThanOrEqualTo)]
    [DataRow("$ge", Operator.GreaterThanOrEqualTo)]
    [DataRow("$lt", Operator.LessThan)]
    [DataRow("$lte", Operator.LessThanOrEqualTo)]
    [DataRow("$le", Operator.LessThanOrEqualTo)]
    [DataRow("$contains", Operator.Contains)]
    [DataRow("$notcontains", Operator.NotContains)]
    [DataRow("$startswith", Operator.StartsWith)]
    [DataRow("$endswith", Operator.EndsWith)]
    [DataRow("$in", Operator.In)]
    [DataRow("$notin", Operator.NotIn)]
    [DataRow("$between", Operator.Between)]
    [DataRow("$notbetween", Operator.NotBetween)]
    [DataRow("$null", Operator.IsNull)]
    [DataRow("$notnull", Operator.IsNotNull)]
    [DataRow("$regex", Operator.Regex)]
    public void When_Parse_called_with_different_operators_returns_correct_Operator(string operatorValue, Operator expectedOperator)
    {
      // Arrange
      string input = $"filter[field][{operatorValue}]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual(expectedOperator, filterInfo.Operator);
    }

    [TestMethod]
    public void When_Parse_called_with_unknown_operator_does_not_create_filters()
    {
      // Arrange
      string input = "filter[field][$unknown]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow("and", Conjunction.And)]
    [DataRow("or", Conjunction.Or)]
    public void When_Parse_called_with_conjunction_returns_correct_Conjunction(string conjunctionValue, Conjunction expectedConjunction)
    {
      // Arrange
      string input = $"filter[${conjunctionValue}][0][field]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual($"field-{conjunctionValue}-group", result.Name);
      Assert.AreEqual(expectedConjunction, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_case_insensitive_operator_returns_correct_Operator()
    {
      // Arrange
      string input = "filter[field][$GT]=10";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual(Operator.GreaterThan, filterInfo.Operator);
    }

    [TestMethod]
    public void When_Parse_called_with_case_insensitive_conjunction_returns_correct_Conjunction()
    {
      // Arrange
      string input = "filter[$AND][0][field]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("field-AND-group", result.Name);
      Assert.AreEqual(Conjunction.And, result.Conjunction);
    }

    [TestMethod]
    public void When_Parse_called_with_complex_nested_structure_returns_hierarchy()
    {
      // Arrange
      string input = "filter[group][$and]=parent&filter[group][$or][parent]=child&filter[child][0][name]=value";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("parent", result.Name);
      Assert.AreEqual(1, result.Count);

      var childGroup = result.First() as FilterInfoCollection;
      Assert.IsNotNull(childGroup);
      Assert.AreEqual("child", childGroup.Name);
      Assert.AreEqual("parent", childGroup.MemberOf);
      Assert.AreEqual(Conjunction.Or, childGroup.Conjunction);
      Assert.AreEqual(1, childGroup.Count);

      var filterInfo = childGroup.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("name-0", filterInfo.Name);
      Assert.AreEqual("child", filterInfo.MemberOf);
    }

    [TestMethod]
    public void When_Parse_called_with_multiple_values_with_empty_entries_handles_correctly()
    {
      // Arrange
      string input = "filter[tags][$in]=red,,blue, ,green";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual(Operator.In, filterInfo.Operator);

      // Should filter out empty entries
      var values = filterInfo.Values.Where((v) => !string.IsNullOrWhiteSpace(v?.ToString())).ToList();
      Assert.IsGreaterThanOrEqualTo(3, values.Count); // Should have at least red, blue, green
    }

    [TestMethod]
    public void When_Parse_called_with_non_sequential_group_indices_handles_correctly()
    {
      // Arrange
      string input = "filter[$and][0][field1]=value1&filter[$and][2][field2]=value2";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void When_Parse_called_with_very_long_input_handles_correctly()
    {
      // Arrange
      var inputBuilder = new System.Text.StringBuilder();
      for (int i = 0; i < 100; i++)
      {
        if (i > 0)
        {
          _ = inputBuilder.Append('&');
        }

        _ = inputBuilder.Append($"filter[field{i}]=value{i}");
      }

      string input = inputBuilder.ToString();

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(100, result.Count);
    }

    [TestMethod]
    public void When_Parse_called_with_unicode_characters_handles_correctly()
    {
      // Arrange
      string input = "filter[字段名]=测试值";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var filterInfo = result.First() as FilterInfo;
      Assert.IsNotNull(filterInfo);
      Assert.AreEqual("字段名-0", filterInfo.Name);
      Assert.AreEqual("字段名", filterInfo.Path);
      Assert.AreEqual("测试值", filterInfo.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_multiple_filters_same_path_different_operators_groups_correctly()
    {
      // Arrange
      string input = "filter[age][$gt]=18&filter[age][$lt]=65";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(2, result.Count);

      var firstFilter = result.First() as FilterInfo;
      Assert.IsNotNull(firstFilter);
      Assert.AreEqual("age-0", firstFilter.Name);
      Assert.AreEqual(Operator.GreaterThan, firstFilter.Operator);

      var secondFilter = result.Last() as FilterInfo;
      Assert.IsNotNull(secondFilter);
      Assert.AreEqual("age-1", secondFilter.Name);
      Assert.AreEqual(Operator.LessThan, secondFilter.Operator);
    }

    [TestMethod]
    public void When_Parse_called_with_same_path_multiple_operators_creates_separate_filters()
    {
      // Arrange - Test multiple operators on the same field path
      string input = "filter[price][$gte]=100&filter[price][$lte]=500&filter[price][$ne]=299";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(3, result.Count, "Should create three separate filters for the same path");
      Assert.AreEqual(Conjunction.And, result.Conjunction);

      // Verify each filter has the same path but different operators and names
      var filters = result.Cast<FilterInfo>().ToList();

      // First filter: price >= 100
      FilterInfo firstFilter = filters[0];
      Assert.AreEqual("price-0", firstFilter.Name);
      Assert.AreEqual("price", firstFilter.Path);
      Assert.AreEqual(string.Empty, firstFilter.MemberOf);
      Assert.AreEqual(Operator.GreaterThanOrEqualTo, firstFilter.Operator);
      Assert.AreEqual("100", firstFilter.Values.FirstOrDefault());

      // Second filter: price <= 500  
      FilterInfo secondFilter = filters[1];
      Assert.AreEqual("price-1", secondFilter.Name);
      Assert.AreEqual("price", secondFilter.Path);
      Assert.AreEqual(string.Empty, secondFilter.MemberOf);
      Assert.AreEqual(Operator.LessThanOrEqualTo, secondFilter.Operator);
      Assert.AreEqual("500", secondFilter.Values.FirstOrDefault());

      // Third filter: price != 299
      FilterInfo thirdFilter = filters[2];
      Assert.AreEqual("price-2", thirdFilter.Name);
      Assert.AreEqual("price", thirdFilter.Path);
      Assert.AreEqual(string.Empty, thirdFilter.MemberOf);
      Assert.AreEqual(Operator.NotEqualTo, thirdFilter.Operator);
      Assert.AreEqual("299", thirdFilter.Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_same_path_same_operator_multiple_times_creates_separate_filters()
    {
      // Arrange - Test the same operator multiple times on the same field
      string input = "filter[category][$ne]=draft&filter[category][$ne]=archived&filter[category][$ne]=deleted";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(3, result.Count, "Should create three separate filters even with same operator");

      var filters = result.Cast<FilterInfo>().ToList();

      // Verify all have same path and operator but different names and values
      for (int i = 0; i < filters.Count; i++)
      {
        Assert.AreEqual($"category-{i}", filters[i].Name);
        Assert.AreEqual("category", filters[i].Path);
        Assert.AreEqual(Operator.NotEqualTo, filters[i].Operator);
        Assert.AreEqual(string.Empty, filters[i].MemberOf);
      }

      Assert.AreEqual("draft", filters[0].Values.FirstOrDefault());
      Assert.AreEqual("archived", filters[1].Values.FirstOrDefault());
      Assert.AreEqual("deleted", filters[2].Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_nested_path_multiple_operators_creates_separate_filters()
    {
      // Arrange - Test multiple operators on nested property paths
      string input = "filter[user][profile][age][$gt]=21&filter[user][profile][age][$lt]=65&filter[user][profile][age][$ne]=30";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(3, result.Count);

      var filters = result.Cast<FilterInfo>().ToList();

      // Verify all filters have the same nested path
      foreach (FilterInfo? filter in filters)
      {
        Assert.AreEqual("user.profile.age", filter.Path);
        Assert.AreEqual(string.Empty, filter.MemberOf);
      }

      // Verify unique names and different operators
      Assert.AreEqual("user.profile.age-0", filters[0].Name);
      Assert.AreEqual(Operator.GreaterThan, filters[0].Operator);
      Assert.AreEqual("21", filters[0].Values.FirstOrDefault());

      Assert.AreEqual("user.profile.age-1", filters[1].Name);
      Assert.AreEqual(Operator.LessThan, filters[1].Operator);
      Assert.AreEqual("65", filters[1].Values.FirstOrDefault());

      Assert.AreEqual("user.profile.age-2", filters[2].Name);
      Assert.AreEqual(Operator.NotEqualTo, filters[2].Operator);
      Assert.AreEqual("30", filters[2].Values.FirstOrDefault());
    }

    [TestMethod]
    public void When_Parse_called_with_mixed_paths_and_duplicate_paths_groups_correctly()
    {
      // Arrange - Test mix of unique paths and duplicate paths with different operators
      string input = "filter[status]=active&filter[price][$gte]=100&filter[price][$lte]=500&filter[category]=electronics&filter[status][$ne]=pending";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(5, result.Count, "Should create five separate filters");

      var filters = result.Cast<FilterInfo>().ToList();

      // Verify status filters (should have status-0 and status-1)
      var statusFilters = filters.Where(f => f.Path == "status").ToList();
      Assert.HasCount(2, statusFilters);
      Assert.AreEqual("status-0", statusFilters[0].Name);
      Assert.AreEqual("status-1", statusFilters[1].Name);

      // Verify price filters (should have price-0 and price-1)
      var priceFilters = filters.Where(f => f.Path == "price").ToList();
      Assert.HasCount(2, priceFilters);
      Assert.AreEqual("price-0", priceFilters[0].Name);
      Assert.AreEqual("price-1", priceFilters[1].Name);

      // Verify category filter (should have category-0)
      var categoryFilters = filters.Where(f => f.Path == "category").ToList();
      Assert.HasCount(1, categoryFilters);
      Assert.AreEqual("category-0", categoryFilters[0].Name);
    }

    [TestMethod]
    public void When_Parse_called_with_same_path_in_conjunction_groups_creates_separate_filters()
    {
      // Arrange - Test same path with different operators within conjunction groups
      string input = "filter[$and][0][score][$gt]=80&filter[$or][0][score][$eq]=100&filter[$and][1][score][$lt]=95";

      // Act
      FilterInfoCollection result = _parser!.Parse(input);

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual("root", result.Name);
      Assert.AreEqual(2, result.Count, "Should create two conjunction groups");

      // Find the AND group
      FilterInfoCollection? andGroup = result.Cast<FilterInfoCollection>().FirstOrDefault(g => g.Name == "score-and-group");
      Assert.IsNotNull(andGroup, "Should create an AND group for score filters");
      Assert.AreEqual(Conjunction.And, andGroup.Conjunction);
      Assert.AreEqual(2, andGroup.Count);

      var andFilters = andGroup.Cast<FilterInfo>().ToList();
      Assert.AreEqual("score-0", andFilters[0].Name);
      Assert.AreEqual("score-2", andFilters[1].Name);

      // Find the OR group
      FilterInfoCollection? orGroup = result.Cast<FilterInfoCollection>().FirstOrDefault(g => g.Name == "score-or-group");
      Assert.IsNotNull(orGroup, "Should create an OR group for score filters");
      Assert.AreEqual(Conjunction.Or, orGroup.Conjunction);
      Assert.AreEqual(1, orGroup.Count);

      FilterInfo orFilter = orGroup.Cast<FilterInfo>().First();
      Assert.AreEqual("score-1", orFilter.Name);
      Assert.AreEqual("score", orFilter.Path);
      Assert.AreEqual(Operator.EqualTo, orFilter.Operator);
    }
  }
}