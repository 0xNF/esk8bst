using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net.Http;
using Esk8Bst;
using Esk8Bst.Services;
using esk8bstlambda.Models;
using Esk8Bst.Models;
using Newtonsoft.Json.Linq;
using Esk8Bst.Parsers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace esk8bstlambda {
    public class Functions {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions() {
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context) {
            var logger = new Esk8LambdaLogger(context.Logger);
            logger.Log("Get Request\n");

            var response = new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Hello AWS Serverless",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            logger.Log("A get request ocurred - usually this is the developer testing that the serverless suite still works.");
            // This is just a test so that we can tell if the server is reading query params
            if(request.QueryStringParameters != null && request.QueryStringParameters.Count > 0) {
                response.Body += String.Join("\n", request.QueryStringParameters.Select(x => $"Key: {x.Key} :: Value: {x.Value}"));
            }

            return response;
        }


        /// <summary>
        /// The endpoint hit when a user submits their email to esk8bst
        /// This schedules a Mailgun Email that will include a Confirm Subscribe Link
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> Subscribe(APIGatewayProxyRequest request, ILambdaContext context) {
            var logger = new Esk8LambdaLogger(context.Logger);
            logger.Log("Subscribe endpoint reached");


            if (request.HttpMethod != HttpMethod.Post.Method) {
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed
                };
            };

            string postbody = request.Body;
            PostedSubscribeObject pso = null;
            string encrypter = "";
            try {
                JObject jobj = JObject.Parse(postbody);
                pso = PostedSubscribeObject.FromJson(jobj);

                if (pso.Email.Contains("@") && pso.Matches.Count > 0) { // we can proceed

                    FirestoreService FSS = new FirestoreService(logger);
                    if(await FSS.CheckIsPreconfirmed(pso.Email)) {
                        // Immediately subscribe the user, they've already been here.
                        await FSS.UpsertSubscriber(pso);
                        return new APIGatewayProxyResponse() {
                            StatusCode = (int)HttpStatusCode.Created,
                            Body = "Alright! You've been confirmed as interested in receiving updates from https://esk8bst.com",
                        };
                    }

                    string encryptionKey = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
                    encrypter = AESThenHMAC.SimpleEncryptWithPassword(postbody, encryptionKey);
                }

            } catch (Exception e) {
                logger.Log("Tried to parse a malformed subscriber json");
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "Failed to parse json properly",
                };
            }

            if (String.IsNullOrWhiteSpace(encrypter)) {
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "Failed to parse json properly - no email found",
                };
            }

            string b64 = EncryptorService.Base64Encode(encrypter);
            MailgunService MSS = new MailgunService(logger);
            MailgunEmail m = new MailgunEmail() {
                To = new List<string> { pso.Email },
                From = MailgunService.POSTMASTER,
                Subject = "Esk8Bst Notification Opt In Request",
                Body = "" +
                "Someone has registered you as being interested in receiving notifications about new electric skateboard postings from https://esk8bst.com.\n\n" +
                "If this was you, please click the link below to confirm your email. If this was not you, or you no longer wish to receive emails from us, then ignore this message.\n\n" +
                $"https://1lol87xzbj.execute-api.us-east-2.amazonaws.com/Prod/confirm?confirmkey={b64}",
            };

            bool success = await MSS.Send(m);
            if (!success) {
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = "Failed to send email to recipent",
                };
            }

            //An email has been sent to the address specified confirming your subscription
            var response = new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "An email has been sent to the address specified confirming your subscription",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }

        /// <summary>
        /// Adds users email to the database with their chosen match objects.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> ConfirmSubscribe(APIGatewayProxyRequest request, ILambdaContext context) {
            var logger = new Esk8LambdaLogger(context.Logger);
            logger.Log("Confirm Subscribe endpoint reached");

            if (request.QueryStringParameters.ContainsKey("confirmkey")) {
                string encryptionkey = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
                string b64payload = request.QueryStringParameters["confirmkey"];
                string encryptedpayload = EncryptorService.Base64Decode(b64payload);
                logger.Log("Received this as the confirm key: " + request.QueryStringParameters["confirmkey"]);
                string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encryptedpayload, encryptionkey);
                PostedSubscribeObject pso = null;
                try {
                    JObject jobj = JObject.Parse(decrypted);
                    pso = PostedSubscribeObject.FromJson(jobj);
                } catch (Exception e) {
                    logger.Log("Tried to parse malformed json and failed at Confirm Subscribe");
                    return new APIGatewayProxyResponse() {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Body = "Failed to parse json properly",
                    };
                }

                if (pso != null && pso.Email.Contains("@") && pso.Matches.Count > 0) {
                    FirestoreService FSS = new FirestoreService(logger);
                    await FSS.UpsertSubscriber(pso);
                    await FSS.InsertPreconfirmed(pso.Email);
                    return new APIGatewayProxyResponse() {
                        StatusCode = (int)HttpStatusCode.Created,
                        Body = "Alright! You've been confirmed as interested in receiving updates from https://esk8bst.com",
                    };
                }
            }

            return new APIGatewayProxyResponse() {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Failed to properly parse the confirm link.",
            };
        }

        /// <summary>
        /// Removes users email from the db
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> Unsubscribe(APIGatewayProxyRequest request, ILambdaContext context) {
            var logger = new Esk8LambdaLogger(context.Logger);
            logger.Log("Unsubscribe endpoint reached");

            if (request.HttpMethod != HttpMethod.Get.Method) {
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                    Body = "This endpoint only responds to GET requests"
                };
            }

            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string decr = "";
            if (request.QueryStringParameters.ContainsKey("confirmkey")) {
                string payload = request.QueryStringParameters["confirmkey"];
                if (!String.IsNullOrWhiteSpace(payload)) {
                    decr = EncryptorService.Base64Decode(payload);
                    //decr = AESThenHMAC.SimpleDecryptWithPassword(payload, key);
                    if(String.IsNullOrWhiteSpace(decr) || !decr.Contains("@")) {
                        return new APIGatewayProxyResponse() {
                            StatusCode = (int)HttpStatusCode.BadRequest,
                            Body = "An email was not found in the confirmkey parameter",
                        };
                    }
                } else {
                    return new APIGatewayProxyResponse() {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = "Missing value for parameter `confirmkey`",
                    };
                }
            } else {
                return new APIGatewayProxyResponse() {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Missing parameter `confirmkey`",
                };
            }
            FirestoreService FSS = new FirestoreService(logger);
            await FSS.DeleteSubscriber(decr.ToLowerInvariant());

            var response = new APIGatewayProxyResponse {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Ok! You've been unsubscribed and will no longer receive updates. If you change your mind, you can always sign up again at https://esk8bst.com"
            };
            return response;
        }

        /// <summary>
        /// The core function launched every hour.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Scan(APIGatewayProxyRequest request, ILambdaContext context) {
            var logger = new Esk8LambdaLogger(context.Logger);
            logger.Log("Scan initiated endpoint reached");
            HttpClient Client = new HttpClient();
            /* Set up our Services */
            FirestoreService FSS = new FirestoreService(logger);
            Esk8Service ESS = new Esk8Service(logger, Client);
            RedditService RSS = new RedditService(logger, ESS, Client);
            MailgunService MSS = new MailgunService(logger, Client);
            MatchService LSS = new MatchService(logger);

            /* Get our Firebase last-scanned time */
            ScanData sd = await FSS.GetScanData();
            if (sd == null) {
                return; /* Something happened, so we quit. */
            }

            /* Check Firebase Subscribers */
            List<Subscriber> subscribers = await FSS.GetSubscribers();
            if (subscribers == null || subscribers.Count == 0) {
                ScanData updatedScanData = new ScanData() { LastScanDate = DateTimeOffset.Now, MostRecentlySeen = DateTimeOffset.Now };
                await FSS.UpdateScanTime(updatedScanData);
                return;
            }

            /* Get the BST Thread */
            if (!(await RSS.GetRedditItem() is JObject frontPage)) {
                /* Some kind of error ocurred, do not update */
                return;
            }
            string BSTUrl = RSS.FindBSTThreadUrl(frontPage);
            if (String.IsNullOrWhiteSpace(BSTUrl)) {
                /* Some kind of error ocurred, do not update */
                return;
            }
            if (!(await RSS.GetRedditItem(BSTUrl) is JArray BSTPage)) {
                /* Some kind of error ocurred, do not update */
                return;
            }

            /* Check if there are new posts, to save on Network requests fetching Company / Board information */
            if (!RSS.AnyNewPosts(BSTPage, sd.MostRecentlySeen)) {
                /* There have been no new posts since last time. */
                ScanData updatedScanData = new ScanData() { LastScanDate = DateTimeOffset.Now, MostRecentlySeen = DateTimeOffset.Now };
                await FSS.UpdateScanTime(updatedScanData);
                return;
            }


            /* Fetch Company and Product information from Esk8 servers */
            List<Product> prods = await ESS.GetCommonBoards();
            List<Company> comps = await ESS.GetCommonCompanies();
            comps = CompanyParser.CombineCompanyLists(comps, prods.Select(x => x.Company));

            List<RegexCategory<Company>> CompRs = ESS.GetCompanyRegexs(comps);
            List<RegexCategory<Product>> ProdRs = ESS.GetProductRegexs(prods);

            /* Parse the full thread for new posts */
            List<BSTComment> comments = RSS.ParseComments(BSTPage, CompRs, ProdRs, sd.LastScanDate);


            /* Line up potential Match Objects */
            /* At this point we don't update until emails have been sent out. */
            Dictionary<Subscriber, List<LambdaMatch>> lambdadelta = LSS.MakeMatches(subscribers, comments);

            /* Assemble the emails to send */
            IEnumerable<MailgunEmail> emails = lambdadelta.Select(x => MSS.MakeEmail(x.Key, x.Value));

            /* Send emails */
            bool SentSuccessfully = await MSS.BatchSend(emails);
            if (SentSuccessfully) {
                ScanData updatedScanData = new ScanData() { LastScanDate = DateTimeOffset.Now, MostRecentlySeen = DateTimeOffset.Now };
                await FSS.UpdateScanTime(updatedScanData);
            }
        } 
    }
}
