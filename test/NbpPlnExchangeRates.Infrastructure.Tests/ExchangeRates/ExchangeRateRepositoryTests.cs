using Microsoft.EntityFrameworkCore;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Domain.ExchangeRates;
using NbpPlnExchangeRates.Infrastructure.ExchangeRates;
using NbpPlnExchangeRates.Infrastructure.Tests.Common;
using NodaTime;

namespace NbpPlnExchangeRates.Infrastructure.Tests.ExchangeRates;

[TestFixture]
public class ExchangeRateRepositoryTests : IntegrationTestBase
{
    private ExchangeRateRepository _exchangeRateRepository;
    private CurrencyCode _currencyCode;
    
    [SetUp]
    public async Task SetUp()
    {
        _exchangeRateRepository = new ExchangeRateRepository(ApplicationDbContext);
        
        CurrencyCode currencyCode = CurrencyCode.Create("USD").Value;
        await ApplicationDbContext.AddAsync(currencyCode);
        await ApplicationDbContext.SaveChangesAsync();
        
        _currencyCode = currencyCode;
    }

    [Test]
    public async Task GetByCurrencyIdAndEffectiveDateAsync_Exists_ReturnsEntity()
    {
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        ExchangeRate exchangeRate = ExchangeRate.Create(
            _currencyCode, 
            3.5m, 
            3.7m, 
            effectiveDate).Value;
        
        await ApplicationDbContext.AddAsync(exchangeRate);
        await ApplicationDbContext.SaveChangesAsync();
        
        ExchangeRate? exchangeRateFromRepo = await _exchangeRateRepository.GetByCurrencyIdAndEffectiveDateAsync(_currencyCode.Id, effectiveDate);
        
        
        Assert.That(exchangeRateFromRepo, Is.Not.Null);
        Assert.That(exchangeRateFromRepo, Is.EqualTo(exchangeRate));
    }
    
    [Test]
    public async Task GetByCurrencyIdAndEffectiveDateAsync_DoesNotExist_ReturnsNull()
    {
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        ExchangeRate exchangeRate = ExchangeRate.Create(
            _currencyCode, 
            3.5m, 
            3.7m, 
            effectiveDate).Value;
        
        await ApplicationDbContext.AddAsync(exchangeRate);
        await ApplicationDbContext.SaveChangesAsync();
        
        ExchangeRate? exchangeRateFromRepo = await _exchangeRateRepository.GetByCurrencyIdAndEffectiveDateAsync(Guid.CreateVersion7(), effectiveDate);
        
        
        Assert.That(exchangeRateFromRepo, Is.Null);
    }
    
    [Test]
    public async Task GetByCurrencyIdAndEffectiveDateAsync_ThereIsMoreThanOneExchangeRateWithSuchEffectiveDateAndCurrencyCodeId_ThrowsException()
    {
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        ExchangeRate exchangeRate1 = ExchangeRate.Create(
            _currencyCode, 
            3.5m, 
            3.7m, 
            effectiveDate).Value;
        ExchangeRate exchangeRate2 = ExchangeRate.Create(
            _currencyCode, 
            3.5m, 
            3.7m, 
            effectiveDate).Value;
        
        await ApplicationDbContext.AddRangeAsync(exchangeRate1, exchangeRate2);
        await ApplicationDbContext.SaveChangesAsync();
        
        Assert.ThrowsAsync<InvalidOperationException>(() => _exchangeRateRepository.GetByCurrencyIdAndEffectiveDateAsync(_currencyCode.Id, effectiveDate));
    }
    
    [Test]
    public async Task AddAsync_AllValid_AddsToDbContextAndSavesChanges()
    {
        ExchangeRate exchangeRate = ExchangeRate.Create(
            _currencyCode, 
            3.5m, 
            3.7m, 
            new LocalDate(2025, 01, 20)).Value;
        
        await _exchangeRateRepository.AddAsync(exchangeRate);
        
        IEnumerable<ExchangeRate> exchangeRatesFromDb = await ApplicationDbContext.ExchangeRates.ToListAsync();
        
        Assert.That(exchangeRatesFromDb.Count(), Is.EqualTo(1));
        Assert.That(exchangeRatesFromDb.Single(), Is.EqualTo(exchangeRate));
    }
}