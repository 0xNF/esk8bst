using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Esk8Bst.Models {
    static class RegExs {


        // BST Tags
        public static readonly RegexCategory<BST> ForSale = new RegexCategory<BST>(
                new Regex("[\\[\\(].*?(sell|sale|wts|lts).*?[\\)\\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                BST.SELL
            );
        public static readonly RegexCategory<BST> ForTrade = new RegexCategory<BST>(
                new Regex("[\\[\\(].*?(trade|wtt|swap).*?[\\)\\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                BST.TRADE
            );
        public static readonly RegexCategory<BST> ForBuy = new RegexCategory<BST>(
                new Regex("[\\[\\(].*?(buy|ltb|wtb|purchase).*?[\\)\\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                BST.BUY
            );
        public static readonly RegexCategory<TransactionStatus> Sold = new RegexCategory<TransactionStatus>(
                new Regex("[\\[\\(].*?(sold|no longer available).*?[\\)\\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                TransactionStatus.CLOSED
            );

        // Currency Tags
        public static List<RegexCategory<Currency>> CurrencyRegexs = new List<RegexCategory<Currency>>() {
            new RegexCategory<Currency>(
                new Regex("(?:(?:USD|\\$USD|\\$)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:USD|\\$USD|\\$)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                CurrencyMap.USD
            ),
            new RegexCategory<Currency>(
                new Regex("(?:(?:CAD|\\$CAD|CND|\\$CND|Canadian Dollars)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:CAD|\\$CAD|CND|\\$CND|Canadian Dollars)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                CurrencyMap.CAD
            ),
            new RegexCategory<Currency>(
                new Regex("(?:(?:eur|\\$eur|€)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:eur|\\$eur|€)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                CurrencyMap.EUR
            ),
            new RegexCategory<Currency>(
                new Regex("(?:(?:gbp|\\$gbp|£gbp|£)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:gbp|\\$gbp|£gbp|£)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                CurrencyMap.GBP
            ),
            new RegexCategory<Currency>(
                new Regex("(?:(?:AUD|\\$AUD|Austrailian Dollars)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:AUD|\\$AUD|Austrailian Dollars)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                CurrencyMap.AUD
            ),
            //new RegexCategory<Currency>(
            //    new Regex("(?:(?:JPY|\\$JPY||￥JPY|Yen|Japanese Yen|￥)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:JPY|\\$JPY||￥JPY|Yen|Japanese Yen|)))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            //    CurrencyMap.JPY
            //)
        };

    }


}
