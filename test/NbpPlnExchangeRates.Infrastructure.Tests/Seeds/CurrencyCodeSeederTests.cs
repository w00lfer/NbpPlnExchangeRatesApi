using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NbpPlnExchangeRates.Infrastructure.Seeds;
using NbpPlnExchangeRates.Infrastructure.Tests.Common;

namespace NbpPlnExchangeRates.Infrastructure.Tests.Seeds;

[TestFixture]
public class CurrencyCodeSeederTests : IntegrationTestBase
{
    private readonly Mock<ILogger> _loggerMock = new();
    
    [Test]
    public async Task SeedAsync_AllOk_SeedsCurrencyCodes()
    {
        
        await CurrencyCodeSeeder.SeedAsync(ApplicationDbContext, _loggerMock.Object);
        
        var currencyCodes = await ApplicationDbContext.CurrencyCodes.ToListAsync();
        Assert.That(currencyCodes.Count, Is.EqualTo(178));
    }
    
    [Test]
    public async Task Seed_AllOk_SeedsCurrencyCodes()
    {
        
        CurrencyCodeSeeder.Seed(ApplicationDbContext, _loggerMock.Object);
        
        var currencyCodes = await ApplicationDbContext.CurrencyCodes.ToListAsync();
        Assert.That(currencyCodes.Count, Is.EqualTo(178));
    }
}