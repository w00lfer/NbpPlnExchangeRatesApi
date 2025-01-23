using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Infrastructure.CurrencyCodes;
using NbpPlnExchangeRates.Infrastructure.Tests.Common;

namespace NbpPlnExchangeRates.Infrastructure.Tests.CurrencyCodes;

public class CurrencyCodeRepositoryTests : IntegrationTestBase
{
    private CurrencyCodeRepository _currencyCodeRepository;
    
    [SetUp]
    public void SetUp()
    {
        _currencyCodeRepository = new CurrencyCodeRepository(ApplicationDbContext);
    }

    [Test]
    public async Task GetByCodeAsync_Exists_ReturnsEntity()
    {
        const string isoCode = "USD";
        CurrencyCode currencyCode = CurrencyCode.Create(isoCode).Value;
        
        await ApplicationDbContext.AddAsync(currencyCode);
        await ApplicationDbContext.SaveChangesAsync();
        
        CurrencyCode? currencyCodeFromRepo = await _currencyCodeRepository.GetByCodeAsync(isoCode);
        
        
        Assert.That(currencyCodeFromRepo, Is.Not.Null);
        Assert.That(currencyCodeFromRepo, Is.EqualTo(currencyCode));
    }
    
    [Test]
    public async Task GetByCodeAsync_DoesNotExist_ReturnsNull()
    {
        CurrencyCode currencyCode = CurrencyCode.Create("USD").Value;
        
        await ApplicationDbContext.AddAsync(currencyCode);
        await ApplicationDbContext.SaveChangesAsync();
        
        CurrencyCode? currencyCodeFromRepo = await _currencyCodeRepository.GetByCodeAsync("EUR");
        
        
        Assert.That(currencyCodeFromRepo, Is.Null);
    }
    
    [Test]
    public async Task GetByCodeAsync_ThereIsMoreThanOneCurrencyCodeWithSuchEffectiveDateAndCurrencyCodeId_ThrowsException()
    {
        const string isoCode = "USD";
        CurrencyCode currencyCode1 = CurrencyCode.Create(isoCode).Value;
        CurrencyCode currencyCode2 = CurrencyCode.Create(isoCode).Value;
        
        await ApplicationDbContext.AddRangeAsync(currencyCode1, currencyCode2);
        await ApplicationDbContext.SaveChangesAsync();
        
        Assert.ThrowsAsync<InvalidOperationException>(() => _currencyCodeRepository.GetByCodeAsync(isoCode));
    }
}