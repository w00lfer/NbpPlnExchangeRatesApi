using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Contrib.HttpClient;
using NbpPlnExchangeRates.Api.Infrastructure;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Domain.Common;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Text;

namespace NbpPlnExchangeRates.API.Tests.Infrastructure;

[TestFixture]
public class NbpApiClientTests
{
    private const string HttpClientBaseAddress = "https://api.nbp.pl/api/exchangerates/";
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    private NbpApiClient _nbpApiClient;
    
    [SetUp]
    public void SetUp()
    {
        HttpClient httpClient = _httpMessageHandlerMock.CreateClient();
        httpClient.BaseAddress = new Uri(HttpClientBaseAddress);
        
        _nbpApiClient = new(httpClient);
    }
    
    [Test]
    public async Task GetCurrencyExchangeRate_ResponseIsBadRequest_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2025, 1, 21);
        
        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsResponse(HttpStatusCode.BadRequest);
        
        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);
        
        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Invalid request for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }

    [Test]
    public async Task GetCurrencyExchangeRate_ResponseIsNotFound_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2025, 1, 21);

        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsResponse(HttpStatusCode.NotFound);

        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Currency not found for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }

    [Test]
    public async Task GetCurrencyExchangeRate_ResponseIsOtherError_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2025, 1, 21);

        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsResponse(HttpStatusCode.InternalServerError);

        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("An error occurred while processing the request to NBP."));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }

    [Test]
    public async Task GetCurrencyExchangeRate_ResponseContentIsNull_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2025, 1, 21);

        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsJsonResponse<CurrencyRateApiResponse>(null);

        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Could not parse NBP payload."));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }

    [Test]
    public async Task GetCurrencyExchangeRate_ResponseHasInvalidNumberOfRates_ReturnsApiError()
    {
        const string currencyCode = "USD";
        const decimal bid = 3.5m;
        const decimal ask = 3.7m;
        LocalDate effectiveDate = new(2025, 1, 21);
        
        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        CurrencyRateApiResponse responseContent = new(
            "C",
            "Currency",
            currencyCode,
            new List<RateApiResponse>()
            {
                new("1", effectiveDate, bid, ask),
                new("2", effectiveDate.PlusDays(1), bid, ask)
            });

        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsJsonResponse(responseContent, new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Invalid number of rates in NBP payload."));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }
    
    [Test]
    public async Task GetCurrencyExchangeRate_ResponseIsSuccessful_ReturnsCurrencyRate()
    {
        const string currencyCode = "USD";
        const decimal bid = 3.5m;
        const decimal ask = 3.7m;
        LocalDate effectiveDate = new(2025, 1, 21);

        var endpointUrl = string.Concat(HttpClientBaseAddress, GetRatesEndpointUrl(currencyCode, effectiveDate));
        CurrencyRateApiResponse responseContent = new(
            "C",
            "Currency",
            currencyCode,
            new List<RateApiResponse>()
            {
                new("1", effectiveDate, bid, ask),
            });

        _httpMessageHandlerMock
            .SetupRequest(HttpMethod.Get, endpointUrl)
            .ReturnsJsonResponse(responseContent, new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        Result<NbpApiClientExchangeCurrencyRateDto> result = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode, effectiveDate);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value.Code, Is.EqualTo(currencyCode));
        Assert.That(result.Value.EffectiveDate, Is.EqualTo(effectiveDate));
        Assert.That(result.Value.Bid, Is.EqualTo(bid));
        Assert.That(result.Value.Ask, Is.EqualTo(ask));
        
        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Get, endpointUrl);
    }
    
    private string GetRatesEndpointUrl(string currencyCode, LocalDate effectiveDate) =>
        $"rates/C/{currencyCode}/{LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd").Format(effectiveDate)}";
}