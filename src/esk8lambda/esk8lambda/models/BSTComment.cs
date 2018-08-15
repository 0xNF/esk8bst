using System;

namespace esk8lambda.models {
    class BSTComment {

        public string Seller { get; set; }
        public DateTimeOffset DatePosted { get; set; }
        public int NumberOfReplies { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public BST BuySellTradeStatus { get; set; }
        public int Price { get; set; }
        public string Currency { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public string Company { get; set; }
        public string Product { get; set; }
        public string Text { get; set; }
    }

}
