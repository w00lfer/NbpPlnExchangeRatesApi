using FluentValidation;
using NodaTime;
using NodaTime.Extensions;

namespace NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;

public class GetExchangeRateQueryValidator : AbstractValidator<GetExchangeRateQuery>
{
    public GetExchangeRateQueryValidator(IClock clock)
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Must not be empty.")
            .Length(3).WithMessage("Must be 3 characters.");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .Must(x => x <= clock.InTzdbSystemDefaultZone().GetCurrentDate())
            .WithMessage("Effective date must be in the past or today.");
    }
}