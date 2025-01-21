using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Api.Common.Errors;

public class ApiError : Error
{
    public ApiError(string errorMessage) : base(errorMessage)
    {
    }
}