// -----------------------------------------------------------------------
// <copyright file="DbInitializer.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Karma.Extensions.AspNetCore.Samples.WebApi.Data
{
  public static class DbInitializer
  {
    private static readonly string[] _summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public static void Initialize(ApplicationDbContext context)
    {
      ArgumentNullException.ThrowIfNull(context);

      _ = context.Database.EnsureCreated();

      // Seed data if needed
      if (!context.WeatherForecasts.Any())
      {
        IEnumerable<WeatherForecast> forecasts = Enumerable.Range(1, 5)
          .Select(index => new WeatherForecast
          {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = _summaries[Random.Shared.Next(_summaries.Length)]
          });

        foreach (WeatherForecast forecast in forecasts)
        {
          _ = context.WeatherForecasts.Add(forecast);
        }

        _ = context.SaveChanges();
      }
    }
  }
}