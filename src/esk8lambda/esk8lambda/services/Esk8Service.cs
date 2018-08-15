using Amazon.Lambda.Core;
using esk8lambda.models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace esk8lambda.services {
    class Esk8Service {
        // Constants
        private const string COMMONCOMPANIESURL = "https://esk8bst.com/data/common_companies.json";
        private const string COMMONBOARDSURL = "https://esk8bst.com/data/common_boards.json";

        // Fields
        private readonly HttpClient Client;
        private readonly ILambdaLogger Logger;


        // Constructors
        public Esk8Service(ILambdaLogger logger, HttpClient client = null) {
            this.Logger = logger;
            this.Client = client ?? new HttpClient();
        }

        // Methods
        public async Task<List<Company>> GetCommonCompanies() {
            List<Company> companies = new List<Company>();
            HashSet<string> gottenHashes = new HashSet<string>();
            try {
                HttpResponseMessage m = await Client.GetAsync(COMMONCOMPANIESURL);
                if (!m.IsSuccessStatusCode) {
                    Logger.Log($"Failed while trying to get Common Companies from Esk8 server. Code: {m.StatusCode}, Reason: {m.ReasonPhrase}");
                    return null;
                }
                string companyjson = await m.Content.ReadAsStringAsync();
                JArray comps = JArray.Parse(companyjson);
                foreach (JObject jobj in comps) {
                    string companyName = (string)jobj.GetValue("company");
                    string companyid = companyName.ToLowerInvariant().Replace(" ", "");
                    if (!gottenHashes.Contains(companyid)) {
                        Company c = new Company() {
                            CompanyId = companyid,
                            CompanyName = companyName,
                        };
                        companies.Add(c);
                        gottenHashes.Add(companyid);
                    }
                }
            }
            catch (Exception e) {
                Logger.Log($"An unknown error ocurred while deserializing the Common Companiwes json file:\n{e.Message}");
                return null;
            }

            return companies;
        }

        public async Task<List<Product>> GetCommonBoards() {
            List<Product> products = new List<Product>();
            HashSet<Company> companies = new HashSet<Company>();
            HashSet<string> gottenHashesProduct = new HashSet<string>();
            try {
                HttpResponseMessage m = await Client.GetAsync(COMMONBOARDSURL);
                if (!m.IsSuccessStatusCode) {
                    Logger.Log($"Failed while trying to get Common Products from Esk8 server. Code: {m.StatusCode}, Reason: {m.ReasonPhrase}");
                    return null;
                }
                string productJson = await m.Content.ReadAsStringAsync();
                JArray comps = JArray.Parse(productJson);
                int i = 0;
                foreach (JObject jobj in comps) {

                    string companyName = (string)jobj.GetValue("company");
                    string companyid = companyName.ToLowerInvariant().Replace(" ", "");
                    string productName = (string)jobj.GetValue("board");
                    string productid = productName.ToLowerInvariant().Replace(" ", "");
                    Company c = new Company() {
                        CompanyId = companyid,
                        CompanyName = companyName,
                    };
                    if (!companies.Contains(c)) {
                        companies.Add(c);
                    }

                    if (!gottenHashesProduct.Contains(productid)) {
                        Product p = new Product() {
                            ProductId = productid,
                            ProductName = productName,
                            Company = c,
                            ProductUrl = ""
                        };
                        gottenHashesProduct.Add(productid);
                        products.Add(p);
                    }
                }
            }
            catch (Exception e) {
                Logger.Log($"An unknown error ocurred while deserializing the Common Companiwes json file:\n{e.Message}");
                return null;
            }

            return products;
        }

        internal protected List<RegexCategory<Company>> GetCompanyRegexs(List<Company> comps) {
            List<RegexCategory<Company>> rcs = new List<RegexCategory<Company>>();
            foreach (Company c in comps) {
                RegexCategory<Company> rc = new RegexCategory<Company>(new Regex($"\\s{c.CompanyName}[\\s\\,\\./\\!\\'\\\"\\`]", RegexOptions.IgnoreCase), c);
                rcs.Add(rc);
            }
            return rcs;
        }
        internal protected List<RegexCategory<Product>> GetProductRegexs(List<Product> prods) {
            List<RegexCategory<Product>> rcs = new List<RegexCategory<Product>>();
            foreach (Product p in prods) {
                RegexCategory<Product> rc = new RegexCategory<Product>(new Regex($"\\s{p.ProductName}[\\s\\,\\./\\!\\'\\\"\\`]", RegexOptions.IgnoreCase), p);
                rcs.Add(rc);
            }
            return rcs;
        }
    }
}
