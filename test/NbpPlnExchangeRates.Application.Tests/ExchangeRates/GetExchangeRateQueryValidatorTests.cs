using FluentValidation.Results;
using Moq;
using NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;
using NodaTime;
using NodaTime.Extensions;

namespace NbpPlnExchangeRates.Application.Tests.ExchangeRates;

[TestFixture]
public class GetExchangeRateQueryValidatorTests
{
    private readonly Mock<IClock> _clockMock = new();
    
    private GetExchangeRateQueryValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new GetExchangeRateQueryValidator(_clockMock.Object);
        
        _clockMock.Reset();
    }

    [Test]
    public void CurrencyCode_CodeIsEmpty_ReturnsValidFalse()
    {
        _clockMock
            .Setup(x => x.GetCurrentInstant())
            .Returns(Instant.FromUtc(2025, 01, 11, 0, 0));
        
        GetExchangeRateQuery query = new GetExchangeRateQuery("", new LocalDate(2025, 01, 10));

        ValidationResult? result = _validator.Validate(query);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(x => x.PropertyName == nameof(query.CurrencyCode)), Is.True);
        Assert.That(result.Errors.Any(x => x.ErrorMessage == "Must not be empty."), Is.True);
    }

    [TestCase("A")]
    [TestCase("AA")]
    [TestCase("AAAA")]
    [TestCase("AAAAA")]
    public void CurrencyCode_CodeLengthIsNot3_ReturnsValidFalse(string currencyCode)
    {
        _clockMock
            .Setup(x => x.GetCurrentInstant())
            .Returns(Instant.FromUtc(2025, 01, 11, 0, 0));
        
        GetExchangeRateQuery query = new GetExchangeRateQuery(currencyCode, new LocalDate(2025, 01, 10));

        ValidationResult? result = _validator.Validate(query);
        
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Single().PropertyName, Is.EqualTo(nameof(query.CurrencyCode)));
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Must be 3 characters."));
    }

    [Test]
    public void EffectiveDate_EffectiveDateInFuture_ReturnsValidFalse()
    {
        _clockMock
            .Setup(x => x.GetCurrentInstant())
            .Returns(Instant.FromUtc(2025, 01, 11, 0, 0));
        
        GetExchangeRateQuery query = new GetExchangeRateQuery("USD", new LocalDate(2025, 01, 12));

        ValidationResult? result = _validator.Validate(query);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Single().PropertyName, Is.EqualTo(nameof(query.EffectiveDate)));
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Effective date must be in the past or today."));
    }
    
    [Test]
    public void CurrencyCode_CodeLengthIs3AndEffectiveDateNotInFuture_ReturnsValidTrue()
    {
        _clockMock
            .Setup(x => x.GetCurrentInstant())
            .Returns(Instant.FromUtc(2025, 01, 11, 0, 0));
        
        GetExchangeRateQuery query = new("USD", new LocalDate(2025, 01, 10));

        ValidationResult? result = _validator.Validate(query);

        Assert.That(result.IsValid, Is.True);
    }
}