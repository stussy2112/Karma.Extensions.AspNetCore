// -----------------------------------------------------------------------
// <copyright file="AddFilterInfoMiddlewareTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Karma.Extensions.AspNetCore.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class AddQueryStringInfoTests
  {
    public TestContext? TestContext
    {
      get;
      set;
    }

    [TestMethod]
    public void When_filter_querystring_present_then_middleware_parses_FilterInfo_and_stores_in_HttpContext_Items()
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
        Assert.AreEqual("root", actual.Name); // or whatever you expect
        Assert.AreEqual(1, actual.Count);

        var filterInfo = actual.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field", filterInfo.Path);
        Assert.AreEqual("value", filterInfo.Values.First());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?filter[field]=value", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_missing_filters_querystring_then_middleware_stores_default_FilterInfoCollection_in_HttpContext_Items()
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
        var filterInfo = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("root", filterInfo.Name); // or whatever you expect
        Assert.AreEqual(0, filterInfo.Count);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_parser_throws_exception_then_middleware_stores_default_FilterInfoCollection_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      var parser = new Mock<IParseStrategy<FilterInfoCollection>>();
      _ = parser.Setup((m) => m.Parse(It.IsAny<string>())).Throws(new InvalidOperationException("Something bad happened."));

      _ = app.ParseQueryStringFilters(parser.Object); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var filterInfo = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("root", filterInfo.Name); // or whatever you expect
        Assert.AreEqual(0, filterInfo.Count);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act      
      HttpResponseMessage response = client.GetAsync(new Uri("/?filter[field]=value", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_sort_querystring_present_then_middleware_parses_SortInfo_and_stores_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringSortingInfo(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.SortInfo));
        var actual = context.Items[ContextItemKeys.SortInfo] as IEnumerable<SortInfo>;
        Assert.IsNotNull(actual);
        Assert.AreEqual(2, actual.Count()); // or whatever you expect

        SortInfo? item = actual.FirstOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("firstName", item.FieldName);
        Assert.AreEqual(ListSortDirection.Descending, item.Direction);

        item = actual.LastOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("age", item.FieldName);
        Assert.AreEqual(ListSortDirection.Ascending, item.Direction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?sort=-firstName,age", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_missing_sort_querystring_then_middleware_stores_empty_SortInfo_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringSortingInfo(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.SortInfo));
        var actual = context.Items[ContextItemKeys.SortInfo] as IEnumerable<SortInfo>;
        Assert.IsNotNull(actual);
        Assert.AreEqual(0, actual.Count());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_paging_querystring_present_then_middleware_parses_PageInfo_and_stores_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringPagingInfo(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.PageInfo));
        var actual = context.Items[ContextItemKeys.PageInfo] as PageInfo;
        Assert.IsNotNull(actual);
        Assert.AreEqual(2u, actual.Offset);
        Assert.AreEqual(3u, actual.Limit);
        Assert.AreEqual(string.Empty, actual.Before);
        Assert.AreEqual(string.Empty, actual.After);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?page[offset]=2&page[limit]=3", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_missing_paging_querystring_then_middleware_stores_empty_PageInfo_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringPagingInfo(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.PageInfo));
        var actual = context.Items[ContextItemKeys.PageInfo] as PageInfo;
        Assert.IsNotNull(actual);
        Assert.AreEqual(0u, actual.Offset);
        Assert.AreEqual(uint.MaxValue, actual.Limit);
        Assert.AreEqual(string.Empty, actual.Before);
        Assert.AreEqual(string.Empty, actual.After);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_multiple_querystring_parsers_added_then_correct_items_stored_in_HttpContext_Items()
    {
      // Arrange
      WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
        EnvironmentName = "Development"
      });
      _ = builder.WebHost.UseTestServer();

      WebApplication app = builder.Build();
      _ = app.ParseQueryStringPagingInfo()
        .ParseQueryStringSortingInfo()
        .ParseQueryStringFilters(); // Use extension method

      // Add a terminal middleware to inspect the context
      app.Run(context =>
      {
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.PageInfo));
        var actual = context.Items[ContextItemKeys.PageInfo] as PageInfo;
        Assert.IsNotNull(actual);
        Assert.AreEqual(2u, actual.Offset);
        Assert.AreEqual(3u, actual.Limit);
        Assert.AreEqual(string.Empty, actual.Before);
        Assert.AreEqual(string.Empty, actual.After);

        // SortInfo
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.SortInfo));
        var sorts = context.Items[ContextItemKeys.SortInfo] as IEnumerable<SortInfo>;
        Assert.IsNotNull(sorts);
        Assert.AreEqual(2, sorts.Count()); // or whatever you expect

        SortInfo? item = sorts.FirstOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("firstName", item.FieldName);
        Assert.AreEqual(ListSortDirection.Descending, item.Direction);

        item = sorts.LastOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("age", item.FieldName);
        Assert.AreEqual(ListSortDirection.Ascending, item.Direction);

        // Filters
        Assert.IsTrue(context.Items.ContainsKey(ContextItemKeys.Filters));
        var filters = context.Items[ContextItemKeys.Filters] as FilterInfoCollection;
        Assert.IsNotNull(filters);
        Assert.AreEqual("root", filters.Name); // or whatever you expect
        Assert.AreEqual(1, filters.Count);
        var filterInfo = filters.First() as FilterInfo;
        Assert.IsNotNull(filterInfo);
        Assert.AreEqual("field", filterInfo.Path);
        Assert.AreEqual("value", filterInfo.Values.First());

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?filter[field]=value&page[offset]=2&page[limit]=3&sort=-firstName,age", UriKind.Relative), TestContext!.CancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);

    }
  }
}