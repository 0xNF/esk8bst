using System.Collections.Generic;

namespace esk8lambda.models {
    class PostedMatchObject {
        public List<string> Companies { get; set; }
        public string Currency { get; set; } = CurrencyMap.USD.Code;
        public int? Price { get; set; }
        public string BST { get; set; } = "SELL";
    }

}
