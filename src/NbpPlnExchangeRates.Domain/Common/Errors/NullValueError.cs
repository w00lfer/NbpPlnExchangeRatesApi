namespace NbpPlnExchangeRates.Domain.Common.Errors;

public class NullValueError: Error
{
    public NullValueError() : base("Null value provided")
    {
    }
}