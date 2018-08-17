using Esk8Bst.Models;
using Esk8Bst.Parsers;
using Esk8Bst.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Esk8Bst.Tests {

    public class RedditServiceTests {

        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public RedditServiceTests() {

        }


        [Fact]
        public async Task TestParseFrontPage() {
            string s = await File.ReadAllTextAsync("resources/frontpage.json");
            JObject frontpagejson = JObject.Parse(s);
            RedditService RSS = new RedditService(logger);
            string bsturl = RSS.FindBSTThreadUrl(frontpagejson);
            Assert.Equal("https://old.reddit.com/r/ElectricSkateboarding/comments/93t8gd/buyselltrade_monthly_sticky_august/.json", bsturl);
        }

        [Fact]
        public async Task TestParseBSTThread() {
            string s = await File.ReadAllTextAsync("resources/bstthread.json");
            JArray BSTThreadJson = JArray.Parse(s);
            RedditService RSS = new RedditService(logger);
            
            // Get Companies, Products, Regexes, etc
            string rawCompanies = await File.ReadAllTextAsync("resources/common_companies.json");
            string rawProducts = await File.ReadAllTextAsync("resources/common_boards.json");
            JArray jarrCompany = JArray.Parse(rawCompanies);
            JArray jarrProduct = JArray.Parse(rawProducts);
            List<Company> companies = CompanyParser.ParseCompanies(jarrCompany);
            List<Product> products = ProductParser.ParseProducts(jarrProduct);
            List<Company> combined = CompanyParser.CombineCompanyLists(companies, products.Select(x => x.Company));
            Esk8Service ESS = new Esk8Service(logger);
            List<RegexCategory<Company>> CompRs = ESS.GetCompanyRegexs(companies);
            List<RegexCategory<Product>> ProdRs = ESS.GetProductRegexs(products);

            List<BSTComment> comments = RSS.ParseComments(BSTThreadJson, CompRs, ProdRs);

            // Assert that there are 29 items in this json
            Assert.Equal(29, comments.Count);

            // Assert that there are X items for WTB
            Assert.Equal(5, comments.Count(x => x.BuySellTradeStatus == BST.BUY));

            // Assert that there are Y items for WTS
            Assert.Equal(24, comments.Count(x => x.BuySellTradeStatus == BST.SELL));

            // Assert that there are 0 items for Trade
            Assert.Equal(0, comments.Count(x => x.BuySellTradeStatus == BST.TRADE));
        }


        [Fact]
        public async Task TestParseBSTThreadWithUntilDate() {
            string s = await File.ReadAllTextAsync("resources/bstthread.json");
            JArray BSTThreadJson = JArray.Parse(s);
            RedditService RSS = new RedditService(logger);

            // Get Companies, Products, Regexes, etc
            string rawCompanies = await File.ReadAllTextAsync("resources/common_companies.json");
            string rawProducts = await File.ReadAllTextAsync("resources/common_boards.json");
            JArray jarrCompany = JArray.Parse(rawCompanies);
            JArray jarrProduct = JArray.Parse(rawProducts);
            List<Company> companies = CompanyParser.ParseCompanies(jarrCompany);
            List<Product> products = ProductParser.ParseProducts(jarrProduct);
            List<Company> combined = CompanyParser.CombineCompanyLists(companies, products.Select(x => x.Company));
            Esk8Service ESS = new Esk8Service(logger);
            List<RegexCategory<Company>> CompRs = ESS.GetCompanyRegexs(companies);
            List<RegexCategory<Product>> ProdRs = ESS.GetProductRegexs(products);


            DateTimeOffset Aug42018 = new DateTimeOffset(2018, 8, 4, 0, 0, 0, TimeSpan.FromSeconds(0));
            List<BSTComment> comments = RSS.ParseComments(BSTThreadJson, CompRs, ProdRs, Aug42018);

            // Assert that there are 29 items in this json
            Assert.Equal(18, comments.Count);

            // Assert that there are X items for WTB
            Assert.Equal(1, comments.Count(x => x.BuySellTradeStatus == BST.BUY));

            // Assert that there are Y items for WTS
            Assert.Equal(17, comments.Count(x => x.BuySellTradeStatus == BST.SELL));

            // Assert that there are 0 items for Trade
            Assert.Equal(0, comments.Count(x => x.BuySellTradeStatus == BST.TRADE));
        }

        [Fact]
        public async Task TestPullFrontPage() {

        }
        [Fact]
        public async Task TestPullBSTThread() {

        }

        [Fact]
        public async Task TestMatchBSTs() {

        }

        [Fact]
        public async Task TestCurrencyParsing() {

        }

        [Fact]
        public async Task TestBuySellTradeParsing() {

        }

        [Fact]
        public async Task TestCompanyParsing () {

        }

        [Fact]
        public async Task TestProductParsing() {

        }

        [Fact]
        public async Task TestPriceParsing() {

        }
    }
}
