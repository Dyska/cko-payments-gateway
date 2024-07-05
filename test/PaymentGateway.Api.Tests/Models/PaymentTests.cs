using System.Numerics;
using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.UnitTests.Models;

public class PaymentTests
{
    [Fact]
    public void Payment_ValidInputs_PropertiesMatchInput()
    {
        //Arrange
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        var validCurrency = "NZD";
        decimal validAmount = 1489m;

        //Act
        var actual = new Payment(validCard, validCurrency, validAmount);

        //Assert
        Assert.Equal(validCard, actual.Card);
        Assert.Equal(validCurrency, actual.ISOCurrencyCode);
        Assert.Equal(validAmount, ToDecimal(actual.Amount));
    }

    [Fact]
    public void Payment_NullCard_ThrowsArgumentException()
    {
        //Arrange
        var validCurrency = "NZD";
        decimal validAmount = 1489m;

        //Act
        //Assert
#pragma warning disable CS8625 // Nullability of reference types in return type doesn't match implicitly non-nullable value
        Assert.Throws<ArgumentException>(() => new Payment(null, validCurrency, validAmount));
#pragma warning restore CS8625 // Nullability of reference types in return type doesn't match implicitly non-nullable value
    }

    [Theory]
    [InlineData("")]
    [InlineData("EUR")]
    [InlineData("GBPP")]
    [InlineData("abc")]
    public void Payment_InvalidCurrencyCode_ThrowsArgumentException(string inputCurrencyCode)
    {
        //Arrange
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        decimal validAmount = 1489m;

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Payment(validCard, inputCurrencyCode, validAmount));
    }

    [Theory]
    [InlineData(123.1)]
    [InlineData(1.0000000001)]
    [InlineData(0)]
    [InlineData(-100)]
    public void Payment_InvalidAmount_ThrowsArgumentException(decimal inputAmount)
    {
        //Arrange
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        var validCurrency = "NZD";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Payment(validCard, validCurrency, inputAmount));
    }

    private static decimal ToDecimal(BigInteger bigIntegerValue)
    {
        return decimal.Parse(bigIntegerValue.ToString());
    }
}
