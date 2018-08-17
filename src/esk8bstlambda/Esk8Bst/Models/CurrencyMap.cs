using System.Collections.Generic;

namespace Esk8Bst.Models {
    /// <summary>
    /// Used to easily map between a currency type "USD" and its symbol "$"
    /// </summary>
    public class CurrencyMap {
        public static readonly Currency GBP = new Currency("GBP", '£');
        public static readonly Currency USD = new Currency("USD", '$');//"$";
        public static readonly Currency EUR = new Currency("EUR", '€');//"€";
        public static readonly Currency CAD = new Currency("CAD", '$');//"$";
        public static readonly Currency AUD = new Currency("AUD", '$');//"$";
        public static readonly Currency JPY = new Currency("JPY", '¥');//"￥";

        public static Dictionary<string, Currency> CurrencyDict = new Dictionary<string, Currency>() {
            { "USD", USD },
            { "GBP", GBP },
            { "EUR", EUR },
            { "CAD", CAD },
            { "AUD", AUD },
            { "JPY", JPY }
        };
    }


}
