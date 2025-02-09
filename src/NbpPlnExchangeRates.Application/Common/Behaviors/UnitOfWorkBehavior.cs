using MediatR;
using NbpPlnExchangeRates.Domain.Common;

namespace NbpPlnExchangeRates.Application.Common.Behaviors;

internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using (var transaction = _unitOfWork.BeginTransaction())
        {
            TResponse response = await next();
            
            if (response.IsFailure)
            {
                transaction.Rollback();
                return response;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();
            
            return response;
        }
    }
}