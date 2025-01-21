using System.Net;
using System.Text.Json;
using NbpPlnExchangeRates.Api.Common.Errors;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Domain.Common;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Text;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public class NbpApiClient : INbpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly LocalDatePattern _localDatePattern;

    public NbpApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var jsonSerializerOptions = new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        _jsonSerializerOptions = jsonSerializerOptions;
        
        _localDatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
    }
    
    public async Task<Result<NbpApiClientExchangeCurrencyRateDto>> GetCurrencyExchangeRate(string currencyCode, LocalDate effectiveDate, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"rates/C/{currencyCode}/{_localDatePattern.Format(effectiveDate)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return HandleNonSuccessStatusCode(response, currencyCode, effectiveDate);
        }

        CurrencyRateApiResponse? currencyRateApiResponse = await response.Content.ReadFromJsonAsync<CurrencyRateApiResponse>(_jsonSerializerOptions, cancellationToken); 

        if (currencyRateApiResponse is null)
        {
            return new ApiError("Could not parse NBP payload.");
        }

        if (currencyRateApiResponse.Rates.Count != 1)
        {
            return new ApiError("Invalid number of rates in NBP payload.");
        }
        
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(
                currencyRateApiResponse.Code,
                currencyRateApiResponse.Rates.Single().EffectiveDate,
                currencyRateApiResponse.Rates.Single().Bid,
                currencyRateApiResponse.Rates.Single().Ask);
        
        return Result.Success(nbpApiClientExchangeCurrencyRateDto);
    }

    private Result<NbpApiClientExchangeCurrencyRateDto> HandleNonSuccessStatusCode(HttpResponseMessage response, string currencyCode, LocalDate effectiveDate)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ApiError($"Invalid request for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."),
            HttpStatusCode.NotFound => new ApiError($"Currency not found for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."),
            _ => new ApiError("An error occurred while processing the request to NBP.")
        };
    }
}