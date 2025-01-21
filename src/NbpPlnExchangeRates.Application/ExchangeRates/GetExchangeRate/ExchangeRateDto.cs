using NodaTime;

namespace NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;

public record ExchangeRateDto(
    string CurrencyCode,
    decimal BuyingRate,
    decimal SellingRate,
    LocalDate EffectiveDate);