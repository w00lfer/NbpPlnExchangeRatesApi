using MediatR;
using Microsoft.AspNetCore.Mvc;
using NbpPlnExchangeRates.Api.Common.Errors;
using NbpPlnExchangeRates.Application.Common.Errors;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.Common.Errors;

namespace NbpPlnExchangeRates.Api.Common.Controllers;

[Route("api")]
public class NbpPlnExchangeRatesControllerBase : ControllerBase
{
    private readonly IMediator _mediator;

    public NbpPlnExchangeRatesControllerBase(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected async Task<IActionResult> QueryAsync<T>(IRequest<Result<T>> request) where T: class
    {
        Result<T> result = await _mediator.Send(request);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : GetActionResultBasedOnErrorType(result.Errors, this);
    }
    
    private IActionResult GetActionResultBasedOnErrorType(
        Error[] errors,
        ControllerBase controllerBase)
    {
        var errorsAreOfSameType = errors
            .Select(e => e.GetType())
            .Distinct()
            .Count() > 1;
        
        if (errorsAreOfSameType)
        {
            throw new NotSupportedException("Converting errors of more than one type is not yet supported.");
        }
        
        return errors.First() switch
        {
            ApiError apiError => controllerBase.BadRequest(apiError.ErrorMessage),
            ApplicationError applicationError => controllerBase.BadRequest(applicationError.ErrorMessage),
            DomainError domainError => controllerBase.BadRequest(domainError.ErrorMessage),
            EntityNotFoundError entityNotFoundError => controllerBase.NotFound(entityNotFoundError.ErrorMessage),
            ValidationError validationError => controllerBase.BadRequest(validationError.ErrorMessage),
            _ => throw new NotSupportedException("This error is not yet supported.")
        };
    }
}