using Moq;
using NbpPlnExchangeRates.Application.ApiClients;
using NbpPlnExchangeRates.Application.Common.Errors;
using NbpPlnExchangeRates.Application.ExchangeRates.GetExchangeRate;
using NbpPlnExchangeRates.Domain.Common;
using NbpPlnExchangeRates.Domain.Common.Errors;
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
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync((CurrencyCode)null);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo($"Currency code is invalid for CurrencyCode={currencyCode}."));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWeekendAndExchangeRateExists_ReturnsMappedExchangeRateWithDifferentEffectiveDateThanTheOneFromRequest()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 19);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);


        Guid currencyCodeId = Guid.CreateVersion7();
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.Id == currencyCodeId && x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);

        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend = new LocalDate(2025, 01, 17);
        ExchangeRate exchangeRate = Mock.Of<ExchangeRate>(x => x.CurrencyCode == currencyCodeEntity
                                                               && x.EffectiveDate == lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend
                                                               && x.BuyingRate == buyingRate
                                                               && x.SellingRate == sellingRate);
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken))
            .ReturnsAsync(exchangeRate);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo(new ExchangeRateDto(currencyCode, buyingRate, sellingRate, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend)));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test, TestCaseSource(nameof(PublicHolidayTestCases))]
    public async Task Handle_DateFallsOnPublicHolidayAndExchangeRateExists_ReturnsMappedExchangeRateWithDifferentEffectiveDateThanTheOneFromRequest((LocalDate effectiveDate, LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday) testCase)
    {
        LocalDate effectiveDate = testCase.effectiveDate;
        LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday = testCase.lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday;
        const string currencyCode = "USD";
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);


        Guid currencyCodeId = Guid.CreateVersion7();
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.Id == currencyCodeId && x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);

        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        ExchangeRate exchangeRate = Mock.Of<ExchangeRate>(x => x.CurrencyCode == currencyCodeEntity
                                                               && x.EffectiveDate == lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday
                                                               && x.BuyingRate == buyingRate
                                                               && x.SellingRate == sellingRate);
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken))
            .ReturnsAsync(exchangeRate);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo(new ExchangeRateDto(currencyCode, buyingRate, sellingRate, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday)));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
        
    [Test]
    public async Task Handle_DateFallsOnWorkdayAndExchangeRateExists_ReturnsMappedExchangeRate()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);


        Guid currencyCodeId = Guid.CreateVersion7();
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.Id == currencyCodeId && x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);

        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        ExchangeRate exchangeRate = Mock.Of<ExchangeRate>(x => x.CurrencyCode == currencyCodeEntity
                                                               && x.EffectiveDate == effectiveDate
                                                               && x.BuyingRate == buyingRate
                                                               && x.SellingRate == sellingRate);
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken))
            .ReturnsAsync(exchangeRate);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo(new ExchangeRateDto(currencyCode, buyingRate, sellingRate, effectiveDate)));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWorkdayAndExchangeRateDoesNotExistAndNbpApiClientReturnsError_ReturnsErrorFromApiClient()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);

        Error error = new ApplicationError("error");
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken))
            .ReturnsAsync(error);
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single(), Is.EqualTo(error));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWorkdayAndExchangeRateDoesNotExistAndExchangeRateCreateReturnsErrorBecauseBuyingIsNegative_ReturnsExchangeRateCreateError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);
        
        const decimal buyingRate = -3.5m;
        const decimal sellingRate = 3.6m;
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(currencyCode, effectiveDate, buyingRate, sellingRate);
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken))
            .ReturnsAsync(Result.Success(nbpApiClientExchangeCurrencyRateDto));
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Rate must be greater or equal to 0."));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWorkdayAndExchangeRateDoesNotExistAndExchangeRateCreateReturnsErrorBecauseSellingIsNegative_ReturnsExchangeRateCreateError()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);
        
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = -3.6m;
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(currencyCode, effectiveDate, buyingRate, sellingRate);
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken))
            .ReturnsAsync(Result.Success(nbpApiClientExchangeCurrencyRateDto));
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsFailure);
        Assert.That(result.Errors.Single().ErrorMessage, Is.EqualTo("Rate must be greater or equal to 0."));
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task Handle_DateFallsOnWorkdayAndExchangeRateDoesNotExistAndExchangeRateCreateIsOk_AddNewExchangeRateToRepoAndReturnsMappedExchangeRate()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 20);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);
        
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(currencyCode, effectiveDate, buyingRate, sellingRate);
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken))
            .ReturnsAsync(Result.Success(nbpApiClientExchangeCurrencyRateDto));
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, effectiveDate, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, effectiveDate, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ExchangeRate>(y => y.CurrencyCode == currencyCodeEntity 
                                     && y.EffectiveDate == effectiveDate 
                                     && y.BuyingRate == buyingRate 
                                     && y.SellingRate == sellingRate),
            cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
        
    [Test]
    public async Task Handle_DateFallsOnWeekendAndExchangeRateDoesNotExistAndExchangeRateCreateIsOk_AddNewExchangeRateToRepoAndReturnsMappedExchangeRateWithDifferentEffectiveDateThanTheOneFromRequest()
    {
        const string currencyCode = "USD";
        LocalDate effectiveDate = new LocalDate(2025, 01, 19);
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend = new LocalDate(2025, 01, 17);
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);
        
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, buyingRate, sellingRate);
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken))
            .ReturnsAsync(Result.Success(nbpApiClientExchangeCurrencyRateDto));
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ExchangeRate>(y => y.CurrencyCode == currencyCodeEntity 
                                     && y.EffectiveDate == lastWorkingDayBeforeEffectiveDateWhichFallsOnWeekend 
                                     && y.BuyingRate == buyingRate 
                                     && y.SellingRate == sellingRate),
            cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    [Test, TestCaseSource(nameof(PublicHolidayTestCases))]
    public async Task Handle_DateFallsOnPublicHolidayAndExchangeRateDoesNotExistAndExchangeRateCreateIsOk_AddNewExchangeRateToRepoAndReturnsMappedExchangeRateWithDifferentEffectiveDateThanTheOneFromRequest((LocalDate effectiveDate, LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday) testCase)
    {
        LocalDate effectiveDate = testCase.effectiveDate;
        LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday = testCase.lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday;
        const string currencyCode = "USD";
        CancellationToken cancellationToken = CancellationToken.None;
        GetExchangeRateQuery query = new(currencyCode, effectiveDate);

        
        CurrencyCode currencyCodeEntity = Mock.Of<CurrencyCode>(x => x.IsoCode == currencyCode);
        _currencyCodeRepositoryMock
            .Setup(repo => repo.GetByCodeAsync(currencyCode, cancellationToken))
            .ReturnsAsync(currencyCodeEntity);
        
        _exchangeRateRepositoryMock
            .Setup(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken))
            .ReturnsAsync((ExchangeRate)null);
        
        const decimal buyingRate = 3.5m;
        const decimal sellingRate = 3.6m;
        NbpApiClientExchangeCurrencyRateDto nbpApiClientExchangeCurrencyRateDto = new(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, buyingRate, sellingRate);
        _nbpApiClientMock
            .Setup(x => x.GetCurrencyExchangeRate(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken))
            .ReturnsAsync(Result.Success(nbpApiClientExchangeCurrencyRateDto));
        
        Result<ExchangeRateDto> result = await _handler.Handle(query, cancellationToken);

        Assert.That(result.IsSuccess);
        
        _currencyCodeRepositoryMock.Verify(repo => repo.GetByCodeAsync(currencyCode, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.GetByCurrencyIdAndEffectiveDateAsync(currencyCodeEntity.Id, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken), Times.Once);
        _nbpApiClientMock.Verify(x => x.GetCurrencyExchangeRate(currencyCode, lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday, cancellationToken), Times.Once);
        _exchangeRateRepositoryMock.Verify(x => x.AddAsync(
            It.Is<ExchangeRate>(y => y.CurrencyCode == currencyCodeEntity 
                                     && y.EffectiveDate == lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday 
                                     && y.BuyingRate == buyingRate 
                                     && y.SellingRate == sellingRate),
            cancellationToken), Times.Once);
        
        VerifyNoOtherCalls();
    }
    
    private static IEnumerable<(LocalDate effectiveDate, LocalDate lastWorkingDayBeforeEffectiveDateWhichFallsOnPublicHoliday)> PublicHolidayTestCases
    {
        get
        {
            yield return new(new LocalDate(2025, 1, 1), new LocalDate(2024, 12, 31));
            yield return new(new LocalDate(2025, 1, 6), new LocalDate(2025, 1, 3));
            yield return new(new LocalDate(2025, 5, 1), new LocalDate(2025, 4, 30));
            yield return new(new LocalDate(2025, 5, 3), new LocalDate(2025, 5, 2));
            yield return new(new LocalDate(2025, 8, 15), new LocalDate(2025, 8, 14));
            yield return new(new LocalDate(2025, 11, 1), new LocalDate(2025, 10, 31));
            yield return new(new LocalDate(2025, 11, 11), new LocalDate(2025, 11, 10));
            yield return new(new LocalDate(2025, 12, 24), new LocalDate(2025, 12, 23));
            yield return new(new LocalDate(2025, 12, 25), new LocalDate(2025, 12, 23));
            yield return new(new LocalDate(2025, 12, 26), new LocalDate(2025, 12, 23));
        }
    }
    
    private void VerifyNoOtherCalls()
    {
        _currencyCodeRepositoryMock.VerifyNoOtherCalls();
        _exchangeRateRepositoryMock.VerifyNoOtherCalls();
        _nbpApiClientMock.VerifyNoOtherCalls();
    }
}