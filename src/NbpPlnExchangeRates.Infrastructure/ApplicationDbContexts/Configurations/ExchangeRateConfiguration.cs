using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NbpPlnExchangeRates.Domain.ExchangeRates;

namespace NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CurrencyCodeId).IsRequired();
        
        builder.Property(x => x.BuyingRate).IsRequired();
        
        builder.Property(x => x.SellingRate).IsRequired();
        
        builder.Property(x => x.EffectiveDate).IsRequired();
    }
}