using MediatR;
using Microsoft.AspNetCore.Mvc;
using NbpPlnExchangeRates.Api.Common.Controllers;
using NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;
using NodaTime;

namespace NbpPlnExchangeRates.Api.Controllers;

[ApiController]
[Route("exchangeRates")]
public class ExchangeRateController: NbpPlnExchangeRatesControllerBase
{
    public ExchangeRateController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("{currencyCode}/{effectiveDate}")]
    [ResponseCache(CacheProfileName = "Default")]
    public Task<IActionResult> GetExchangeRates([FromRoute] string currencyCode, [FromRoute] LocalDate effectiveDate, CancellationToken cancelationToken) => 
        QueryAsync(new GetExchangeRateQuery(currencyCode, effectiveDate), cancelationToken);
}