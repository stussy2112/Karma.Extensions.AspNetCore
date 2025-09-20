// -----------------------------------------------------------------------
// <copyright file="PageInfoQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using Karma.Extensions.AspNetCore.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Karma.Extensions.AspNetCore.Tests
{
  [TestClass]
  [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
  public sealed class PageInfoQueryStringParserTests
  {
    public TestContext? TestContext
    {
      get;
      set;
    }

    [TestMethod]
    public void When_uri_includes_page_info_with_offset_limit_parses_as_PageInfo()
    {
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
    public void When_uri_includes_page_info_with_after_limit_parses_as_PageInfo()
    {
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
        Assert.AreEqual(3u, actual.Limit);
        Assert.AreEqual("2", actual.After);
        Assert.AreEqual(string.Empty, actual.Before);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?page[after]=2&page[limit]=3", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_uri_includes_page_info_with_before_limit_parses_as_PageInfo()
    {
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
        Assert.AreEqual(3u, actual.Limit);
        Assert.AreEqual("2", actual.Before);
        Assert.AreEqual(string.Empty, actual.After);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?page[before]=2&page[limit]=3", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_uri_includes_page_info_with_cursor_limit_parses_as_PageInfo()
    {
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
        Assert.AreEqual(3u, actual.Limit);
        Assert.AreEqual(string.Empty, actual.Before);
        Assert.AreEqual("2", actual.After);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?page[cursor]=2&page[limit]=3", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_uri_does_not_include_page_info_parser_returns_default()
    {
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
      HttpResponseMessage response = client.GetAsync(new Uri("/somepath/", UriKind.Relative), TestContext!.CancellationTokenSource.Token)
        .ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }
  }
}
