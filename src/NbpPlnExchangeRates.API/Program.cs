using NbpPlnExchangeRates.Application;
using NbpPlnExchangeRates.Domain;
using NbpPlnExchangeRates.Infrastructure;
using Scalar.AspNetCore;

namespace NbpPlnExchangeRates.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddApiDI(builder);
        builder.Services.AddApplicationDI();
        builder.Services.AddInfrastructureDI();
        builder.Services.AddDomainDI();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            
            app.MapScalarApiReference(options =>
            {
                options.Servers = [];
            });
        }

        app.UseHttpsRedirection();

        app.UseResponseCaching();
        
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}