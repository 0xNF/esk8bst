using System.Collections.Generic;

namespace esk8lambda.models {
    /// <summary>
    /// Used to easily map between a currency type "USD" and its symbol "$"
    /// </summary>
    class CurrencyMap {
        public static readonly Currency GBP = new Currency("GBP", '£');
        public static readonly Currency USD = new Currency("GBP", '$');//"$";
        public static readonly Currency EUR = new Currency("GBP", '€');//"€";
        public static readonly Currency CAD = new Currency("GBP", '$');//"$";
        public static readonly Currency AUD = new Currency("GBP", '$');//"$";
        public static readonly Currency JPY = new Currency("GBP", '¥');//"￥";

        public static Dictionary<string, Currency> CurrencyDict = new Dictionary<string, Currency>() {
            { "USD", USD },
            { "GBP", GBP },
            { "EUR", EUR },
            { "CAD", CAD },
            { "AUD", AUD },
            { "JPY", JPY }
        };
    }

    public class Currency {
        public readonly string Code;
        public readonly char symbol;

        public Currency(string code, char symbol) {
            this.Code = code;
            this.symbol = symbol;
        }
    }

}
