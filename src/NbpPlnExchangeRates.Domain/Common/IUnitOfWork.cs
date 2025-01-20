using System.Data;

namespace NbpPlnExchangeRates.Domain.Common;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    IDbTransaction BeginTransaction();
}