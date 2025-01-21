using NbpPlnExchangeRates.Domain.Common;
using NodaTime;

namespace NbpPlnExchangeRates.Application.ApiClients;

public interface INbpApiClient
{
    Task<Result<NbpApiClientExchangeCurrencyRateDto>> GetCurrencyExchangeRate(string currencyCode, LocalDate effectiveDate, CancellationToken cancellationToken = default);
}