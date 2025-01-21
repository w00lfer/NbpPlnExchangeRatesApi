using NodaTime;

namespace NbpPlnExchangeRates.Application.ApiClients;

public record NbpApiClientExchangeCurrencyRateDto(
    string Code,
    LocalDate EffectiveDate,
    decimal Bid,
    decimal Ask);