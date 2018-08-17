using Esk8Bst.Models;
using Esk8Bst.Parsers;
using Esk8Bst.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Esk8Bst.Tests {
    public class LaunchSettingsFixture : IDisposable {
        public LaunchSettingsFixture() {
            using (var file = File.OpenText("Properties\\launchSettings.json")) {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables) {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }

        public void Dispose() {
            // ... clean up
        }
    }

    class TestLogger : ILogger {
        public void Log(string line) {
            Debug.WriteLine(line);
        }
    }

    public class Esk8ServiceTests {
        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public Esk8ServiceTests() {

        }

        [Fact]
        public void TestEncryption() {
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string email = "nickflower@gmail.com";
            string encrypted = AESThenHMAC.SimpleEncryptWithPassword(email, key);

            Assert.NotEqual(encrypted, email);
            string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, key);
            Assert.Equal(decrypted, email);

            return;
        }

        [Fact]
        public void TestOneWayHash() {
            string x = "the quick brown fox jumped over the lazy dog";
            string hash = EncryptorService.OneWayHash(x);
            Assert.Equal(" ��-��efeX(�g�\u0016���;ž@Ibt�͵�&5�", hash);
        }

        [Fact]
        public void TestPayloadDescryption() {
            string key = "Xley2zWu52Dv5lhnhlAm97GQf01p3v3Knzhwf0QbTZRYlhrrJTZJaM2iZzMD6p5";
            string encrypted = "joXgj8J6MML+2/PZ7Avj0jO7To7XD7Rfrji0vcaRI9UpO8aCF8iTRnFOiFNBjx8gvRwDzt6i72lHhOQ/nI4XE/xPYlII+cJDyb8gMtzrw9pov4gNjQBWt/AnHM2Itla0ZsRxL82qfHhj3/PvTaKeSa2hrnV21eo//y5aX7QQEuj0nGA+708JXSzzH/2qXSQu";
            string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, key);
            JObject j = JObject.Parse(decrypted);
            PostedSubscribeObject pso = PostedSubscribeObject.FromJson(j);
        }
        
        [Fact]
        public async Task TestParseCompanies() {
            string s = await File.ReadAllTextAsync("resources/common_companies.json");
            JArray jarrCompany = JArray.Parse(s);
            Esk8Service ESS = new Esk8Service(logger);
            List<Company> companies = CompanyParser.ParseCompanies(jarrCompany);
            Assert.Equal(42, companies.Count);
            return;
        }

        [Fact]
        public async Task TestParseProducts() {
            string s = await File.ReadAllTextAsync("resources/common_boards.json");
            JArray jarrProduct = JArray.Parse(s);
            Esk8Service ESS = new Esk8Service(logger);
            List<Product> Products = ProductParser.ParseProducts(jarrProduct);
            Assert.Equal(138, Products.Count);
            return;
        }

        [Fact]
        public async Task TestCombineProductCompanies() {
            Esk8Service ESS = new Esk8Service(logger);

            string rawCompanies = await File.ReadAllTextAsync("resources/common_companies.json");
            string rawProducts = await File.ReadAllTextAsync("resources/common_boards.json");

            JArray jarrCompany = JArray.Parse(rawCompanies);
            JArray jarrProduct = JArray.Parse(rawProducts);

            List<Company> companies = CompanyParser.ParseCompanies(jarrCompany);
            List<Product> products = ProductParser.ParseProducts(jarrProduct);

            List<Company> combined = CompanyParser.CombineCompanyLists(companies, products.Select(x => x.Company));
            Assert.Equal(42, combined.Count);
        }

    }
}
