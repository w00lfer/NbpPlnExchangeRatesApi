using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NbpPlnExchangeRates.Domain.CurrencyCodes;

namespace NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts.Configurations;

public class CurrencyCodeConfiguration : IEntityTypeConfiguration<CurrencyCode>
{
    public void Configure(EntityTypeBuilder<CurrencyCode> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder
            .Property(x => x.IsoCode)
            .IsRequired()
            .HasMaxLength(3);
    }
}