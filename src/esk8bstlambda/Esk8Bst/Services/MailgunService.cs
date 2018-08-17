using System;
using System.Collections.Generic;
using System.Net.Http;
using Esk8Bst.Models;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;

namespace Esk8Bst.Services {
    public class MailgunService {

        // CONSTANTS
        private static string DOMAIN { get; set; }
        private static string APIKEY { get; set; }
        public static string POSTMASTER { get; private set; }

        // FIELDS
        private readonly HttpClient Client;
        private readonly ILogger Logger;

        public MailgunService(ILogger logger, HttpClient client = null) {
            Client = client ?? new HttpClient();
            Logger = logger;
            APIKEY = Environment.GetEnvironmentVariable("MAILGUN_API_KEY");
            DOMAIN = Environment.GetEnvironmentVariable("MAILGUN_DOMAIN_KEY");
            POSTMASTER = $"Esk8Bst <mailgun@{DOMAIN}>";
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes("api" + ":" + APIKEY)));
        }

        // METHODS
        public HttpRequestMessage MakeMailgunMessage(MailgunEmail e) {
            UriBuilder ub = new UriBuilder($"https://api.mailgun.net/v3/{DOMAIN}/messages");

            HttpRequestMessage m = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = ub.Uri,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                    { "from", e.From },
                    { "to", String.Join(",", e.To) },
                    { "subject", e.Subject },
                    { "text", e.Body },
                    { "o:testmode", e.IsTest ? "true" : "false" },
                }),
            };
            return m;
        }

        public MailgunEmail MakeEmail(Subscriber s, List<LambdaMatch> lms) {
            // remember to include an Unsubscribe Link
            int itemCount = lms.Sum(x => x.Posts.Count);

            string subjectSingle = "1 new item that matched your criteria has been posted";
            string subjectPlural = $"{itemCount} new items that matched your critiera have been posted";
            MailgunEmail m = new MailgunEmail() {
                From = POSTMASTER,
                To = { s.DocumentId },
                Subject = itemCount == 1 ? subjectSingle : subjectPlural
            };
            string body = (itemCount == 1 ? subjectSingle : subjectPlural) + "\n\n";

            string BSTtoSTring(BST bst) {
                switch(bst) {
                    case BST.BST:
                        return "buy, sell, or trade";
                    case BST.BUY:
                        return "buy";
                    case BST.SELL:
                        return "sell";
                    case BST.TRADE:
                        return "trade";
                    case BST.NONE:
                        return "none";
                }
                return "";
            }
            foreach (LambdaMatch lm in lms) {

                // Assemble Subheader
                string WTStr = $"People looking to {BSTtoSTring(lm.FbMatch.BST)}";
                string fromStr = "products from " + (lm.FbMatch.Companies.Any() ? $"companies [{String.Join(", ", lm.FbMatch.Companies)}]" : "any company");

                Currency c = CurrencyMap.CurrencyDict[lm.FbMatch.Currency];
                string priceStr = lm.FbMatch.Price.HasValue ? $"for {(lm.FbMatch.BST == BST.BUY ? "more than or equal to " : "less than or equal to")} " + $"{c.symbol}{lm.FbMatch.Price.Value} {c.Code}" : "at any price";


                string matchBody = $"{WTStr} {fromStr} {priceStr}:\n";
                // Assemble Items
                foreach(BSTComment comment in lm.Posts) {
                    string seller = comment.Seller;
                    string comp = comment.Company;
                    string product = comment.Product == "?" ? "<product unknown>" : comment.Product;
                    string url = comment.Url;
                    Currency c2 = CurrencyMap.CurrencyDict[comment.Currency];

                    string pstr = comment.Price == 0 ? "<price not posted>" : $"{c2.symbol}{comment.Price} {c2.Code}";
                    string commentStr = $"{comp} {product} by {seller} for {pstr}\n{url}\n\n";
                    matchBody += commentStr;
                }

                body += matchBody;
            }


            // make unsubscribe link

            string encryptionKey = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string encryptedPayload = EncryptorService.Base64Encode(s.DocumentId); //AESThenHMAC.SimpleEncryptWithPassword(s.DocumentId.ToLower(), encryptionKey);
            string link = $"https://1lol87xzbj.execute-api.us-east-2.amazonaws.com/Prod/unsubscribe?confirmkey={encryptedPayload}";

            string unsubtext = $"Unsubscribe: {link}";

            body += $"\n\n\n\n {unsubtext}";
            m.Body = body;

            return m;
        }

        public async Task<bool> BatchSend(IEnumerable<MailgunEmail> emails) {
            foreach (MailgunEmail e in emails) {
                await Send(e);
            }
            return true;
        }

        public async Task<bool> Send(MailgunEmail email) {


            HttpRequestMessage m = MakeMailgunMessage(email);
            HttpResponseMessage r = await Client.SendAsync(m);
            if (r.IsSuccessStatusCode) {
                Logger.Log("Sent an email successfullt");
                return true;
            }
            else {
                Logger.Log($"Encountered a failure when sending email: {r.ReasonPhrase}");
                return false;
            }
        }

    }

}
