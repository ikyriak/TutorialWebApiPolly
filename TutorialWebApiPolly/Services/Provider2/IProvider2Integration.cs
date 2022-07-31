namespace WebApiPolly.Services.Provider2
{
    public interface IProvider2Integration
    {
        Task<Provider2GetResponse?> GetWeatherForecastAsync(int delayMilli = 0, bool? returnErrorResponse = false, bool randomErros = false);
    }
}