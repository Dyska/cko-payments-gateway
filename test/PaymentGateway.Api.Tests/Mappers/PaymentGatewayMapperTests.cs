using System.Text.Json;

using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Mappers;

namespace PaymentGateway.Api.UnitTests.Mappers;

public class PaymentGatewayMapperTests
{
    [Theory]
    [InlineData("2222405343248877", "8877")]
    [InlineData("9876543210123456789", "6789")]
    public void ToProcessPaymentResponse_ValidInput_PropertiesMatchInput(string cardNumber, string finalFourDigits)
    {
        //Arrange
        var validCard = new Card("03", "2025", cardNumber, "354");
        var validCurrency = "NZD";
        var validAmount = 10443m;

        var input = new Payment(validCard, validCurrency, validAmount);
        input.SetDeclined();

        //Act
        var actual = input.ToProcessPaymentResponse();

        //Assert
        Assert.Equal(input.Id, actual.Id);
        Assert.Equal(PaymentStatus.Declined.ToString(), actual.Status);
        Assert.Equal(input.ISOCurrencyCode, actual.ISOCurrencyCode);
        Assert.Equal(input.Amount, actual.Amount);
        Assert.Equal(validCard.ExpiryMonth, actual.CardDetails.ExpiryMonth);
        Assert.Equal(validCard.ExpiryYear, actual.CardDetails.ExpiryYear);
        Assert.Equal(finalFourDigits, actual.CardDetails.CardNumberFinalFourDigits);
    }

    [Fact]
    public void ToProcessPaymentResponse_SuccessfulPayment_DoesntContainSensitiveInformation()
    {
        //Arrange
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        var validCurrency = "NZD";
        var validAmount = 10443m;
        var authorizationCode = Guid.NewGuid();

        var input = new Payment(validCard, validCurrency, validAmount);
        input.SetSuccessful(authorizationCode);

        //Act
        string? actualJson = JsonSerializer.Serialize(input.ToProcessPaymentResponse());

        //Assert
        Assert.NotNull(actualJson);

        Assert.DoesNotContain("\"CardNumber\"", actualJson);
        Assert.DoesNotContain(validCard.CardNumber, actualJson);
        Assert.DoesNotContain("\"CVV\"", actualJson);
        Assert.DoesNotContain(validCard.CVV, actualJson);
        Assert.DoesNotContain("\"AuthorizationCode\"", actualJson);
        Assert.DoesNotContain(authorizationCode.ToString(), actualJson);
    }
}