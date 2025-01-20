using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Infrastructure.ApplicationDbContexts;
using NbpPlnExchangeRates.Infrastructure.Common;
using NbpPlnExchangeRates.Infrastructure.Options;

namespace NbpPlnExchangeRates.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureDI(this IServiceCollection services)
    {
        services.AddHostedService<AppInitializer>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        AddDb(services);
        
    }
    
    private static void AddDb(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            var databaseOptions = serviceProvider.GetService<IOptions<DatabaseOptions>>().Value;
                
            optionsBuilder.UseSqlServer(databaseOptions.DatabaseConnectionString, sqlServerOptionsAction =>
            {
                sqlServerOptionsAction.UseNodaTime();
                
                sqlServerOptionsAction.CommandTimeout(databaseOptions.CommandTimeout);
            });
        
            optionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
        
            optionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnablesSensitiveDataLogging);
        });
    }
}