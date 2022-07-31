namespace WebApiPolly.Services.Provider1
{
    public interface IProvider1Integration
    {
        Task<Provider1GetResponse?> GetWeatherForecastAsync(int delayMilli = 0, bool? returnErrorResponse = false);
    }
}