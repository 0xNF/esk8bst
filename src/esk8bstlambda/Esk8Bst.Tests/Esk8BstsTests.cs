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
