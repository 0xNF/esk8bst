using Esk8Bst.Models;
using Esk8Bst.Parsers;
using Esk8Bst.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Esk8Bst.Tests {
    public class MatchTests {
        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public MatchTests() {

        }

        [Fact]
        public async Task TestMakeLambdaDelta() {
            string bstthreadloc = await File.ReadAllTextAsync("resources/bstthread.json");
            JArray BSTThreadJson = JArray.Parse(bstthreadloc);
            RedditService RSS = new RedditService(logger);

            // Get BST thread
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


            // Get mock subscribers
            string subloc = await File.ReadAllTextAsync("resources/Subscribers.json");
            JArray subArray = JArray.Parse(subloc);
            List<PostedSubscribeObject> psubs = SubscriberParser.ParseSubscribers(subArray);
            List<Subscriber> subs = psubs.Select(x => Subscriber.FromPostedSubscriber(x)).ToList();


            Subscriber BoostedMeepoWowGoEvolveGuy = subs.FirstOrDefault(x => x.DocumentId == "BoostedMeepo@SELL1000.com".ToLower());
            Subscriber LiterallyEverythingGuy = subs.FirstOrDefault(x => x.DocumentId == "Literally@Everything.com".ToLower());
            Subscriber WTSABackfire = subs.FirstOrDefault(x => x.DocumentId == "sellingabackfire@whoknows.com".ToLower());
            MatchService LSS = new MatchService(logger);
            Dictionary<Subscriber, List<LambdaMatch>> matches = LSS.MakeMatches(subs, comments);




            // Assert that 3 people had matches on this thread
            Assert.Equal(3, matches.Count);

            // Assert that Guy#1 had 10 matches, all of which are either Boosted, Meepo, WowGo, or Evolve below $1000 USD
            var bmwegMatches = matches[BoostedMeepoWowGoEvolveGuy];
            Assert.Equal(10, bmwegMatches[0].Posts.Count);
            Assert.True(bmwegMatches[0].Posts.All(x => new string[] { "boosted", "wowgo", "meepo", "evolve" }.Contains(x.Company.ToLowerInvariant())));
            Assert.True(bmwegMatches[0].Posts.All(x => x.Price <= bmwegMatches[0].FbMatch.Price));
            Assert.True(bmwegMatches[0].Posts.All(x => x.Currency == "USD"));
            Assert.True(bmwegMatches[0].Posts.All(x => x.BuySellTradeStatus == BST.SELL));


            // Assert that Guy#2 matched literally everything
            var leMatches = matches[LiterallyEverythingGuy];
            Assert.Equal(comments.Count, leMatches[0].Posts.Count); // Comment count and return count should always be equal - this guy just wants to see everything!

            // Assert that Guy#3 has a single match for someome who wants to buy his Backfire
            var bfMatches = matches[WTSABackfire];
            Assert.Single(bfMatches[0].Posts);
            Assert.True(bfMatches[0].Posts.All(x => x.Company.ToLower() == "backfire"));
            Assert.True(bfMatches[0].Posts.All(x => x.BuySellTradeStatus == BST.BUY));
        }

    }
}
