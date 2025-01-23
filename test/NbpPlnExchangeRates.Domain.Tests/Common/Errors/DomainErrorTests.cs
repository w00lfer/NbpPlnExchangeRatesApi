using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Domain.Tests.Common.Errors;

[TestFixture]
public class DomainErrorTests
{
    [Test]
    public void Constructor_AllValid_CreatesDomainErrorWithMessage()
    {
        const string message = "error message";
        DomainError domainError = new DomainError(message);
        
        Assert.That(domainError.ErrorMessage, Is.EqualTo(message));
    }
}