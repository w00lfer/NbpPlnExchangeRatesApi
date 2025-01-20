using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.IsoCodes;

namespace NbpPlnExchangeRates.Infrastructure.Seeds;

public static class CurrencyCodeSeeder
{
    public static async Task SeedAsync(DbContext context, ILogger? logger)
    {
        if (context.Set<CurrencyCode>().Any())
            return;

        var currencyCodeCreatesForSeed = GetCurrencyCodeCreatesForSeed();
        if (logger is not null && currencyCodeCreatesForSeed.Any(c => c.IsFailure))
        {
            LogFailures(currencyCodeCreatesForSeed, logger);
        }

        context.Set<CurrencyCode>().AddRange(currencyCodeCreatesForSeed.Where(c => c.IsSuccess).Select(c => c.Value));
        await context.SaveChangesAsync();
    }

    public static void Seed(DbContext context, ILogger? logger)
    {
        if (context.Set<CurrencyCode>().Any())
            return;

        var currencyCodeCreatesForSeed = GetCurrencyCodeCreatesForSeed();
        if (logger is not null && currencyCodeCreatesForSeed.Any(c => c.IsFailure))
        {
            LogFailures(currencyCodeCreatesForSeed, logger);
        }

        context.Set<CurrencyCode>().AddRange(currencyCodeCreatesForSeed.Where(c => c.IsSuccess).Select(c => c.Value));
        context.SaveChanges();
    }

    private static List<Result<CurrencyCode>> GetCurrencyCodeCreatesForSeed() =>
        new List<Result<CurrencyCode>>()
        {
            CurrencyCode.Create("AED"),
            CurrencyCode.Create("AFN"),
            CurrencyCode.Create("ALL"),
            CurrencyCode.Create("AMD"),
            CurrencyCode.Create("ANG"),
            CurrencyCode.Create("AOA"),
            CurrencyCode.Create("ARS"),
            CurrencyCode.Create("AUD"),
            CurrencyCode.Create("AWG"),
            CurrencyCode.Create("AZN"),
            CurrencyCode.Create("BAM"),
            CurrencyCode.Create("BBD"),
            CurrencyCode.Create("BDT"),
            CurrencyCode.Create("BGN"),
            CurrencyCode.Create("BHD"),
            CurrencyCode.Create("BIF"),
            CurrencyCode.Create("BMD"),
            CurrencyCode.Create("BND"),
            CurrencyCode.Create("BOB"),
            CurrencyCode.Create("BOV"),
            CurrencyCode.Create("BRL"),
            CurrencyCode.Create("BSD"),
            CurrencyCode.Create("BTN"),
            CurrencyCode.Create("BWP"),
            CurrencyCode.Create("BYN"),
            CurrencyCode.Create("BZD"),
            CurrencyCode.Create("CAD"),
            CurrencyCode.Create("CDF"),
            CurrencyCode.Create("CHE"),
            CurrencyCode.Create("CHF"),
            CurrencyCode.Create("CHW"),
            CurrencyCode.Create("CLF"),
            CurrencyCode.Create("CLP"),
            CurrencyCode.Create("CNY"),
            CurrencyCode.Create("COP"),
            CurrencyCode.Create("COU"),
            CurrencyCode.Create("CRC"),
            CurrencyCode.Create("CUP"),
            CurrencyCode.Create("CVE"),
            CurrencyCode.Create("CZK"),
            CurrencyCode.Create("DJF"),
            CurrencyCode.Create("DKK"),
            CurrencyCode.Create("DOP"),
            CurrencyCode.Create("DZD"),
            CurrencyCode.Create("EGP"),
            CurrencyCode.Create("ERN"),
            CurrencyCode.Create("ETB"),
            CurrencyCode.Create("EUR"),
            CurrencyCode.Create("FJD"),
            CurrencyCode.Create("FKP"),
            CurrencyCode.Create("GBP"),
            CurrencyCode.Create("GEL"),
            CurrencyCode.Create("GHS"),
            CurrencyCode.Create("GIP"),
            CurrencyCode.Create("GMD"),
            CurrencyCode.Create("GNF"),
            CurrencyCode.Create("GTQ"),
            CurrencyCode.Create("GYD"),
            CurrencyCode.Create("HKD"),
            CurrencyCode.Create("HNL"),
            CurrencyCode.Create("HTG"),
            CurrencyCode.Create("HUF"),
            CurrencyCode.Create("IDR"),
            CurrencyCode.Create("ILS"),
            CurrencyCode.Create("INR"),
            CurrencyCode.Create("IQD"),
            CurrencyCode.Create("IRR"),
            CurrencyCode.Create("ISK"),
            CurrencyCode.Create("JMD"),
            CurrencyCode.Create("JOD"),
            CurrencyCode.Create("JPY"),
            CurrencyCode.Create("KES"),
            CurrencyCode.Create("KGS"),
            CurrencyCode.Create("KHR"),
            CurrencyCode.Create("KMF"),
            CurrencyCode.Create("KPW"),
            CurrencyCode.Create("KRW"),
            CurrencyCode.Create("KWD"),
            CurrencyCode.Create("KYD"),
            CurrencyCode.Create("KZT"),
            CurrencyCode.Create("LAK"),
            CurrencyCode.Create("LBP"),
            CurrencyCode.Create("LKR"),
            CurrencyCode.Create("LRD"),
            CurrencyCode.Create("LSL"),
            CurrencyCode.Create("LYD"),
            CurrencyCode.Create("MAD"),
            CurrencyCode.Create("MDL"),
            CurrencyCode.Create("MGA"),
            CurrencyCode.Create("MKD"),
            CurrencyCode.Create("MMK"),
            CurrencyCode.Create("MNT"),
            CurrencyCode.Create("MOP"),
            CurrencyCode.Create("MRU"),
            CurrencyCode.Create("MUR"),
            CurrencyCode.Create("MVR"),
            CurrencyCode.Create("MWK"),
            CurrencyCode.Create("MXN"),
            CurrencyCode.Create("MXV"),
            CurrencyCode.Create("MYR"),
            CurrencyCode.Create("MZN"),
            CurrencyCode.Create("NAD"),
            CurrencyCode.Create("NGN"),
            CurrencyCode.Create("NIO"),
            CurrencyCode.Create("NOK"),
            CurrencyCode.Create("NPR"),
            CurrencyCode.Create("NZD"),
            CurrencyCode.Create("OMR"),
            CurrencyCode.Create("PAB"),
            CurrencyCode.Create("PEN"),
            CurrencyCode.Create("PGK"),
            CurrencyCode.Create("PHP"),
            CurrencyCode.Create("PKR"),
            CurrencyCode.Create("PLN"),
            CurrencyCode.Create("PYG"),
            CurrencyCode.Create("QAR"),
            CurrencyCode.Create("RON"),
            CurrencyCode.Create("RSD"),
            CurrencyCode.Create("RUB"),
            CurrencyCode.Create("RWF"),
            CurrencyCode.Create("SAR"),
            CurrencyCode.Create("SBD"),
            CurrencyCode.Create("SCR"),
            CurrencyCode.Create("SDG"),
            CurrencyCode.Create("SEK"),
            CurrencyCode.Create("SGD"),
            CurrencyCode.Create("SHP"),
            CurrencyCode.Create("SLE"),
            CurrencyCode.Create("SOS"),
            CurrencyCode.Create("SRD"),
            CurrencyCode.Create("SSP"),
            CurrencyCode.Create("STN"),
            CurrencyCode.Create("SVC"),
            CurrencyCode.Create("SYP"),
            CurrencyCode.Create("SZL"),
            CurrencyCode.Create("THB"),
            CurrencyCode.Create("TJS"),
            CurrencyCode.Create("TMT"),
            CurrencyCode.Create("TND"),
            CurrencyCode.Create("TOP"),
            CurrencyCode.Create("TRY"),
            CurrencyCode.Create("TTD"),
            CurrencyCode.Create("TWD"),
            CurrencyCode.Create("TZS"),
            CurrencyCode.Create("UAH"),
            CurrencyCode.Create("UGX"),
            CurrencyCode.Create("USD"),
            CurrencyCode.Create("USN"),
            CurrencyCode.Create("UYI"),
            CurrencyCode.Create("UYU"),
            CurrencyCode.Create("UYW"),
            CurrencyCode.Create("UZS"),
            CurrencyCode.Create("VED"),
            CurrencyCode.Create("VES"),
            CurrencyCode.Create("VND"),
            CurrencyCode.Create("VUV"),
            CurrencyCode.Create("WST"),
            CurrencyCode.Create("XAF"),
            CurrencyCode.Create("XAG"),
            CurrencyCode.Create("XAU"),
            CurrencyCode.Create("XBA"),
            CurrencyCode.Create("XBB"),
            CurrencyCode.Create("XBC"),
            CurrencyCode.Create("XBD"),
            CurrencyCode.Create("XCD"),
            CurrencyCode.Create("XDR"),
            CurrencyCode.Create("XOF"),
            CurrencyCode.Create("XPD"),
            CurrencyCode.Create("XPF"),
            CurrencyCode.Create("XPT"),
            CurrencyCode.Create("XSU"),
            CurrencyCode.Create("XTS"),
            CurrencyCode.Create("XUA"),
            CurrencyCode.Create("XXX"),
            CurrencyCode.Create("YER"),
            CurrencyCode.Create("ZAR"),
            CurrencyCode.Create("ZMW"),
            CurrencyCode.Create("ZWG")
        };
    
    private static void LogFailures(List<Result<CurrencyCode>> currencyCodeCreatesForSeed, ILogger logger)
    {
        foreach (Result<CurrencyCode> currencyCodeCreate in currencyCodeCreatesForSeed.Where(c => c.IsFailure))
        {
            logger.LogError("{Message}", currencyCodeCreate.AggregatedErrorMessages);
        }
    }
}