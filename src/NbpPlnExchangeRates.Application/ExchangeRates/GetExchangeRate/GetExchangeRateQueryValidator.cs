using FluentValidation;
using NodaTime;
using NodaTime.Extensions;

namespace NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;

public class GetExchangeRateQueryValidator : AbstractValidator<GetExchangeRateQuery>
{
    public GetExchangeRateQueryValidator(IClock clock)
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .Must(x => x <= clock.InTzdbSystemDefaultZone().GetCurrentDate());
    }
}