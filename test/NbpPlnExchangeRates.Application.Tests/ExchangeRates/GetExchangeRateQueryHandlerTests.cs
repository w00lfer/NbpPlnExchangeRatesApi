using Moq;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.CurrencyCodes;
using NbpPlnExchangeRates.Domain.ExchangeRates;
using NodaTime;

namespace NbpPlnExchangeRates.Application.Tests.ExchangeRates;

[TestFixture]
public class GetExchangeRateQueryHandlerTests
{
    private Mock<ICurrencyCodeRepository> _currencyCodeRepositoryMock = new();
    private Mock<IExchangeRateRepository> _exchangeRateRepositoryMock = new();
    private Mock<INbpApiClient> _nbpApiClientMock = new();


    private GetExchangeRateQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _handler = new(_currencyCodeRepositoryMock.Object, _exchangeRateRepositoryMock.Object, _nbpApiClientMock.Object);
        
        _currencyCodeRepositoryMock.Reset();
        _exchangeRateRepositoryMock.Reset();
        _nbpApiClientMock.Reset();
    }

    [Test]
    public async Task Handle_RequestCurrencyCodeCannotBeFound_ReturnsEntityNotFoundError()
    {
        const string currencyCode = "QQQ";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);


        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, CancellationToken.None))
            .ReturnsAsync((CurrencyCode)null);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Currency code is invalid for CurrencyCode={currencyCode}."));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, CancellationToken.None), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWeekendAndExchangeRateExists_ReturnsMappedExchangeRate()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 19);
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);


        Guid currencyCodeId = Guid.CreateVersion7();
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.Id == currencyCodeId && x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, CancellationToken.None))
            .ReturnsAsync(currencyCodeEntity);

        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend = new LocalDate(2025, 01, 17);
        ExchangeRate exchangeRate = Mock.Of<ExchangeRate>(x => x.CurrencyCode == currencyCodeEntity
                                                               && x.EffectiveDate == lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend
                                                               && x.BuyingRate == buyingRate
                                                               && x.SellingRate == sellingRate);
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, CancellationToken.None))
            .ReturnsAsync(exchangeRate);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, CancellationToken.None);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo(new ExchangeRateDto(currencyCode, buyingRate, sellingRate, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend)));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, CancellationToken.None), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, CancellationToken.None), Times.Once);
        
        VerifyNoOtherCalls();
    }
    

    private void VerifyNoOtherCalls()
    {
        _currencyCodeRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateRepositoryMock.VerifyNoOtherCalls();
        _nbpApiClientMock.VerifyNoOtherCalls();
    }
}