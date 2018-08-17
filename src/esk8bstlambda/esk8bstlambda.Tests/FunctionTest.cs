using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using esk8bstlambda;
using System.Net;
using System.Net.Http;
using Esk8Bst.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Esk8Bst.Services;

namespace esk8bstlambda.Tests
{
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


    public class FunctionTest
    {
        private readonly LaunchSettingsFixture lsf = new LaunchSettingsFixture();

        public FunctionTest()
        {
        }

        [Fact]
        public void TetGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();


            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Hello AWS Serverless", response.Body);
        }

        [Fact]
        public async Task TestSubscribeMethod() {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            PostedSubscribeObject pso = new PostedSubscribeObject() {
                Email = "test@gtest.com",
                Matches = new List<PostedMatchObject>() {
                    new PostedMatchObject() {
                        BST = "SELL",
                        Companies = new List<string>() {"boosted"},
                        Currency = "USD",
                        Price = 1000
                    }
                }
            };
            string s = pso.ToJson().ToString();


            request = new APIGatewayProxyRequest {
                HttpMethod = HttpMethod.Post.Method,
                Body = s
            };
            context = new TestLambdaContext();
            response = await functions.Subscribe(request, context);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("An email has been sent to the address specified confirming your subscription", response.Body);
        }

        [Fact]
        public async Task TestSubscribeFailsWithNotPost() {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            request = new APIGatewayProxyRequest {
                HttpMethod = HttpMethod.Get.Method,
            };
            context = new TestLambdaContext();
            response = await functions.Subscribe(request, context);
            Assert.Equal((int)HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task TestConfirmSubscribeMethod() {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            string confirmkey = "S7bk3IBieUbURW54nG4+1BbUxsxG3zsOeFaeCmhLoPppEYJCgSYOES2y1QrcpMKf72jeH+4nhBqUOvsRq5wE7UhEOlbNoxVe6F8v4kaFm5TcyXVQphIYcGB0BNwi5GknaCC420srJlCwC8hFX/d5akIV2RAghPayoi1dhtXkFtiKFHcolC9HXNAkjY0zIZqSeV3qi6c30hKGYMZk9bO0dpJ0DIL6Qgyi1lw3x3sMO7BGfaO+pladK0+O/OKV7j6VvBYGnVU83BukvOPYKGm7U5UWyN0tMcxNbUAK4rJb3bxjbgUP+bOYDoVOKmXc1dV6bfQHJM+tdjo+VS/0MqIxCfcjE+VpceddmujYEMox0QA=";
            request = new APIGatewayProxyRequest {
                HttpMethod = HttpMethod.Get.Method,
                QueryStringParameters = new Dictionary<string, string>() {
                    { "confirmkey", confirmkey }
                }
            };

            context = new TestLambdaContext();
            response = await functions.ConfirmSubscribe(request, context);
            Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TestUnsubscribeMethod() {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            request = new APIGatewayProxyRequest {
                HttpMethod = HttpMethod.Get.Method,
                QueryStringParameters = new Dictionary<string, string>()
            };
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string payload = EncryptorService.Base64Encode("test@test.com"); //AESThenHMAC.SimpleEncryptWithPassword("nflower@winetech.com", key);
            request.QueryStringParameters.Add("confirmkey", payload);
            context = new TestLambdaContext();
            response = await functions.Unsubscribe(request, context);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestScanMethod() {
            TestLambdaContext context;
            APIGatewayProxyRequest request;

            Functions functions = new Functions();

            request = new APIGatewayProxyRequest {
                HttpMethod = HttpMethod.Get.Method,
            };
            context = new TestLambdaContext();
            await functions.Scan(request, context);
            //Assert.EqualKK((int)HttpStatusCode.OK, response.StatusCode);
        }
    }
}
