using System.Numerics;

namespace PaymentGateway.Api.Domain.Models
{
    public class Payment
    {
        public Card Card { get; }
        public string ISOCurrencyCode { get; }
        public BigInteger Amount { get; }

        public Payment(Card card, string currencyCode, decimal amount)
        {
            if (!IsISOCurrencyCodeValid(currencyCode)) {
                throw new ArgumentException("Invalid currency code provided. Supported currencies: 'EUR', 'NZD', 'GBP'.");
            }
            
            if (!IsAmountValid(amount)) {
                throw new ArgumentException("Amount must be an integer.");
            }

            Card = card;
            ISOCurrencyCode = currencyCode;
            Amount = ConvertDecimalToBigInteger(amount);
        }

        private static bool IsISOCurrencyCodeValid(string currencyCode)
        {
            if (currencyCode.Length != 3)
            {
                return false;
            }

            switch (currencyCode) {
                case "EUR":
                case "NZD":
                case "GBP":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsAmountValid(decimal amount)
        {
            return decimal.Truncate(amount) == amount;
        }

        private static BigInteger ConvertDecimalToBigInteger(decimal decimalValue) {
            string decimalString = decimalValue.ToString();
            return BigInteger.Parse(decimalString);
        }
    }
}