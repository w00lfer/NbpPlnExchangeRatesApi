using FluentValidation.Results;
using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Application.Common.Errors;

public class ValidationError : Error
{
    public ValidationError(IEnumerable<ValidationFailure> failures) 
        : base(string.Join(Environment.NewLine, failures.Select(f => f.ErrorMessage)))
    {
    }
    
    public ValidationError(ValidationFailure failure) : base(failure.ErrorMessage)
    {
    }
}