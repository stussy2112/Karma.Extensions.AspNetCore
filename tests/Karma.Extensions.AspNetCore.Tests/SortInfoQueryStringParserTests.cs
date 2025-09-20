// -----------------------------------------------------------------------
// <copyright file="SortInfoQueryStringParserTests.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
  [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
  public sealed class SortInfoQueryStringParserTests
  {
    public TestContext? TestContext
    {
      get;
      set;
    }

    [TestMethod]
    public void When_uri_includes_multiple_sort_info_parses_as_SortInfo()
    {
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
        Assert.AreEqual(2, actual.Count());

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
      HttpResponseMessage response = client.GetAsync(new Uri("/?sort=-firstName,age", UriKind.Relative), TestContext!.CancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }

    [TestMethod]
    public void When_uri_includes_sort_info_parses_as_SortInfo()
    {
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
        Assert.AreEqual(2, actual.Count());

        SortInfo? item = actual.FirstOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("age", item.FieldName);
        Assert.AreEqual(ListSortDirection.Descending, item.Direction);

        item = actual.LastOrDefault();
        Assert.IsNotNull(item);
        Assert.AreEqual("firstName", item.FieldName);
        Assert.AreEqual(ListSortDirection.Ascending, item.Direction);

        return context.Response.WriteAsync("OK", TestContext!.CancellationTokenSource.Token);
      });

      app.Start();

      HttpClient client = app.GetTestClient();

      // Act
      HttpResponseMessage response = client.GetAsync(new Uri("/?sort=-age,  firstName,", UriKind.Relative), TestContext!.CancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();

      // Assert
      _ = response.EnsureSuccessStatusCode();
      string content = response.Content.ReadAsStringAsync(TestContext!.CancellationTokenSource.Token).GetAwaiter().GetResult();
      Assert.AreEqual("OK", content);
    }
  }
}
