using NbpPlnExchangeRates.Domain.Common;
using NodaTime;

namespace NbpPlnExchangeRates.Application.ApiClients;

public interface INbpApiClient
{
    Task<Result<NbpApiClientCurrencyRateDto>> GetCurrencyRates(string currencyCode, LocalDate effectiveDate);
}