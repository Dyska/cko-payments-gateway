using Moq;

using PaymentGateway.Api.Domain.Clients.Bank;
using PaymentGateway.Api.Domain.Clients.Bank.Models;
using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.UnitTests.Helpers;

namespace PaymentGateway.Api.UnitTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IBankClient> _mockBankClient;
    private readonly PaymentService _subject;

    public PaymentServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockBankClient = new Mock<IBankClient>();
        _subject = new PaymentService(_mockPaymentRepository.Object, _mockBankClient.Object);
    }

    [Fact]
    public async void FetchPayment_WhenPaymentExists_ReturnsPayment()
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        var paymentId = validPayment.Id;
        _mockPaymentRepository.Setup(pay => pay.GetPayment(It.Is<Guid>(g => g == paymentId))).ReturnsAsync(validPayment);

        //Act
        var actual = await _subject.FetchPayment(paymentId);

        //Assert
        Assert.Equal(validPayment, actual);
        _mockPaymentRepository.Verify(pay => pay.GetPayment(It.IsAny<Guid>()), Times.Once());
    }

    [Fact]
    public async void FetchPayment_WhenPaymentNotFound_ReturnsNull()
    {
        //Arrange
        var paymentId = Guid.NewGuid();
        Payment? nullPayment = null;

        _mockPaymentRepository.Setup(pay => pay.GetPayment(It.IsAny<Guid>())).ReturnsAsync(nullPayment);

        //Act
        var actual = await _subject.FetchPayment(paymentId);

        //Assert
        Assert.Null(actual);
        _mockPaymentRepository.Verify(pay => pay.GetPayment(It.IsAny<Guid>()), Times.Once());
    }

    [Fact]
    public async void ProcessPayment_SavesPaymentToRepository()
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        _mockBankClient.Setup(
            b => b.SubmitPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse
            {
                Authorized = true,
                AuthorizationCode = Guid.NewGuid(),
            });

        //Act
        await _subject.ProcessPayment(validPayment);

        //Assert
        _mockPaymentRepository.Verify(pay => pay.SavePayment(It.Is<Payment>(p => p.Equals(validPayment))), Times.Once());
    }

    [Fact]
    public async void ProcessPayment_SubmitsPaymentToBankInCorrectFormat()
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        _mockBankClient.Setup(
            b => b.SubmitPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse
            {
                Authorized = true,
                AuthorizationCode = Guid.NewGuid(),
            });

        //Act
        await _subject.ProcessPayment(validPayment);

        //Assert
        _mockBankClient.Verify(
            b => b.SubmitPayment(It.Is<PaymentRequest>(
                p => p.CardNumber.Equals(validPayment.Card.CardNumber) &&
                    p.ExpiryDate.Equals($"{validPayment.Card.ExpiryMonth}/{validPayment.Card.ExpiryYear}") &&
                    p.Currency.Equals(validPayment.ISOCurrencyCode) &&
                    p.Amount.Equals(validPayment.Amount) &&
                    p.CVV.Equals(validPayment.Card.CVV)
                )),
                Times.Once());
    }

    [Fact]
    public async void ProcessPayment_WhenBankAuthorizesPayment_PaymentStatusSetToAuthorized()
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        var authorizationCode = Guid.NewGuid();
        _mockBankClient.Setup(
            b => b.SubmitPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse
            {
                Authorized = true,
                AuthorizationCode = authorizationCode,
            });

        //Act
        var actual = await _subject.ProcessPayment(validPayment);

        //Assert
        Assert.Equal(PaymentStatus.Authorized, actual.Status);
        Assert.Equal(authorizationCode, actual.AuthorizationCode);

        //Other properties unchanged
        Assert.Equal(validPayment.Amount, actual.Amount);
        Assert.Equal(validPayment.Card, actual.Card);
        Assert.Equal(validPayment.Id, actual.Id);
        Assert.Equal(validPayment.ISOCurrencyCode, actual.ISOCurrencyCode);
    }

    [Fact]
    public async void ProcessPayment_WhenBankDeclinesPayment_PaymentStatusSetToDeclined()
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        _mockBankClient.Setup(
            b => b.SubmitPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse
            {
                Authorized = false,
            });

        //Act
        var actual = await _subject.ProcessPayment(validPayment);

        //Assert
        Assert.Equal(PaymentStatus.Declined, actual.Status);
        Assert.Null(actual.AuthorizationCode);

        //Other properties unchanged
        Assert.Equal(validPayment.Amount, actual.Amount);
        Assert.Equal(validPayment.Card, actual.Card);
        Assert.Equal(validPayment.Id, actual.Id);
        Assert.Equal(validPayment.ISOCurrencyCode, actual.ISOCurrencyCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void ProcessPayment_RegardlessOfBankResponse_UpdatesPaymentInRepository(bool shouldBankAuthorizeRequest)
    {
        //Arrange
        var validPayment = TestPaymentFactory.GenerateValidPayment();
        var paymentResponse = shouldBankAuthorizeRequest ?
            new PaymentResponse
            {
                Authorized = true,
                AuthorizationCode = Guid.NewGuid(),
            } :
            new PaymentResponse
            {
                Authorized = false,
            };

        _mockBankClient.Setup(
            b => b.SubmitPayment(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(paymentResponse);

        //Act
        await _subject.ProcessPayment(validPayment);

        //Assert
        _mockPaymentRepository.Verify(pay => pay.UpdatePayment(It.IsAny<Payment>()), Times.Once());
    }
}