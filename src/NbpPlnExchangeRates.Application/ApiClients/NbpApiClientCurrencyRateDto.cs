using NodaTime;

namespace NbpPlnExchangeRates.Application.ApiClients;

public record NbpApiClientCurrencyRateDto(
    string Code,
    LocalDate EffectiveDate,
    decimal Bid,
    decimal Ask);