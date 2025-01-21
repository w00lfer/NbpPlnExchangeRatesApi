using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NbpPlnExchangeRates.Api.Infrastructure;
using NbpPlnExchangeRates.Infrastructure.Options;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace NbpPlnExchangeRates.Api;

public static class DependencyInjection
{
    public static void AddApiDI(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddResponseCaching();
        
        services
            .AddControllers(options =>
            {
                options.CacheProfiles.Add("Default",
                    new CacheProfile()
                    {
                        Duration = 30
                    });
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });
        
        AddFluentValidation(services);
        
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
        
        AddOptions(builder);
        
        services.ConfigureNbpApiClient();
        
        services.AddOpenApi("v1");
    }

    private static void AddFluentValidation(IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters();
    }
    
    private static void AddOptions(WebApplicationBuilder builder)
    {
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(nameof(DatabaseOptions)));
    }
}