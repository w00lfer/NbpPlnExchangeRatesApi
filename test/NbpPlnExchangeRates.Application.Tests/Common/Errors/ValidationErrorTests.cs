using FluentValidation.Results;
using NbpPlnExchangeRates.Application.Common.Errors;

namespace NbpPlnExchangeRates.Application.Tests.Common.Errors;

[TestFixture]
public class ValidationErrorTests
{
    [Test]
    public void Constructor_AllValid_CreatesDomainErrorWithMessage()
    {
        const string message = "error message";
        ValidationError validationError = new ValidationError(new ValidationFailure("propertyName", message));
        
        Assert.That(validationError.ErrorMessage, Is.EqualTo(message));
    }
}