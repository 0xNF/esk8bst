using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using System.Net.Http;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace esk8lambda {

    class Email {

    }

    class CurrencyMap {
        public const string GBP = "£";
        public const string USD = "$";
        public const string EUR = "€";
        public const string CAD = "$";
        public const string AUD = "$";
        public const string JPY = "￥";

        public static Dictionary<string, string> CurrencyDict = new Dictionary<string, string>() {
            { "USD", USD },
            { "GBP", GBP },
            { "EUR", EUR },
            { "CAD", CAD },
            { "AUD", AUD },
            { "JPY", JPY }
        };
    }

    class RedditThread {

    }

    /// <summary>
    /// The Match object as pulled from Firebase
    /// </summary>
    class FirebaseMatch {
        public string Email { get; }
        public List<string> Companies { get; }
        public int Price { get; }
        public string Currency { get; } = CurrencyMap.USD;
        
        public string GetMatchString() {
            string companies = $"From Companies: {(this.Companies.Contains("any") ? "any" : String.Join(',', this.Companies))}";
            string price = $"With a maximum price of: {CurrencyMap.CurrencyDict[Currency]}{Price}";
            return  $"{companies}\n{price}";
        }
    }

    /// <summary>
    /// The full set of objects that a single email will be made from
    /// Contains each match rule with their matches.
    /// </summary>
    class UserMatch {
        readonly string Email;
        readonly List<LambdaMatch> Matches = new List<LambdaMatch>();
    }

    /// <summary>
    /// A match object for this Lambda
    /// Contains a Matching criteria, plus a list of Posts that matched it
    /// </summary>
    class LambdaMatch {
        public FirebaseMatch FbMatch { get; }
        public List<string> PostUrls { get; }

        public string GetMatchString() {
            string s = $"There were {PostUrls.Count} posts that matched your criteria of:\n{FbMatch.GetMatchString()}";
            return s;
        }
    }

    class BSTComment {
        public DateTimeOffset DatePosted { get; }
        public string Seller { get; }
        public string Company { get; }
        public string Product { get; }
        public int Price { get; }
        public string Currency { get; }
    }


    public class Function
    {
        private const string FrontPageUrl = "https://old.reddit.com/r/electricskateboarding/.json";

        private ILambdaContext Context { get; set; }

        public HttpClient Client { get; } = new HttpClient();
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(string input, ILambdaContext context) {
            try {
                this.Context = context;

                // Get our Firebase last-scanned time
                DateTimeOffset? LastSeenItemTimestamp = await GetFirebaseLastScannedTime();
                if (LastSeenItemTimestamp == null) {
                    // Usually we update our last scanned time here, but we failed so ignore it.
                    return;
                }

                // Check Firebase Match requests
                List<FirebaseMatch> fbMatches = await GetFirebaseMatches();
                if (fbMatches == null || fbMatches.Count == 0) {
                    // update the last scan time so that next successful scan doesn't show every result to the user.
                    bool updateSuccess = await UpdateFirebaseLastScanTime(DateTimeOffset.Now);
                    return;
                }

                // Find our front page thread
                JObject frontPageJson = await GetFrontPage();
                if (frontPageJson == null) {
                    // Couldn't find the front page, won't update the Last Scanned timer.
                    return;
                }
                // Find our thread's url
                string bstUrl = GetBSTUrl(frontPageJson);
                if (String.IsNullOrWhiteSpace(bstUrl)) {
                    // Couldn't retrieve the thread JSON, won't update the Last Scanned timer.
                    return;
                }

                // Get thread data
                JObject BSTJson = await GetBSTJson(bstUrl);
                if (BSTJson == null) {
                    // Couldn't retrieve Thread Json, won't update the Last Scanned Timer.
                    return;
                }

                // parse the thread and get items
                List<BSTComment> NewComments = ParseThreadIntoComments(BSTJson, LastSeenItemTimestamp.Value);
                DateTimeOffset LastSeenThisThread = NewComments.OrderByDescending(x => x.DatePosted).First().DatePosted;
                if (NewComments.Count == 0) {
                    // Scanned successfully, but no new comments were found, updating last scanned timer.
                    bool updateSuccess = await UpdateFirebaseLastScanTime(LastSeenThisThread);
                    return;
                }

                // Map into a dictionary of each User : [list of items that matched]
                Dictionary<string, UserMatch> LBMatches = GetLambdaMatches(NewComments, fbMatches);

                // Compile Emails for people: at this point the key doesn't matter, it is in the email field.
                List<Email> emails = MakeEmails(LBMatches.Values);

                // Send emails
                bool SentSuccessfully = await SendEmails(emails);

                // Update Last Scanned Time after it is confirmed that the emails have been sent.
                if (SentSuccessfully) {
                    bool updateSuccess = await UpdateFirebaseLastScanTime(LastSeenThisThread);
                }
            } catch (Exception e) {
                LambdaLogger.Log($"Catastrophic failure while performing the Reddit Lambda Get: {e.Message}");
            }
        }

        private async Task<bool> SendEmails(List<Email> emails) {
            if(emails.Count == 0) {
                return true;
            }
            return false;
        }

        private List<Email> MakeEmails(IEnumerable<UserMatch> matches) {
            return new List<Email>();
        }

        /// <summary>
        /// Given a list of comments and a list of Matches,
        /// Go through the matches and determine if they of then match a given FirebaseMatch Criteria
        /// If so, add that comment to a KVP of [user who requested match, [list of matches] ]
        /// </summary>
        /// <param name="newComments"></param>
        /// <param name="fbMatches"></param>
        /// <returns></returns>
        private Dictionary<string, UserMatch> GetLambdaMatches(List<BSTComment> newComments, List<FirebaseMatch> fbMatches) {
            return new Dictionary<string, UserMatch>(); // email : usermatch
        }

        private async Task<bool> UpdateFirebaseLastScanTime(DateTimeOffset timestamp) {
            return false;
        }

        private List<BSTComment> ParseThreadIntoComments(JObject jobj, DateTimeOffset lastseen) {
            List<BSTComment> comments = new List<BSTComment>();

            return comments;
        }

        /// <summary>
        /// Queries Firebase for the Matches collection, which will return either an error or a list of zero.
        /// </summary>
        /// <returns></returns>
        private async Task<List<FirebaseMatch>> GetFirebaseMatches() {
            return null;
        }

        private async Task<DateTimeOffset?> GetFirebaseLastScannedTime() {
            return null;
        }

        private string GetBSTUrl(JObject jobj) {
            return null;
        }


        private async Task<JObject> GetFrontPage() {
            try {
                HttpResponseMessage m = await Client.GetAsync(FrontPageUrl);
                if(!m.IsSuccessStatusCode) {
                    LambdaLogger.Log($"Failed to retrieve Reddit front page json. \n\n: Status Code {m.StatusCode}, Error Reason: {m.ReasonPhrase}");
                    return null;
                }
                string content = await m.Content.ReadAsStringAsync();
                JObject jobj = JObject.Parse(content);
                return jobj;
            } catch (Exception e) {
                LambdaLogger.Log($"Failed to retrieve Reddit front page json: {e.Message}");
                return null;
            }
        }


        private async Task<JObject> GetBSTJson(string bstUrl) {
            return null;
        }




    }

}
