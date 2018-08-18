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

    public class MailgunTests {
        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public MailgunTests() {

        }

        [Fact]
        public async Task TestSendEmail() {

            MailgunService MSS = new MailgunService(logger);
            MailgunEmail m = new MailgunEmail() {
                To = new List<string>() { "test@test.com" },
                From = "ESk8BST <mailgun@mg.esk8bst.com>",
                Subject = "This is a Test Email",
                Body = "Testing from Test",
                IsTest = true
            };
            bool success = await MSS.Send(m);
            Assert.True(success);
        }

        [Fact]
        public async Task TestMakeEmailsFromMatches() { 
            MailgunService MSS = new MailgunService(logger);

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

            MatchService LSS = new MatchService(logger);
            Dictionary<Subscriber, List<LambdaMatch>> matches = LSS.MakeMatches(subs, comments);

            List<MailgunEmail> emails = matches.Select(x => MSS.MakeEmail(x.Key, x.Value)).ToList();


        }
    }
}
