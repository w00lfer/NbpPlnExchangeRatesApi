namespace NbpPlnExchangeRates.Domain.Common.Errors;

public class EntityNotFoundError : Error
{
    public EntityNotFoundError(string errorMessage) : base(errorMessage)
    {
    }
    
}