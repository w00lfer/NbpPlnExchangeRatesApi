using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.CurrencyCodes;

namespace NbpPlnExchangeRates.Domain.Tests.CurrencyCodes;

[TestFixture]
public class CurrencyCodeTests
{
    [TestCase("US")]
    [TestCase("USDD")]
    [TestCase("U")]
    [TestCase("")]
    public void Create_IsoCodeIsTooShort_ReturnsFailure(string isoCode)
    {
        Result<CurrencyCode> result = CurrencyCode.Create(isoCode);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("IsoCode's code must be exactly 3 characters long."));
    }
    
    [Test]
    public void Create_IsoCodeIsNotUppercase_ReturnsFailure()
    {
        Result<CurrencyCode> result = CurrencyCode.Create("usd");

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("IsoCode's code must be uppercase."));
    }
    
    [Test]
    public void Create_IsoCodeIsValid_ReturnsSuccess()
    {
        Result<CurrencyCode> result = CurrencyCode.Create("USD");

        Assert.That(result.IsSuccess);
        Assert.That(result.Value.IsoCode, Is.EqualTo("USD"));
    }
}