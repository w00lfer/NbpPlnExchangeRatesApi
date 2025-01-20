using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Application.Common.Errors;

public class ApplicationError : Error
{
    public ApplicationError(string errorMessage) : base(errorMessage)
    {
    }
}