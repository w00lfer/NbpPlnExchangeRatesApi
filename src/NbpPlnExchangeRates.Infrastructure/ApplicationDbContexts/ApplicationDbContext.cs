using Microsoft.EntityFrameworkCore;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Domain.ExchangeRates;

namespace NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    public DbSet<CurrencyCode> CurrencyCodes { get; set; }
}