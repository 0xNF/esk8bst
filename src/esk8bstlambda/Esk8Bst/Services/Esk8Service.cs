using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Esk8Bst.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Esk8Bst.Parsers;

namespace Esk8Bst.Services {

    public class Esk8Service {
        // Constants
        private const string COMMONCOMPANIESURL = "https://esk8bst.com/data/common_companies.json";
        private const string COMMONBOARDSURL = "https://esk8bst.com/data/common_boards.json";

        // Fields
        private readonly HttpClient Client;
        private readonly ILogger Logger;


        // Constructors
        public Esk8Service(ILogger logger, HttpClient client = null) {
            this.Logger = logger;
            this.Client = client ?? new HttpClient();
        }

        // Methods
        public async Task<List<Company>> GetCommonCompanies() {
            try {
                HttpResponseMessage m = await Client.GetAsync(COMMONCOMPANIESURL);
                if (!m.IsSuccessStatusCode) {
                    Logger.Log($"Failed while trying to get Common Companies from Esk8 server. Code: {m.StatusCode}, Reason: {m.ReasonPhrase}");
                    return null;
                }
                string companyjson = await m.Content.ReadAsStringAsync();
                JArray comps = JArray.Parse(companyjson);
                return CompanyParser.ParseCompanies(comps);
            }
            catch (Exception e) {
                Logger.Log($"An unknown error ocurred while deserializing the Common Companies json file:\n{e.Message}");
                return null;
            }
        }


        public async Task<List<Product>> GetCommonBoards() {
            List<Product> products = new List<Product>();
            try {
                HttpResponseMessage m = await Client.GetAsync(COMMONBOARDSURL);
                if (!m.IsSuccessStatusCode) {
                    Logger.Log($"Failed while trying to get Common Products from Esk8 server. Code: {m.StatusCode}, Reason: {m.ReasonPhrase}");
                    return null;
                }
                string productJson = await m.Content.ReadAsStringAsync();
                JArray comps = JArray.Parse(productJson);
                products = ProductParser.ParseProducts(comps);
                return products;
            }
            catch (Exception e) {
                Logger.Log($"An unknown error ocurred while deserializing the Common Companies json file:\n{e.Message}");
                return null;
            }
        }

        public List<RegexCategory<Company>> GetCompanyRegexs(List<Company> comps) {
            List<RegexCategory<Company>> rcs = new List<RegexCategory<Company>>();
            foreach (Company c in comps) {
                RegexCategory<Company> rc = new RegexCategory<Company>(new Regex($"\\s{c.CompanyName}[\\s\\,\\./\\!\\'\\\"\\`]", RegexOptions.IgnoreCase), c);
                rcs.Add(rc);
            }
            return rcs;
        }
        public List<RegexCategory<Product>> GetProductRegexs(List<Product> prods) {
            List<RegexCategory<Product>> rcs = new List<RegexCategory<Product>>();
            foreach (Product p in prods) {
                RegexCategory<Product> rc = new RegexCategory<Product>(new Regex($"\\s{p.ProductName}[\\s\\,\\./\\!\\'\\\"\\`]", RegexOptions.IgnoreCase), p);
                rcs.Add(rc);
            }
            return rcs;
        }
    }

}
