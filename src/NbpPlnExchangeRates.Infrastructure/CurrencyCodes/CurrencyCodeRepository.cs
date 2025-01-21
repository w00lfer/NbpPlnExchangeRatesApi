using Microsoft.EntityFrameworkCore;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts;

namespace NbpPlnExchangeRates.Infrastructure.CurrencyCodes;

public class CurrencyCodeRepository : ICurrencyCodeRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CurrencyCodeRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Task<CurrencyCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _applicationDbContext.CurrencyCodes
            .SingleOrDefaultAsync(
                x => x.IsoCode == code,
                cancellationToken);
}