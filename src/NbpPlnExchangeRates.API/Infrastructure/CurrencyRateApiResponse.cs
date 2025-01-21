using NodaTime;

namespace NbpPlnExchangeRates.Api.Infrastructure;

public record CurrencyRateApiResponse(
    string Table,
    string Currency,
    string Code,
    List<RateApiResponse> Rates);
    
public record RateApiResponse(
    string No,
    LocalDate EffectiveDate,
    decimal Bid,
    decimal Ask);