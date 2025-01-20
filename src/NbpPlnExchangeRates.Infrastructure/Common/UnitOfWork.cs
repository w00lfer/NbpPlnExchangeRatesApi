using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts;

namespace NbpPlnExchangeRates.Infrastructure.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _applicationDbContext;

    public UnitOfWork(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = _applicationDbContext.Database.BeginTransaction();

        return transaction.GetDbTransaction();
    }
}