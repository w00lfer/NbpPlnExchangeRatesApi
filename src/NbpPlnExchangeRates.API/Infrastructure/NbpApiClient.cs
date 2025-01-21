using System.Net;
using NbpPlnExchangeRates.Api.Common.Errors;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Domain.Common;
using NodaTime;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public class NbpApiClient : INbpApiClient
{
    private readonly HttpClient _httpClient;

    public NbpApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Result<NbpApiClientCurrencyRateDto>> GetCurrencyRates(string currencyCode, LocalDate effectiveDate)
    {
        var response = await _httpClient.GetAsync($"rates/C/{currencyCode}/{effectiveDate}");

        if (!response.IsSuccessStatusCode)
        {
            return HandleNonSuccessStatusCode(response, currencyCode, effectiveDate);
        }

        CurrencyRateApiResponse? currencyRateApiResponse = await response.Content.ReadFromJsonAsync<CurrencyRateApiResponse>();

        if (currencyRateApiResponse is null)
        {
            return new ApiError("Could not parse NBP payload.");
        }

        if (currencyRateApiResponse.Rates.Count != 1)
        {
            return new ApiError("Invalid number of rates in NBP payload.");
        }
        
        NbpApiClientCurrencyRateDto nbpApiClientCurrencyRateDto = new(
                currencyRateApiResponse.Code,
                currencyRateApiResponse.Rates.Single().EffectiveDate,
                currencyRateApiResponse.Rates.Single().Bid,
                currencyRateApiResponse.Rates.Single().Ask);
        
        return Result.Success(nbpApiClientCurrencyRateDto);
    }

    private Result<NbpApiClientCurrencyRateDto> HandleNonSuccessStatusCode(HttpResponseMessage response, string currencyCode, LocalDate effectiveDate)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ApiError($"Invalid request for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."),
            HttpStatusCode.NotFound => new ApiError($"Currency not found for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."),
            _ => new ApiError("An error occurred while processing the request to NBP.")
        };
    }
}