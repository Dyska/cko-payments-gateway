namespace PaymentGateway.Api.Domain.Models
{
    public class Card
    {
        public string ExpiryMonth { get; }
        public string ExpiryYear { get; }
        public string CardNumber { get; }
        public string CVV { get; }

        public Card(string expiryMonth, string expiryYear, string cardNumber, string cvv)
        {
            if (!IsMonthAndYearExpiryValid(expiryMonth, expiryYear)) {
                throw new ArgumentException("The provided month and year must be valid and in the future.");
            }

            if (!IsCardNumberValid(cardNumber)) {
                throw new ArgumentException("Card number must be between 14 and 19 characters long and only contain numeric characters.");
            }

            if (!IsCVVValid(cvv)) {
                throw new ArgumentException("CVV must be between 3 and 4 characters long and only contain numeric characters.");
            }

            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            CardNumber = cardNumber;
            CVV = cvv;
        }

        private static bool IsMonthAndYearExpiryValid(string inputMonth, string inputYear)
        {
            bool isMonthValid = int.TryParse(inputMonth, out int month) && month >= 1 && month <= 12;
            bool isYearValid = int.TryParse(inputYear, out int year);

            return isMonthValid && isYearValid && IsMonthYearInFuture(month, year);
        }

        //Assumption: Card is valid until UTC midnight in expiry month + year
        private static bool IsMonthYearInFuture(int month, int year)
        {
            var utcNow = DateTime.UtcNow;

            if (year < utcNow.Year || (year == utcNow.Year && month < utcNow.Month))
            {
                return false;
            }
            return true;
        }

        private static bool IsCardNumberValid(string cardNumber)
        {
            if (cardNumber.Length < 14 || cardNumber.Length > 19)
            {
                return false;
            }

            foreach (var c in cardNumber)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsCVVValid(string cVV)
        {
            if (cVV.Length < 3 || cVV.Length > 4)
            {
                return false;
            }

            foreach (var c in cVV)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}