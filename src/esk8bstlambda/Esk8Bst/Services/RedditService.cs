using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Esk8Bst.Models;
using System.Threading.Tasks;

namespace Esk8Bst.Services {

    public class RedditService {

        // Constants
        private const string FrontPageUrl = "https://old.reddit.com/r/electricskateboarding/.json";
        private const string CanonicalName = "BUY/SELL/TRADE Monthly Sticky";
        private static readonly char[] bstSplits = new char[] { '\\', '/', ' ', '\t', '\n', 'r' };
        private static readonly string[] tokens = new string[] { "buy", "sell", "trade" };

        // Fields
        private readonly HttpClient Client;
        private readonly ILogger Logger;
        private readonly Esk8Service Esk8Service;

        // Constructors
        public RedditService(ILogger logger, Esk8Service esk8 = null, HttpClient client = null) {
            Client = client ?? new HttpClient();
            Logger = logger;
            Esk8Service = esk8 ?? new Esk8Service(logger, Client);
        }


        // Methods

        /// <summary>
        /// Given a fetched BST thread, we check if there have been any new posts since the provided scan date.
        /// </summary>
        /// <param name="posts"></param>
        /// <returns></returns>
        public bool AnyNewPosts(JArray thread, DateTimeOffset lastSeen) {
            JArray posts = thread[1]["data"]["children"] as JArray;
            foreach (JObject jpost in posts) {
                JObject commentObj = jpost.GetValue("data") as JObject;
                DateTimeOffset createdAt = DateTimeOffset.FromUnixTimeSeconds((long)commentObj["created_utc"]);
                if (createdAt > lastSeen) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a reddit object, either the front page or the BST thread.
        /// </summary>
        /// <returns></returns>
        public async Task<JToken> GetRedditItem(string url = null) {
            string getThisUrl = url ?? FrontPageUrl;
            string pagetype = url == null ? "front page" : "BST Thread";

            try {
                HttpResponseMessage m = await Client.GetAsync(getThisUrl);
                if (!m.IsSuccessStatusCode) {
                    Logger.Log($"Failed to retrieve {pagetype}. Status Code: {m.StatusCode}, Message: {m.ReasonPhrase}");
                    return null;
                }
                string content = await m.Content.ReadAsStringAsync();
                JToken thread = JToken.Parse(content);
                return thread;
            }
            catch (Newtonsoft.Json.JsonException je) {
                Logger.Log($"Failed while trying to deserialzie {pagetype} to json");
                return null;
            }
            catch (Exception e) {
                Logger.Log($"An unspecified error ocurred while getting the {pagetype}: {e.Message}");
                return null;
            }

        }

        /// <summary>
        /// Attempts to find the thread url for the current BUY SELL TRADE thread.
        /// Returns null if not found.
        /// </summary>
        /// <returns></returns>
        public string FindBSTThreadUrl(JObject page) {
            try {
                JArray pagechildren = page["data"]["children"] as JArray;
                foreach (JObject thread in pagechildren) {
                    if (thread.TryGetValue("data", out JToken commentData)) {
                        JObject commentobj = commentData as JObject;
                        if (commentobj.TryGetValue("title", out JToken titleToken) && commentobj.TryGetValue("url", out JToken urlToken)) {
                            // This is a valid thread.
                            string title = titleToken.Value<string>();
                            string url = urlToken.Value<string>();
                            if (FuzzyMatchForBST(title)) {
                                return url + ".json";
                            }
                        }
                    }
                }
                Logger.Log("Failed to BST thread url on front page, no title matched any established BST title pattern. Does one exist?");
                return null;
            }
            catch (Exception e) {
                Logger.Log($"Failed to find a BST thread from the front page json object - likely a key didn't exist and the Newtonsoft parser crashed. \n {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Attempts to leniently match a thread title to the monthly BST title pattern.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool FuzzyMatchForBST(string title) {
            title = title.ToLowerInvariant();
            if (title.Contains(CanonicalName)) {
                return true;
            }
            else {
                foreach (string token in tokens) {
                    if (!title.Contains(token)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Given a comment in the form a JObject, parse it into a more manageable BSTComment
        /// </summary>
        /// <param name="jobj"></param>
        /// <returns></returns>
        private BSTComment MakeComment(JObject jobj, List<RegexCategory<Company>> companies, List<RegexCategory<Product>> products) {
            try {
                string author = (string)jobj["author"];
                DateTimeOffset createdAt = DateTimeOffset.FromUnixTimeSeconds((long)jobj["created_utc"]);
                string permalink = "https://old.reddit.com" + (string)jobj["permalink"];
                string body = (string)jobj["body"];
                BST bstStatus = ParseBuySellTradeStatus(body);
                if (bstStatus == BST.NONE) {
                    return null;
                }
                TransactionStatus tStatus = ParseTransactionStatus(body);
                if (tStatus != TransactionStatus.OPEN) {
                    return null;
                }
                (Currency, int) CurrencyPrice = ParseCurrencyAndPrice(body);

                Company company = ParseCompany(body, companies);
                Product product = ParseProduct(body, products);
                if (product != UnknownProduct) {
                    company = product.Company;
                }

                string url = "https://reddit.com" + jobj["permalink"];

                BSTComment comment = new BSTComment() {
                    BuySellTradeStatus = bstStatus,
                    Company = company.CompanyName,
                    Product = product.ProductName,
                    Currency = CurrencyPrice.Item1.Code,
                    DatePosted = createdAt,
                    Location = "",
                    NumberOfReplies = 0,
                    Price = CurrencyPrice.Item2,
                    Seller = author,
                    TransactionStatus = tStatus,
                    Url = url,
                    Text = body
                };

                return comment;
            }
            catch (Exception) {
                // Parsing error, ignore it.
                return null;
            }

        }

        private BST ParseBuySellTradeStatus(string s) {
            BST bstType = BST.NONE;
            if (RegExs.ForBuy.Regex.IsMatch(s)) {
                bstType = BST.BUY;
            }
            else if (RegExs.ForSale.Regex.IsMatch(s)) {
                bstType = BST.SELL;
            }
            else if (RegExs.ForTrade.Regex.IsMatch(s)) {
                bstType = BST.TRADE;
            }
            return bstType;
        }

        private TransactionStatus ParseTransactionStatus(string s) {
            TransactionStatus tStatus = TransactionStatus.OPEN;
            if (RegExs.Sold.Regex.IsMatch(s)) {
                tStatus = TransactionStatus.CLOSED;
            }
            return tStatus;
        }

        private (Currency, int) ParseCurrencyAndPrice(string s) {
            Currency currencyType = CurrencyMap.USD;
            int price = 0;
            foreach (RegexCategory<Currency> rc in RegExs.CurrencyRegexs) {
                var m = rc.Regex.Match(s);
                if (m.Success) {
                    currencyType = rc.Tag;
                    if (m.Groups[1].Success) {
                        price = int.Parse(m.Groups[1].Value);
                    }
                    else if (m.Groups[2].Success) {
                        price = int.Parse(m.Groups[2].Value);
                    }
                    break;
                }
            }
            return (currencyType, price);
        }

        public static readonly Company UnknownCompany = new Company() {
            CompanyName = "?",
            CompanyId = "unknown / other"
        };
        public static readonly Company DIY = new Company() {
            CompanyId = "diy",
            CompanyName = "DIY",
        };
        public static readonly Product UnknownProduct = new Product() {
            Company = UnknownCompany,
            ProductId = "unknown / other",
            ProductName = "?"
        };
        private Company ParseCompany(string s, List<RegexCategory<Company>> comps) {
            s = s.ToLowerInvariant();
            foreach (RegexCategory<Company> rc in comps) {
                if (rc.Regex.IsMatch(s)) {
                    return rc.Tag;
                }
            }

            return UnknownCompany;
        }
        private Product ParseProduct(string s, List<RegexCategory<Product>> prods) {
            s = s.ToLowerInvariant();
            foreach (RegexCategory<Product> rc in prods) {
                if (rc.Regex.IsMatch(s)) {
                    return rc.Tag;
                }
            }
            return UnknownProduct;
        }

        /// <summary>
        /// Given a BSTThread in the form of a JObject, parse out all its comments into a list of BSTComments
        /// </summary>
        /// <param name="BstThread"></param>
        /// <returns></returns>
        public List<BSTComment> ParseComments(JArray BstThread, List<RegexCategory<Company>> companies, List<RegexCategory<Product>> products, DateTimeOffset? until = null) {
            List<BSTComment> comments = new List<BSTComment>();
            until = until ?? DateTimeOffset.MinValue;
            try {
                JArray jarr = BstThread[1]["data"]["children"] as JArray;
                foreach (JObject jobj in jarr) {
                    JObject commentObj = jobj.GetValue("data") as JObject;
                    DateTimeOffset createdAt = DateTimeOffset.FromUnixTimeSeconds((long)commentObj["created_utc"]);
                    if (createdAt <= until) {
                        // Only scan until we reach something we've seen before.
                        break;
                    }
                    BSTComment comment = MakeComment(commentObj, companies, products);
                    if (comment != null) {
                        comments.Add(comment);
                    }

                }
            }
            catch (Exception e) {
                Logger.Log($"Parser failed while looking at the comments of a BST thread, likely a key wasn't present: {e.Message}");
            }
            return comments;
        }

    }

}
