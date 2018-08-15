using Amazon.Lambda.Core;
using esk8lambda.models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace esk8lambda.services {
    class MailgunService {

        // CONSTANTS
        public static string DOMAIN { get; private set; }
        public static string APIKEY { get; private set; }

        // FIELDS
        private readonly HttpClient Client;
        private readonly ILambdaLogger Logger;

        public MailgunService(ILambdaLogger logger, HttpClient client = null) {
            Client = client ?? new HttpClient();
            Logger = logger;
        }

        // METHODS
        public HttpRequestMessage MakeMailgunMessage(MailgunEmail e) {
            UriBuilder ub = new UriBuilder($"https://api.mailgun.net/v3/{MailgunService.DOMAIN}/messages");
            var ht = HttpUtility.ParseQueryString(ub.Query);
            ht["from"] = e.From;
            ht["to"] = String.Join(',', e.To);
            ht["subject"] = e.Subject;
            ht["body"] = ""; //todo
            ub.Query = ht.ToString();

            HttpRequestMessage m = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = ub.Uri,
            };
            m.Headers.Authorization = new AuthenticationHeaderValue("api", APIKEY);
            return m;
        }

        public IEmail MakeEmail(Subscriber s, List<LambdaMatch> lm) {

        }

    }
}
