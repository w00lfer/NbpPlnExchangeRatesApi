namespace NbpPlnExchangeRates.Domain.CurrencyCodes;

public interface ICurrencyCodeRepository
{
    Task<CurrencyCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}