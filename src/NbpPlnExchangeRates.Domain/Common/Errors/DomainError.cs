namespace NbpPlnExchangeRates.Domain.Common.Errors;

public class DomainError : Error
{
    public DomainError(string errorMessage) : base(errorMessage)
    {
    }
}