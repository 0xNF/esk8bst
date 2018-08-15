using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using FireSharp;
using esk8lambda.services;
using esk8lambda.models;
using System.Web;
using System.Net.Http.Headers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace esk8lambda {

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
        public Match FbMatch { get; }
        public List<BSTComment> Posts { get; } = new List<BSTComment>();

        public LambdaMatch(Match m) {
            this.FbMatch = m;
        }
        public string GetMatchString() {
            string s = $"There were {Posts.Count} posts that matched your criteria of:\n{FbMatch.GetMatchString()}";
            return s;
        }
    }


    public class Function
    {
        private ILambdaContext Context { get; set; }
        public HttpClient Client { get; } = new HttpClient();
        private Esk8Service ESS;
        private RedditService RSS;
        private FirestoreService FSS;
        private MailgunService MSS;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(string input, ILambdaContext context) {
            try {
                this.Context = context;

                /* Set up our Services */
                FSS = new FirestoreService(context.Logger);
                ESS = new Esk8Service(context.Logger, Client);
                RSS = new RedditService(context.Logger, ESS, Client);
                MSS = new MailgunService(context.Logger, Client);

                /* Get our Firebase last-scanned time */
                ScanData sd = await FSS.GetScanData();
                if(sd == null) {
                    return; /* Something happened, so we quit. */
                }

                /* Check Firebase Subscribers */
                List<Subscriber> subscribers = await FSS.GetSubscribers();
                if(subscribers == null || subscribers.Count == 0) {
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
                if(String.IsNullOrWhiteSpace(BSTUrl)) {
                    /* Some kind of error ocurred, do not update */
                    return;
                }
                if (!(await RSS.GetRedditItem() is JArray BSTPage)) {
                    /* Some kind of error ocurred, do not update */
                    return;
                }

                /* Check if there are new posts, to save on Network requests fetching Company / Board information */
                if(!RSS.AnyNewPosts(BSTPage, sd.MostRecentlySeen)) {
                    /* There have been no new posts since last time. */
                    ScanData updatedScanData = new ScanData() { LastScanDate = DateTimeOffset.Now, MostRecentlySeen = DateTimeOffset.Now };
                    await FSS.UpdateScanTime(updatedScanData);
                    return;
                }


                /* Fetch Company and Product information from Esk8 servers */
                List<Product> prods = await ESS.GetCommonBoards();
                List<Company> comps = await ESS.GetCommonCompanies();
                HashSet<string> compids = comps.Select(x => x.CompanyId).ToHashSet();
                foreach (Product p in prods) {
                    if (!compids.Contains(p.Company.CompanyId)) {
                        compids.Add(p.Company.CompanyId);
                        comps.Add(p.Company);
                    }
                }
                List<RegexCategory<Company>> CompRs = ESS.GetCompanyRegexs(comps);
                List<RegexCategory<Product>> ProdRs = ESS.GetProductRegexs(prods);

                /* Parse the full thread for new posts */
                List<BSTComment> comments = RSS.ParseComments(BSTPage, CompRs, ProdRs, sd.LastScanDate);


                /* Line up potential Match Objects */
                /* At this point we don't update until emails have been sent out. */
                Dictionary<Subscriber, List<LambdaMatch>> lambdadelta = new Dictionary<Subscriber, List<LambdaMatch>>();
                foreach (Subscriber s in subscribers) {
                    List<LambdaMatch> lambdamatches = new List<LambdaMatch>();
                    foreach (Match m in s.Matches) {
                        LambdaMatch lm = new LambdaMatch(m);
                        foreach (BSTComment comment in comments) {
                            // BST type matched
                            if (comment.BuySellTradeStatus == m.BST) {
                                // Company type matches
                                if (m.Companies.Contains(comment.Company) || m.Companies.Count == 0) {
                                    if (m.BST == BST.BUY && comment.Price >= m.Price) {
                                        // MATCH FOUND
                                        lm.Posts.Add(comment);
                                    }
                                    else if (m.BST == BST.SELL && comment.Price <= m.Price) {
                                        // MATCH FOUND
                                        lm.Posts.Add(comment);
                                    }
                                    else if (m.BST == BST.TRADE) {
                                        // MATCH FOUND
                                        lm.Posts.Add(comment);
                                    }
                                }
                            }
                            if (lm.Posts.Any()) {
                                lambdamatches.Add(lm);
                            }
                        }
                    }
                }

                /* Assemble the emails to send */
                IEnumerable<MailgunEmail> emails = lambdadelta.Select(x => MSS.MakeEmail(x.Key, x.Value));

                /* Send emails */
                bool SentSuccessfully = await MSS.BatchSend(emails);
                if (SentSuccessfully) {
                    bool updateSuccess = await UpdateFirebaseLastScanTime(LastSeenThisThread);
                }
            } catch (Exception e) {
                LambdaLogger.Log($"Catastrophic failure while performing the Reddit Lambda Get: {e.Message}");
            }
        }

        private async Task<bool> SendEmails(List<MailgunEmail> emails) {
            if(emails.Count == 0) {
                return true;
            }
            return false;
        }

        private List<MailgunEmail> MakeEmails(IEnumerable<UserMatch> matches) {
            return new List<MailgunEmail>();
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


    }

}
