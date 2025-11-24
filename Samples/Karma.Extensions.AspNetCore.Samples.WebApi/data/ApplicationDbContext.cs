// -----------------------------------------------------------------------
// <copyright file="ApplicationDbContext.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Karma.Extensions.AspNetCore.Samples.WebApi.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Define your DbSets here
    public DbSet<WeatherForecast> WeatherForecasts
    {
      get; set;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => base.OnModelCreating(modelBuilder);// Configure your entity relationships, constraints, etc. here
  }
}
