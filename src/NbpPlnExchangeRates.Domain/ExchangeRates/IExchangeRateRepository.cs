using NodaTime;

namespace NbpPlnExchangeRates.Domain.ExchangeRates;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByCurrencyIdAndEffectiveDateAsync(Guid currencyCodeId, LocalDate effectiveDate, CancellationToken cancellationToken = default);
    
    Task AddAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
}