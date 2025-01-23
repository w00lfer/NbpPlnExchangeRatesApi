using Microsoft.Extensions.Http.Resilience;
using NbpPlnExchangeRates.Application.ApiClients;
using Polly;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public static class NbpApiClientConfiguration
{
    private const string Url = "https://api.nbp.pl/api/exchangerates/";

    public static void ConfigureNbpApiClient(this IServiceCollection services)
    { 
        services.AddHttpClient<INbpApiClient, NbpApiClient>(client => { client.BaseAddress = new Uri(Url); })
            .AddResilienceHandler("NbpApiResilienceStrategy", rb =>
            {
                rb.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>()
                });

                rb.AddTimeout(TimeSpan.FromSeconds(3));

                rb.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    FailureRatio = 0.2,
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(1),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>()
                }); 
            });
    }
}