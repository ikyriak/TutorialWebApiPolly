using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApiPolly.DTOs;
using WebApiPolly.Services.Provider1;
using WebApiPolly.Services.Provider2;

namespace TutorialWebApiPolly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastsController : ControllerBase
    {
        // INFO: In an actual project, we could organize the following code in separate files to
        //       have a thin controller. For example, a service file that retrieves and maps the
        //       data, mapper files, etc. For simplicity reasons of this tutorial, we have them all
        //       here :)

        private readonly ILogger<WeatherForecastsController> _logger;
        private readonly IProvider1Integration _provider1Integration;
        private readonly IProvider2Integration _provider2Integration;

        public WeatherForecastsController(
            ILogger<WeatherForecastsController> logger,
            IProvider1Integration provider1Integration,
            IProvider2Integration provider2Integration)
        {
            _logger = logger;
            _provider1Integration = provider1Integration;
            _provider2Integration = provider2Integration;
        }

        [HttpGet]
        public async Task<ActionResult<GetWeatherForecastResponse>> GetDataAsync()
        {
            // Call the providers to retrieve the data
            Task<Provider1GetResponse?> provider1ResponseTask = _provider1Integration.GetWeatherForecastAsync();
            Task<Provider2GetResponse?> provider2ResponseTask = _provider2Integration.GetWeatherForecastAsync();

            await Task.WhenAll(provider1ResponseTask, provider2ResponseTask);

            // Map and combine the retrieved data from the providers.
            return MapProviderReponses(provider1ResponseTask.Result, provider2ResponseTask.Result);
        }

        [HttpGet("continuous-failures")]
        public async Task<ActionResult<GetWeatherForecastResponse>> GetDataDuringContinuousFailuresAsync()
        {
            // Call the providers to retrieve the data
            Task<Provider1GetResponse?> provider1ResponseTask = _provider1Integration.GetWeatherForecastAsync();
            Task<Provider2GetResponse?> provider2ResponseTask = _provider2Integration.GetWeatherForecastAsync(1000, true);

            await Task.WhenAll(provider1ResponseTask, provider2ResponseTask);

            // Map and combine the retrieved data from the providers.
            return MapProviderReponses(provider1ResponseTask.Result, provider2ResponseTask.Result);
        }

        [HttpGet("timeout-errors")]
        public async Task<ActionResult<GetWeatherForecastResponse>> GetDataDuringTimeoutsAsync()
        {
            // Call the providers to retrieve the data
            Task<Provider1GetResponse?> provider1ResponseTask = _provider1Integration.GetWeatherForecastAsync();
            Task<Provider2GetResponse?> provider2ResponseTask = _provider2Integration.GetWeatherForecastAsync(10000, false);

            await Task.WhenAll(provider1ResponseTask, provider2ResponseTask);

            // Map and combine the retrieved data from the providers.
            return MapProviderReponses(provider1ResponseTask.Result, provider2ResponseTask.Result);
        }

        [HttpGet("transient-faults")]
        public async Task<ActionResult<GetWeatherForecastResponse>> GetDataDuringDelaysAsync()
        {
            // Call the providers to retrieve the data
            Task<Provider1GetResponse?> provider1ResponseTask = _provider1Integration.GetWeatherForecastAsync();
            Task<Provider2GetResponse?> provider2ResponseTask = _provider2Integration.GetWeatherForecastAsync(randomErros: true);

            await Task.WhenAll(provider1ResponseTask, provider2ResponseTask);

            // Map and combine the retrieved data from the providers.
            return MapProviderReponses(provider1ResponseTask.Result, provider2ResponseTask.Result);
        }



        private static GetWeatherForecastResponse MapProviderReponses(Provider1GetResponse? provider1GetResponse, Provider2GetResponse? provider2GetResponse)
        {
            List<WeatherForecast> weatherForecasts = new List<WeatherForecast>();

            if (provider1GetResponse is not null)
            {
                weatherForecasts.AddRange(provider1GetResponse.WeatherForecasts.Select(f => new WeatherForecast()
                {
                    ProviderId = 1,
                    Date = f.Date,
                    Summary = f.Summary,
                    TemperatureC = f.TemperatureC,
                }));
            }

            if (provider2GetResponse is not null)
            {
                weatherForecasts.AddRange(provider2GetResponse.WeatherForecasts.Select(f => new WeatherForecast()
                {
                    ProviderId = 2,
                    Date = f.Date,
                    Summary = f.Summary,
                    TemperatureC = f.TemperatureC,
                }));
            }

            return new GetWeatherForecastResponse(weatherForecasts);
        }
    }
}