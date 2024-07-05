using System.Numerics;

namespace PaymentGateway.Api.Domain.Models;

public class Payment
{
    public Guid Id { get; init; }
    public PaymentStatus Status { get; private set; }
    public Card Card { get; }
    public string ISOCurrencyCode { get; }
    public BigInteger Amount { get; }

    public Payment(Card card, string currencyCode, decimal amount)
    {
        if (card == null)
        {
            throw new ArgumentException("Must provide valid Card details.");
        }
        if (!IsISOCurrencyCodeValid(currencyCode))
        {
            throw new ArgumentException("Invalid currency code provided. Supported currencies: 'USD', 'NZD', 'GBP'.");
        }
        if (!IsAmountValid(amount))
        {
            throw new ArgumentException("Amount must be an integer.");
        }

        Card = card;
        ISOCurrencyCode = currencyCode;
        Amount = ConvertDecimalToBigInteger(amount);

        Id = Guid.NewGuid();
        Status = PaymentStatus.Pending;
    }

    public void UpdateStatus(PaymentStatus status)
    {
        //TODO: state machine - which state transitions are valid?

        Status = status;
    }

    private static bool IsISOCurrencyCodeValid(string currencyCode)
    {
        if (currencyCode.Length != 3)
        {
            return false;
        }

        switch (currencyCode)
        {
            case "USD":
            case "NZD":
            case "GBP":
                return true;
            default:
                return false;
        }
    }

    //Assumption: Negative and 0 amounts are not allowed
    private static bool IsAmountValid(decimal amount)
    {
        bool isInteger = decimal.Truncate(amount) == amount;
        return isInteger && amount > 0;
    }

    private static BigInteger ConvertDecimalToBigInteger(decimal decimalValue)
    {
        string decimalString = decimalValue.ToString();
        return BigInteger.Parse(decimalString);
    }
}