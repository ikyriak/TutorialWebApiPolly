using Microsoft.AspNetCore.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;

namespace WebApiPolly.Services.Provider2
{
    public class Provider2Integration : IProvider2Integration
    {
        private const string Provider2_Base_Url = "https://localhost:7220";

        private static readonly HttpClient _httpClient = new()
        {
            // Network HTTP timeout in 2 seconds:
            Timeout = TimeSpan.FromSeconds(2)
        };

        private static readonly AsyncPolicyWrap<Provider2GetResponse?> _resiliencePolicies = InitiliazeApiPolicies();


        public async Task<Provider2GetResponse?> GetWeatherForecastAsync(int delayMilli = 0, bool? returnErrorResponse = false, bool randomErros = false)
        {
            Console.WriteLine($"{Environment.NewLine}{DateTime.Now:u} - REQUEST");

            int executionTry = 0;
            Random random = new();

            // Use Polly to apply the strategies (policies) to handle transient faults.
            return await _resiliencePolicies.ExecuteAsync(async () =>
            {
                try
                {
                    // This is used to simulate random errors and delays.
                    if (randomErros)
                    {
                        // Random Delays and Errors:
                        delayMilli = random.Next(0, 4000);
                        returnErrorResponse = random.NextDouble() >= 0.5;
                    }

                    // The code that retrieves data from the provider.
                    // TIP: We could create a private function for this code to be more readable.
                    var requestUri = $"{Provider2_Base_Url}/Provider2?delayMilli={delayMilli}&returnErrorResponse={returnErrorResponse}";

                    var httpResponseMessage = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

                    Console.WriteLine($"{DateTime.Now:u} - GetWeatherForecastAsync: ExecutionTry {executionTry++} | Success response: {httpResponseMessage.IsSuccessStatusCode}");

                    httpResponseMessage.EnsureSuccessStatusCode();

                    return await httpResponseMessage.Content.ReadFromJsonAsync<Provider2GetResponse?>();
                }
                catch (Exception ex)
                {
                    // This try-catch is used for logging the related exception.
                    // In case you do not need the log, you can remote the try-catch.
                    Console.WriteLine($"{DateTime.Now:u} - HttpCommunication | Exception: {ex.Message}");
                    throw;
                }
            });

        }

        private static AsyncPolicyWrap<Provider2GetResponse?> InitiliazeApiPolicies()
        {
            int maxRetries = 2;
            int breakCurcuitAfterErrors = 6;
            int keepCurcuitBreakForMinutes = 1;
            int timeoutInSeconds = 3;


            // Specify the type of exception that our policy can handle.
            // Alternately, we could specify the return results we would like to handle.
            var policyBuilder = Policy<Provider2GetResponse?>
                .Handle<Exception>();


            // Fallback policy:
            // Provide a default or substitute value if an execution faults.
            var fallbackPolicy = policyBuilder
              .FallbackAsync((calcellationToken) =>
              {
                  // In our case we return a null response.
                  Console.WriteLine($"{DateTime.Now:u} - Fallback null value is returned.");

                  return Task.FromResult<Provider2GetResponse?>(null);
              });


            // Wait and Retry policy:
            // Retry with exponential backoff, meaning that the delay between
            // retries increases as the number of retries increases.
            var retryPolicy = policyBuilder
                .WaitAndRetryAsync(maxRetries, retryAttempt =>
                {
                    var waitTime = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    Console.WriteLine($"{DateTime.Now:u} - RetryPolicy | Retry Attempt: {retryAttempt} | WaitSeconds: {waitTime.TotalSeconds}");

                    return waitTime;
                });


            // Break the circuit after 6 consecutive exceptions and keep circuit broken for 1 minute.
            var breakerPolicy = policyBuilder
                 .CircuitBreakerAsync(breakCurcuitAfterErrors, TimeSpan.FromMinutes(keepCurcuitBreakForMinutes),
                 onBreak: (exception, timespan, context) =>
                 {
                     // OnBreak, i.e. when circuit state change to open
                     Console.WriteLine($"{DateTime.Now:u} - BreakerPolicy | State changed to Open (blocked).");
                 },
                 onReset: (context) =>
                 {
                     // OnReset, i.e. when circuit state change to closed
                     Console.WriteLine($"{DateTime.Now:u} - BreakerPolicy | State changed to Closed (normal).");
                 });

            // Optional: Handle the "BrokenCircuitException" to keep a related log or/and return an alternative response.
            var fallbackPolicForCircuitBreaker = Policy<Provider2GetResponse?>
                .Handle<BrokenCircuitException>()
                .FallbackAsync((calcellationToken) =>
                {
                    // In our case we return a null response.
                    Console.WriteLine($"{DateTime.Now:u} - The Circuit is Open (blocked) for this Provider. A fallback null value is returned. Try again later.");

                    return Task.FromResult<Provider2GetResponse?>(null);
                });


            // Timeout Policy: Used when we cannot apply a timeout in the communication.
            // In our case, we have set the timeout directly in the HttpClient.
            // So, this timeout will not be used.
            var timeoutPolicy = Policy
                .TimeoutAsync<Provider2GetResponse?>(timeoutInSeconds, TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, _, _) =>
                {
                    Console.WriteLine($"{DateTime.Now:u} - TimeoutPolicy | Execution timed out after {timespan.TotalSeconds} seconds.");
                    return Task.CompletedTask;
                });


            // Define the combined policy strategy:
            return Policy.WrapAsync(
                fallbackPolicy,
                retryPolicy,
                fallbackPolicForCircuitBreaker,
                breakerPolicy,
                timeoutPolicy);
        }
    }
}
