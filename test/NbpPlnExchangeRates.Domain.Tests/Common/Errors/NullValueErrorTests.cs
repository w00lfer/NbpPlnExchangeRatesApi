using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Domain.Tests.Common.Errors;

[TestFixture]
public class NullValueErrorTests
{
    [Test]
    public void Constructor_AllValid_CreatesNullValueErrorWithMessage()
    {
        NullValueError nullValueError = new NullValueError();
        
        Assert.That(nullValueError.ErrorMessage, Is.EqualTo("Null value provided"));
    }
}