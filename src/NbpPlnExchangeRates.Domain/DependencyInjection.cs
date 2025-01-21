using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace NbpPlnExchangeRates.Domain;

public static class DependencyInjection
{
    public static void AddDomainDI(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(c => SystemClock.Instance);
    }
}