using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.Common.Errors;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NodaTime;

namespace NbpPlnExchangeRates.Domain.ExchangeRates;

public class ExchangeRate
{
    private ExchangeRate(
        CurrencyCode currencyCode,
        decimal buyingRate,
        decimal sellingRate,
        LocalDate effectiveDate)
    {
        Id = Guid.CreateVersion7();
        CurrencyCodeId = currencyCode.Id;
        CurrencyCode = currencyCode;
        BuyingRate = buyingRate;
        SellingRate = sellingRate;
        EffectiveDate = effectiveDate;
    }

    protected ExchangeRate()
    {
    }
    
    
    public Guid Id { get; set; }
    
    public Guid CurrencyCodeId { get; set; }
    
    public decimal BuyingRate { get; set; }
    
    public decimal SellingRate { get; set; }
    
    public LocalDate EffectiveDate { get; set; }
    
    public virtual CurrencyCode CurrencyCode { get; set; }
    
    public static Result<ExchangeRate> Create(
        CurrencyCode currencyCode,
        decimal buyingRate,
        decimal sellingRate,
        LocalDate effectiveDate)
    {
        var buyingRateValidationResult = ValidateRate(buyingRate);
        if (buyingRateValidationResult.IsFailure)
        {
            return Result.Failure<ExchangeRate>(buyingRateValidationResult.Errors);
        }
        
        var sellingRateValidationResult = ValidateRate(sellingRate);
        if (sellingRateValidationResult.IsFailure)
        {
            return Result.Failure<ExchangeRate>(sellingRateValidationResult.Errors);
        }
        
        return Result.Success(new ExchangeRate(currencyCode, buyingRate, sellingRate, effectiveDate));
    }
    
    private static Result ValidateRate(decimal rate) =>
        rate >= 0
            ? Result.Success()
            : new DomainError("Rate must be greater or equal to 0.");
    
    
}