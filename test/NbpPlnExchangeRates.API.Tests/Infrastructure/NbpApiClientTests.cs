using System.Net;
using System.Text.Json;
using Moq;
using Moq.Contrib.HttpClient;
using NbpPlnExchangeRates.Api.Infrastructure;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Domain.Common;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace NbpPlnExchangeRates.API.Tests.Infrastructure;

[TestFixture]
public class NbpApiClientTests
{
    private const string HttpClientBaseAddress = "https://api.nbp.pl/api/exchangerates";
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
    public async Task GetCurrencyRates_WhenResponseIsBadRequest_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2024, 1, 21);
        
        _httpMessageHandlerMock
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsResponse(HttpStatusCode.BadRequest);
        
        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);
        
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Invalid request for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."));
    }

    [Test]
    public async Task GetCurrencyRates_WhenResponseIsNotFound_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2024, 1, 21);

        _httpMessageHandlerMock
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsResponse(HttpStatusCode.NotFound);

        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Currency not found for CurrencyCode={currencyCode}, EffectiveDate={effectiveDate}."));
    }

    [Test]
    public async Task GetCurrencyRates_WhenResponseIsOtherError_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2024, 1, 21);

        _httpMessageHandlerMock
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsResponse(HttpStatusCode.InternalServerError);

        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("An error occurred while processing the request to NBP."));
    }

    [Test]
    public async Task GetCurrencyRates_WhenResponseContentIsNull_ReturnsApiError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new(2024, 1, 21);

        _httpMessageHandlerMock
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsJsonResponse<CurrencyRateApiResponse>(null);

        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Could not parse NBP payload."));
    }

    [Test]
    public async Task GetCurrencyRates_WhenResponseHasInvalidNumberOfRates_ReturnsApiError()
    {
        const string currencyCode = "USD";
        const decimal bid = 3.5m;
        const decimal ask = 3.7m;
        LocalDate effectiveDate = new(2024, 1, 21);

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
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsJsonResponse(responseContent);

        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Invalid number of rates in NBP payload."));
    }
    
    [Test]
    public async Task GetCurrencyRates_WhenResponseIsSuccessful_ReturnsCurrencyRate()
    {
        const string currencyCode = "USD";
        const decimal bid = 3.5m;
        const decimal ask = 3.7m;
        LocalDate effectiveDate = new(2024, 1, 21);

        CurrencyRateApiResponse responseContent = new(
            "C",
            "Currency",
            currencyCode,
            new List<RateApiResponse>()
            {
                new("1", effectiveDate, bid, ask),
            });

        _httpMessageHandlerMock
            .SetupRequest(HttpClientBaseAddress + GetRatesEndpointUrl(currencyCode, effectiveDate))
            .ReturnsResponse(responseContent);

        Result<NbpApiClientCurrencyRateDto> result = await _nbpApiClient.GetCurrencyRates(currencyCode, effectiveDate);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Code, Is.EqualTo(currencyCode));
        Assert.That(result.Value.EffectiveDate, Is.EqualTo(effectiveDate));
        Assert.That(result.Value.Bid, Is.EqualTo(bid));
        Assert.That(result.Value.Ask, Is.EqualTo(ask));
    }
    
    private string GetRatesEndpointUrl(string currencyCode, LocalDate effectiveDate) =>
        $"rates/C/{currencyCode}/{effectiveDate}";
}