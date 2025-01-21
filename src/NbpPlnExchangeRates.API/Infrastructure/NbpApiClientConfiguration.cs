using NbpPlnExchangeRates.Application.ApiClients;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public static class NbpApiClientConfiguration
{
    private const string Url = "https://api.nbp.pl/api/exchangerates/";
    
    public static void ConfigureNbpApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<INbpApiClient, NbpApiClient>(client =>
        {
            client.BaseAddress = new Uri(Url);
        });
    }
}