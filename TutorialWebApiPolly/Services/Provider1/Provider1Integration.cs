namespace WebApiPolly.Services.Provider1
{
    public class Provider1Integration : IProvider1Integration
    {
        private const string Provider1_Base_Url = "https://localhost:7051";

        private static readonly HttpClient _httpClient = new()
        {
            // Network HTTP timeout in 2 seconds:
            Timeout = TimeSpan.FromSeconds(2)
        };

        public async Task<Provider1GetResponse?> GetWeatherForecastAsync(int delayMilli = 0, bool? returnErrorResponse = false)
        {
            var requestUri = $"{Provider1_Base_Url}/Provider1?delayMilli={delayMilli}&returnErrorResponse={returnErrorResponse}";

            var httpResponseMessage = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadFromJsonAsync<Provider1GetResponse>();
        }
    }
}
