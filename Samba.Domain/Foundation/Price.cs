namespace Samba.Domain.Foundation
{
    public class Price
    {
        public Price()
        {

        }

        public Price(decimal price, string currencyCode)
        {
            Amount = price;
            //CurrencyCode = currencyCode;
        }

        public string CurrencyCode { get {return "";}
            set { }
        }
        public decimal Amount { get; set; }

        public override string ToString()
        {
            return Amount.ToString("#,#0.00 " + CurrencyCode);
        }
    }
}
