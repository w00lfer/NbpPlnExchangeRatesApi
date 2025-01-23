using NbpPlnExchangeRates.Api.Common.Errors;

namespace NbpPlnExchangeRates.API.Tests.Common;

[TestFixture]
public class ApiErrorTests
{
    [Test]
    public void Constructor_AllValid_CreatesDomainErrorWithMessage()
    {
        const string message = "error message";
        ApiError apiError = new ApiError(message);
        
        Assert.That(apiError.ErrorMessage, Is.EqualTo(message));
    }
}