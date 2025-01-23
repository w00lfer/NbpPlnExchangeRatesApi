using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Infrastructure.Common;

namespace NbpPlnExchangeRates.Infrastructure.Tests.Common;

[TestFixture]
public class UnitOfWorkTests : IntegrationTestBase
{
    private UnitOfWork _unitOfWork;
    
    [SetUp]
    public void SetUp()
    {
        _unitOfWork = new(ApplicationDbContext);
    }

    [Test]
    public async Task SaveChangesAsync_AllValid_InvokesApplicationDbContextSaveChanges()
    {
        CurrencyCode currencyCode = CurrencyCode.Create("USD").Value;

        await ApplicationDbContext.AddAsync(currencyCode);
        
        await _unitOfWork.SaveChangesAsync();

        List<CurrencyCode> currencyCodesFromDb = await ApplicationDbContext.CurrencyCodes.ToListAsync();
        
        Assert.That(currencyCodesFromDb.Count, Is.EqualTo(1));
    }

    [Test]
    public void BeginTransaction_AllValid_BeginsTransactionAndReturnsTransaction()
    {
        IDbTransaction transaction = _unitOfWork.BeginTransaction();
        
        Assert.That(transaction, Is.EqualTo(ApplicationDbContext.Database.CurrentTransaction.GetDbTransaction()));
    }
}