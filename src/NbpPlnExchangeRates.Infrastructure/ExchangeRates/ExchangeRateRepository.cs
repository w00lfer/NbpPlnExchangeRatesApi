using Microsoft.EntityFrameworkCore;
using NbpPlnExchangeRates.Domain.ExchangeRates;
using NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts;
using NodaTime;

namespace NbpPlnExchangeRates.Infrastructure.ExchangeRates;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ExchangeRateRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Task<ExchangeRate?> GetByCurrencyIdAndEffectiveDateAsync(Guid currencyCodeId, LocalDate effectiveDate, CancellationToken cancellationToken = default) => 
        _applicationDbContext.ExchangeRates
            .SingleOrDefaultAsync(
                x => x.CurrencyCodeId == currencyCodeId && x.EffectiveDate == effectiveDate,
                cancellationToken);
    

    public async Task AddAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        await _applicationDbContext.ExchangeRates.AddAsync(exchangeRate, cancellationToken);
        
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}