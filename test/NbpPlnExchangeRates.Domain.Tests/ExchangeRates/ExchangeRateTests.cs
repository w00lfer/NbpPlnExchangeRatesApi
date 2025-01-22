using Moq;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Domain.ExchangeRates;
using NodaTime;

namespace NbpPlnExchangeRates.Domain.Tests.ExchangeRates;

[TestFixture]
public class ExchangeRateTests
{
    [Test]
    public void Create_BuyingRateIsNegative_ReturnsFailure()
    {
        CurrencyCode currencyCode = Mock.Of<CurrencyCode>(x => x.IsoCode == "USD");
        const decimal buyingRate = -1m;
        const decimal sellingRate = 3.7m;
        LocalDate effectiveDate = new LocalDate(2023, 10, 10);

        Result<ExchangeRate> result = ExchangeRate.Create(currencyCode, buyingRate, sellingRate, effectiveDate);

        Assert.That(result.IsFailure); 
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Rate must be greater or equal to 0."));
    }

    [Test]
    public void Create_SellingRateIsNegative_ReturnsFailure()
    {
        CurrencyCode currencyCode = Mock.Of<CurrencyCode>(x => x.IsoCode == "USD");
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = -1m;
        LocalDate effectiveDate = new LocalDate(2023, 10, 10);

        Result<ExchangeRate> result = ExchangeRate.Create(currencyCode, buyingRate, sellingRate, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Rate must be greater or equal to 0."));
    }

    [Test]
    public void Create_BothRatesAreNegative_ReturnsFailure()
    {
        CurrencyCode currencyCode = Mock.Of<CurrencyCode>(x => x.IsoCode == "USD");
        const decimal buyingRate = -1m;
        const decimal sellingRate = -1m;
        LocalDate effectiveDate = new LocalDate(2023, 10, 10);

        Result<ExchangeRate> result = ExchangeRate.Create(currencyCode, buyingRate, sellingRate, effectiveDate);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Rate must be greater or equal to 0."));
    }
    
    [Test]
    public void Create_WhenAllParametersAreValid_ReturnsSuccessWithExchangeRate()
    {
        CurrencyCode currencyCode = Mock.Of<CurrencyCode>(x => x.IsoCode == "USD");
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.7m;
        LocalDate effectiveDate = new LocalDate(2023, 10, 10);

        Result<ExchangeRate> result = ExchangeRate.Create(currencyCode, buyingRate, sellingRate, effectiveDate);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value.CurrencyCode, Is.EqualTo(currencyCode));
        Assert.That(result.Value.BuyingRate, Is.EqualTo(buyingRate));
        Assert.That(result.Value.SellingRate, Is.EqualTo(sellingRate));
        Assert.That(result.Value.EffectiveDate, Is.EqualTo(effectiveDate));

    }
}