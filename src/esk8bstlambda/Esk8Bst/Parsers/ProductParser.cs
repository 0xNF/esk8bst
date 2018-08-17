using Esk8Bst.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Esk8Bst.Parsers {
    public static class ProductParser {

        private static Product ParseProduct(JObject jobj) {
            string companyName = (string)jobj.GetValue("company");
            string companyid = companyName.ToLowerInvariant().Replace(" ", "");
            string productName = (string)jobj.GetValue("board");
            string productid = productName.ToLowerInvariant().Replace(" ", "");

            Company c = new Company() {
                CompanyId = companyid,
                CompanyName = companyName,
            };

            Product p = new Product() {
                ProductId = productid,
                ProductName = productName,
                Company = c,
                ProductUrl = ""
            };

            return p;

        }

        public static List<Product> ParseProducts(JArray jarr) {
            List<Product> products = new List<Product>();
            HashSet<string> gottenHashes = new HashSet<string>();
            foreach (JObject jobj in jarr) {
                Product p = ParseProduct(jobj);
                if (!gottenHashes.Contains(p.ProductId)) {
                    products.Add(p);
                }
            }
            return products;
        }
    }

}