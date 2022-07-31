﻿namespace ProviderExampleApi2.DTOs
{
    public class GetWeatherForecastResponse
    {
        public GetWeatherForecastResponse(IEnumerable<WeatherForecast> weatherForecasts)
        {
            WeatherForecasts = weatherForecasts;
        }

        public IEnumerable<WeatherForecast> WeatherForecasts { get; set; }
    }
}
