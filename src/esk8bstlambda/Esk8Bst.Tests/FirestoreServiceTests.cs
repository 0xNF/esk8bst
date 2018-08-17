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
    public class FirestoreServiceTests {
        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public FirestoreServiceTests() {

        }

        [Fact]
        public async Task TestParsePostedSubscribers() {
            FirestoreService FSS = new FirestoreService(logger);

            string s = await File.ReadAllTextAsync("resources/Subscribers.json");
            JArray subArray = JArray.Parse(s);
            List<PostedSubscribeObject> subs = SubscriberParser.ParseSubscribers(subArray);
            List<Subscriber> subtrue = subs.Select(x => Subscriber.FromPostedSubscriber(x)).ToList();

            Assert.Equal(6, subtrue.Count);
        }

        [Fact]
        public async Task TestParseScanTime() {

        }

        [Fact]
        public async Task TestInsertPreconfirmed() {
            string s = "test@test.com";
            FirestoreService FSS = new FirestoreService(logger);
            await FSS.InsertPreconfirmed(s);
        }

    }
}
