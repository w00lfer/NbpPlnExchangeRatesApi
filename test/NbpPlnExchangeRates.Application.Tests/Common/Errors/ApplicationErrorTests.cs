using NbpPlnExchangeRates.Application.Common.Errors;

namespace NbpPlnExchangeRates.Application.Tests.Common.Errors;

[TestFixture]
public class ApplicationErrorTests
{
    [Test]
    public void Constructor_AllValid_CreatesDomainErrorWithMessage()
    {
        const string message = "error message";
        ApplicationError applicationError = new ApplicationError(message);
        
        Assert.That(applicationError.ErrorMessage, Is.EqualTo(message));
    }
}