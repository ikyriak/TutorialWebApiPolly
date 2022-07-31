namespace WebApiPolly.Services.Provider2
{
    public class Provider2GetResponse
    {
        public Provider2GetResponse(IEnumerable<Provider2WeatherForecast> weatherForecasts)
        {
            WeatherForecasts = weatherForecasts;
        }

        public IEnumerable<Provider2WeatherForecast> WeatherForecasts { get; set; }
    }
}
