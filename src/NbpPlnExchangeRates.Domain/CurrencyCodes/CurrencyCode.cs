using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Domain.CurrencyCodes;

public class CurrencyCode
{
    private CurrencyCode(string isoCode)
    {
        Id = Guid.CreateVersion7();
        IsoCode = isoCode;
    }

    protected CurrencyCode()
    {}

    public static Result<CurrencyCode> Create(string code)
    {
        var codeLengthValidationResult = ValidateIsoCodeLength(code);
        if (codeLengthValidationResult.IsFailure)
        {
            return Result.Failure<CurrencyCode>(codeLengthValidationResult.Errors);
        }
        
        var codeIsUppercaseValidationResult = ValidateIsoCodeIsUppercase(code);
        return codeIsUppercaseValidationResult.IsSuccess
            ? Result.Success(new CurrencyCode(code))
            : Result.Failure<CurrencyCode>(codeIsUppercaseValidationResult.Errors);
    }
    
    public Guid Id { get; private set; }
    public string IsoCode { get; private set; }
    
    private static Result ValidateIsoCodeLength(string code) =>
        code.Length == 3
            ? Result.Success()
            : new DomainError("IsoCode's code must be exactly 3 characters long.");
    
    private static Result ValidateIsoCodeIsUppercase(string code) =>
        code.All(char.IsUpper)
            ? Result.Success()
            : new DomainError("IsoCode's code must be uppercase.");
}