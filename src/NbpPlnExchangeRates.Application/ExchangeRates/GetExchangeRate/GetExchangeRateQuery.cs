using MediatR;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.Common.Errors;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Domain.ExchangeRates;
using NodaTime;

namespace NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;

public record GetExchangeRateQuery(
    string CurrencyCode,
    LocalDate EffectiveDate)
    : IRequest<Result<ExchangeRateDto>>;

public class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, Result<ExchangeRateDto>>
{
    private static readonly List<LocalDate> FixedPublicHolidays = new() 
    {
        new LocalDate(1, 1, 1),
        new LocalDate(1, 1, 6),
        new LocalDate(1, 5, 1),
        new LocalDate(1, 5, 3),
        new LocalDate(1, 8, 15),
        new LocalDate(1, 11, 1),
        new LocalDate(1, 11, 11),
        new LocalDate(1, 12, 25),
        new LocalDate(1, 12, 26)
    };
    
    private readonly ICurrencyCodeRepository _currencyCodeRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly INbpApiClient _nbpApiClient;

    public GetExchangeRateQueryHandler(
        ICurrencyCodeRepository currencyCodeRepository,
        IExchangeRateRepository exchangeRateRepository,
        INbpApiClient nbpApiClient)
    {
        _currencyCodeRepository = currencyCodeRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _nbpApiClient = nbpApiClient;
    }

    public async Task<Result<ExchangeRateDto>> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var currencyCode = await _currencyCodeRepository.GetByCodeAsync(request.CurrencyCode, cancellationToken);
        if (currencyCode is null)
        {
            return Result.Failure<ExchangeRateDto>(new EntityNotFoundError($"Currency code is invalid for CurrencyCode={request.CurrencyCode}."));
        }
        
        LocalDate sanitisedEffectiveDate = DoesDateFallsOnAWeekend(request.EffectiveDate) || DoesDateFallsOnAPublicHoliday(request.EffectiveDate)
            ? GetLastBusinessDay(request.EffectiveDate)
            : request.EffectiveDate;
        
        var exchangeRate = await _exchangeRateRepository.GetByCurrencyIdAndEffectiveDateAsync(currencyCode.Id, sanitisedEffectiveDate, cancellationToken);
        if (exchangeRate is not null)
        {
            return Result.Success(new ExchangeRateDto(exchangeRate.CurrencyCode.IsoCode, exchangeRate.BuyingRate, exchangeRate.SellingRate, sanitisedEffectiveDate));
        }
        
        return await GetExchangeRateFromApiAndPersistItAsync(currencyCode, sanitisedEffectiveDate, cancellationToken);
    }

    private static LocalDate GetLastBusinessDay(LocalDate date)
    {
        while (DoesDateFallsOnAWeekend(date) || DoesDateFallsOnAPublicHoliday(date))
        {
            date = date.PlusDays(-1);
        }
        return date;
    }
    
    private static bool DoesDateFallsOnAWeekend(LocalDate date)
    {
        var dayOfWeek = date.DayOfWeek;
        
        return dayOfWeek is IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday;
    }
    
    private static bool DoesDateFallsOnAPublicHoliday(LocalDate date) =>
        FixedPublicHolidays.Any(holiday => holiday.Month == date.Month && holiday.Day == date.Day);

    private static Result<ExchangeRateDto> MapExchangeRateToExchangeRateDto(ExchangeRate exchangeRate) =>
        Result.Success(new ExchangeRateDto(
            exchangeRate.CurrencyCode.IsoCode,
            exchangeRate.BuyingRate,
            exchangeRate.SellingRate,
            exchangeRate.EffectiveDate));
    
    private async Task<Result<ExchangeRateDto>> GetExchangeRateFromApiAndPersistItAsync(CurrencyCode currencyCode, LocalDate effectiveDate, CancellationToken cancellationToken)
    {
        var getCurrencyExchangeRateResult = await _nbpApiClient.GetCurrencyExchangeRate(currencyCode.IsoCode, effectiveDate, cancellationToken);

        if (getCurrencyExchangeRateResult.IsFailure)
        {
            return Result.Failure<ExchangeRateDto>(getCurrencyExchangeRateResult.Errors);
        }
        
        var createExchangeRateResult = ExchangeRate.Create(
            currencyCode,
            getCurrencyExchangeRateResult.Value.Bid,
            getCurrencyExchangeRateResult.Value.Ask,
            getCurrencyExchangeRateResult.Value.EffectiveDate);

        if (createExchangeRateResult.IsFailure)
        {
            return Result.Failure<ExchangeRateDto>(createExchangeRateResult.Errors);
        }
        
        await _exchangeRateRepository.AddAsync(createExchangeRateResult.Value, cancellationToken);
        
        return MapExchangeRateToExchangeRateDto(createExchangeRateResult.Value);
    }
}