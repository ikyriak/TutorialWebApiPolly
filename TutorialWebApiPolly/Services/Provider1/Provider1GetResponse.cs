namespace WebApiPolly.Services.Provider1
{
    public class Provider1GetResponse
    {
        public Provider1GetResponse(IEnumerable<Provider1WeatherForecast> weatherForecasts)
        {
            WeatherForecasts = weatherForecasts;
        }

        public IEnumerable<Provider1WeatherForecast> WeatherForecasts { get; set; }
    }
}
