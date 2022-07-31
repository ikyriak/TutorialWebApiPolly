using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProviderExampleApi1.DTOs;

namespace ProviderExampleApi1.Controllers
{
    [ApiController]
    [Route("Provider1")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GetWeatherForecastResponse>> GetAsync(int? delayMilli, bool? returnErrorResponse)
        {
            if (delayMilli.HasValue && delayMilli.Value > 0)
            {
                await Task.Delay(delayMilli.Value);
            }

            if (returnErrorResponse.HasValue && returnErrorResponse.Value)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"Provider1: {Summaries[Random.Shared.Next(Summaries.Length)]}"
            });

            return new GetWeatherForecastResponse(weatherForecasts);
        }
    }
}