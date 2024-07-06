using PaymentGateway.Api.Domain.Clients.Bank;
using PaymentGateway.Api.Domain.Clients.Bank.Models;
using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.UnitTests.Mappers;

public class BankClientMapperTests
{
    [Theory]
    [InlineData("03", "2025", "03/2025")]
    [InlineData("11", "2029", "11/2029")]
    [InlineData("1", "2027", "1/2027")]
    public void ToBankPaymentRequest_ValidInput_PropertiesMatchInput(string inputMonth, string inputYear, string expectedExpiryDate)
    {
        //Arrange
        var validCard = new Card(inputMonth, inputYear, "9876543210123456789", "354");
        var validCurrency = "NZD";
        var validAmount = 10443m;

        var input = new Payment(validCard, validCurrency, validAmount);

        //Act
        PaymentRequest actual = input.ToBankPaymentRequest();

        //Assert
        Assert.Equal(input.Card.CardNumber, actual.CardNumber);
        Assert.Equal(expectedExpiryDate, actual.ExpiryDate);
        Assert.Equal(input.ISOCurrencyCode, actual.Currency);
        Assert.Equal(input.Amount, actual.Amount);
        Assert.Equal(input.Card.CVV, actual.CVV);
    }
}