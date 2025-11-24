// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Karma.Extensions.AspNetCore.Samples.WebApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Karma.Extensions.AspNetCore.Samples.WebApi
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      ConfigureServices(builder.Services);

      WebApplication app = builder.Build();

      ConfigureApplication(app);

      app.Run();
    }

    private static IApplicationBuilder UseOpenApiUi(WebApplication app)
    {
      // NSwag OpenAPI documents -> only supports JSON OR Yaml
      _ = app.UseOpenApi((o) => o.Path = "/openapi/{documentName}.json");
      _ = app.UseOpenApi((o) => o.Path = "/openapi/{documentName}.yaml");

      // Pretty Scalar UI for OpenAPI
      // SEE: https://guides.scalar.com/scalar/scalar-api-references/net-integration?utm_source=chatgpt.com#configuration-options__openapi-document
      _ = app.MapScalarApiReference((o) => o.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));

      // Adds the Swagger UI bits
      _ = app.UseSwagger((o) => o.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
      return app.UseSwaggerUI((o) =>
      {
        o.SwaggerEndpoint("/openapi/v1.json", "Karma.JsonApi.Sample.WebApi v1");
        o.SwaggerDocumentUrlsPath = "/openapi";
        o.ExposeSwaggerDocumentUrlsRoute = true;
      });
    }

    private static void ConfigureApplication(WebApplication app)
    {
      // After app.Build() and before configuring the pipeline
      using (IServiceScope scope = app.Services.CreateScope())
      {
        System.IServiceProvider services = scope.ServiceProvider;
        ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
      }

      // Configure the HTTP request pipeline.
      _ = UseOpenApiUi(app);

      _ = app.UseAuthorization();
      _ = app.MapControllers();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      // Add DbContext with In-Memory Database for simplicity
      _ = services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InMemoryDb"));
      _ = services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

      // Add other services as needed
      //_ = services.AddControllers()
      //  .AddSortInfoParameterBinding()
      //  .AddPagingInfoParameterBinding();

      _ = services.AddControllers().AddQueryStringInfoParameterBinding();

      // Microsoft.OpenApi -> Requires .NET Core 9.0
      //services.AddOpenApi();

      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      _ = services.AddEndpointsApiExplorer().AddSwaggerGen();

      // NSwag -> Needs `AddEndpointsApiExplorer`
      _ = services
        .AddEndpointsApiExplorer()
        .AddOpenApiDocument();
    }
  }
}
