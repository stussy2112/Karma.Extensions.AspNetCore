// -----------------------------------------------------------------------
// <copyright file="WeatherForecast.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Karma.Extensions.AspNetCore.Samples.WebApi
{
  [DebuggerDisplay("{Id,nq} - {Date.ToShortDateString(),nq} - {TemperatureC}C/{TemperatureF}F - {Summary,nq}")]
  public class WeatherForecast
  {
    public int Id
    {
      get; set;
    }

    public DateOnly Date
    {
      get; set;
    }

    public int TemperatureC
    {
      get; set;
    }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary
    {
      get; set;
    }
  }
}
