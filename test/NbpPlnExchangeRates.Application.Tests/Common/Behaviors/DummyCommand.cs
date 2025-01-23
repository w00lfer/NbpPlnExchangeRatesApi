using MediatR;
using NbpPlnExchangeRates.Domain.Common;

namespace NbpPlnExchangeRates.Application.Tests.Common.Behaviors;


public sealed record DummyCommand()
    : IRequest<Result>;