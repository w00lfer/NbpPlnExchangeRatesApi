using NbpPlnExchangeRates.Application;
using NbpPlnExchangeRates.Domain;
using NbpPlnExchangeRates.Infrastructure;
using Scalar.AspNetCore;

namespace NbpPlnExchangeRatesApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddApiDI();
        builder.Services.AddApplicationDI();
        builder.Services.AddInfrastructureDI();
        builder.Services.AddDomainDI();
        
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi("v1");

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

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}