// -----------------------------------------------------------------------
// <copyright file="WeatherForecastController.cs" company="Karma, LLC">
//   Copyright (c) Karma, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Karma.Extensions.AspNetCore.ModelBinding;
using Karma.Extensions.AspNetCore.Samples.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Karma.Extensions.AspNetCore.Samples.WebApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private readonly IRepository<WeatherForecast, int> _repository;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IRepository<WeatherForecast, int> repository)
    {
      _repository = repository;
      _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WeatherForecast), 200, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<WeatherForecast>> GetAsync(int id)
    {
      WeatherForecast? item = await _repository.GetByIdAsync(id);
      if (item == null)
      {
        return NotFound();
      }

      return Ok(item);
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetAsync([FromQuery(Name = "filter")]FilterInfoCollection? filters = null, [FromQuery(Name = "sort")]IEnumerable<SortInfo>? sortInfos = null, [FromQuery(Name = "page")] PageInfo? pageInfo = null)
    {
      IEnumerable<WeatherForecast> items = await _repository.GetAllAsync();

      IEnumerable<WeatherForecast> filtered = filters?.Apply(items) ?? items;
      IEnumerable<WeatherForecast> sorted = sortInfos?.Apply(filtered) ?? filtered;
      // MUST DO OFFSET PAGING FOR RELIABILITY
      IEnumerable<WeatherForecast> paged = pageInfo?.Apply(sorted) ?? sorted;

      return Ok(paged);

    }
  }
}
