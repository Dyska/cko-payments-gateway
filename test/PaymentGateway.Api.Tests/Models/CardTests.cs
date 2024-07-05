using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.UnitTests.Models;

public class CardTests
{
    [Fact]
    public void Card_ValidInputs_PropertiesMatchInput() {
        //Arrange
        var validMonth = "03";
        var validYear = "2025";
        var validCardNumber = "2222405343248877";
        var validCVV = "354";

        //Act
        var actual = new Card(validMonth, validYear, validCardNumber, validCVV);

        //Assert
        Assert.Equal(validMonth, actual.ExpiryMonth);
        Assert.Equal(validYear, actual.ExpiryYear);
        Assert.Equal(validCardNumber, actual.CardNumber);
        Assert.Equal(validCVV, actual.CVV);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("15")]
    [InlineData("0")]
    public void Card_InvalidMonth_ThrowsArgumentException(string inputMonth) {
        //Arrange
        var validYear = "2025";
        var validCardNumber = "2222405343248877";
        var validCVV = "354";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Card(inputMonth, validYear, validCardNumber, validCVV));
    }

    [Theory]
    [InlineData("202x")]
    [InlineData("")]
    [InlineData("2025.1")]
    public void Card_InvalidYear_ThrowsArgumentException(string inputYear) {
        //Arrange
        var validMonth = "10";
        var validCardNumber = "2222405343248877";
        var validCVV = "354";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Card(validMonth, inputYear, validCardNumber, validCVV));
    }

    [Theory]
    [InlineData("01","2024")]
    [InlineData("11","2023")]
    [InlineData("11","1999")]
    public void Card_MonthYearNotInFuture_ThrowsArgumentException(string inputMonth, string inputYear) {
        //Arrange
        var validCardNumber = "2222405343248877";
        var validCVV = "354";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Card(inputMonth, inputYear, validCardNumber, validCVV));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1990365748819")]
    [InlineData("60571211936811103882")]
    [InlineData("N4I9zK2rKotU7qyF")]
    public void Card_InvalidCardNumber_ThrowsArgumentException(string inputCardNumber) {
        //Arrange
        var validMonth = "03";
        var validYear = "2025";
        var validCVV = "354";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Card(validMonth, validYear, inputCardNumber, validCVV));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("0")]
    [InlineData("123a")]
    [InlineData("a123")]
    public void Card_InvalidCVV_ThrowsArgumentException(string inputCVV) {
        //Arrange
        var validMonth = "03";
        var validYear = "2025";
        var validCardNumber = "2222405343248877";

        //Act
        //Assert
        Assert.Throws<ArgumentException>(() => new Card(validMonth, validYear, validCardNumber, inputCVV));
    }
}
