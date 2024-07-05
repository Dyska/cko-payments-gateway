namespace PaymentGateway.Api.Domain.Models;

public class Payment
{
    public Guid Id { get; init; }
    public PaymentStatus Status { get; private set; }
    public Guid? AuthorizationCode { get; private set; }
    public Card Card { get; }
    public string ISOCurrencyCode { get; }
    public decimal Amount { get; }

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
        Amount = amount;

        Id = Guid.NewGuid();
        Status = PaymentStatus.Pending;
    }

    //State machine?
    public void SetSuccessful(Guid authorizationCode)
    {
        Status = PaymentStatus.Authorized;
        AuthorizationCode = authorizationCode;
    }

    public void SetDeclined()
    {
        Status = PaymentStatus.Declined;
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
}