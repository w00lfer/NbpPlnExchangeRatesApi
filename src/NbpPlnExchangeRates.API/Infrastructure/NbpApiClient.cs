using NbpPlnExchangeRates.Application.ApiClients;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public class NbpApiClient : INbpApiClient
{
    private readonly HttpClient _httpClient;

    public NbpApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}