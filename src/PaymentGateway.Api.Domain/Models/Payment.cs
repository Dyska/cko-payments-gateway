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
                throw new ArgumentException("Invalid currency code provided. Supported currencies: 'USD', 'NZD', 'GBP'.");
            }
            
            if (!IsAmountValid(amount)) {
                throw new ArgumentException("Amount must be an integer.");
            }

            Card = card ?? throw new ArgumentException("Must provide valid Card details.");
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

        private static BigInteger ConvertDecimalToBigInteger(decimal decimalValue) {
            string decimalString = decimalValue.ToString();
            return BigInteger.Parse(decimalString);
        }
    }
}