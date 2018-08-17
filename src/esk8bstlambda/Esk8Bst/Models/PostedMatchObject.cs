using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Esk8Bst.Models {
    public class PostedMatchObject {
        public List<string> Companies { get; set; }
        public string Currency { get; set; } = CurrencyMap.USD.Code;
        public int? Price { get; set; }
        public string BST { get; set; } = "SELL";

        public JObject ToJson() {
            JArray companyArr = new JArray();
            foreach(string comp in Companies) {
                companyArr.Add(comp);
            }
            Dictionary<string, JToken> keys = new Dictionary<string, JToken>() {
                { "companies", companyArr },
                { "currency", Currency },
                { "bst", BST }
            };
            if(Price.HasValue) {
                keys.Add("price", Price.Value);
            }
            return JObject.FromObject(keys);
        }

        public static PostedMatchObject FromJson(JObject jobj) {
            string bst = "NONE";
            List<string> companies = new List<string>();

            if (jobj.TryGetValue("bst", out JToken JTBST)) {
                string bstval = JTBST.Value<string>();
                if (String.IsNullOrWhiteSpace(bstval)) {
                    throw new Exception("Field 'bst' wasn't a valid BST string");
                }
                if (bstval == "SELL" || bstval == "BUY" || bstval == "TRADE" || bstval == "BST") {
                    bst = bstval;
                }
                else {
                    throw new Exception("Field 'bst' wasn't a valid BST string");
                }

            } else {
                throw new Exception("Missing required field 'bst'");
            }

            if (jobj.TryGetValue("companies", out JToken JTComps)) {
                if (!(JTComps is JArray compArr)) {
                    throw new Exception("Field 'companies' wasn't of the expected type");
                }
                foreach(JToken jtComp in JTComps) {
                    string s = jtComp.Value<string>();
                    if(!String.IsNullOrWhiteSpace(s)) {
                        companies.Add(s);
                    }
                }
            }
            else {
                throw new Exception("Missing required field 'companies'");
            }


            string currency = null;
            if (jobj.TryGetValue("currency", out JToken JTCurrency)) {
                // doesn't matter if it doesn't exist
                string cur = JTCurrency.Value<string>();
                if(!String.IsNullOrWhiteSpace(cur)) {
                    cur = cur.ToUpperInvariant();
                    if (CurrencyMap.CurrencyDict.ContainsKey(cur)){
                        currency = CurrencyMap.CurrencyDict[cur].Code;
                    }
                }
            }

            int? price = null;
            if (jobj.TryGetValue("price", out JToken JTPrice)) {
                int p = (int)JTPrice;
                if(p >= 0) {
                    price = p;
                }
                // doesn't matter if it doesn't exist
            }

            PostedMatchObject pmo = new PostedMatchObject() {
                BST = bst,
                Companies = companies,
                Currency = currency ?? CurrencyMap.USD.Code,
                Price = price
            };
            return pmo;
        }
    }


}
