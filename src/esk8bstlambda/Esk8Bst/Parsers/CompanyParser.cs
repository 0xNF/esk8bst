using Esk8Bst.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Esk8Bst.Parsers {
    public static class CompanyParser {

        private static Company ParseCompany(JObject jobj) {
            string companyName = (string)jobj.GetValue("company");
            string companyid = companyName.ToLowerInvariant().Replace(" ", "");
            Company c = new Company() {
                CompanyId = companyid,
                CompanyName = companyName,
            };
            return c;
        }

        public static List<Company> ParseCompanies(JArray jarr) {
            List<Company> companies = new List<Company>();
            HashSet<string> gottenHashes = new HashSet<string>();
            foreach (JObject jobj in jarr) {
                Company c = ParseCompany(jobj);
                if (!gottenHashes.Contains(c.CompanyId)) {
                    companies.Add(c);
                }
            }
            return companies;
        }

        public static List<Company> CombineCompanyLists(IEnumerable<Company> a, IEnumerable<Company> b) {
            List<Company> combined = new List<Company>();
            HashSet<string> ids = new HashSet<string>();
            foreach(Company c in a.Concat(b)) {
                if(!ids.Contains(c.CompanyId)) {
                    combined.Add(c);
                    ids.Add(c.CompanyId);
                }
            }
            return combined;
        }
    }

    public static class SubscriberParser {

        public static List<PostedSubscribeObject> ParseSubscribers(JArray jarr) {
            List<PostedSubscribeObject> subs = new List<PostedSubscribeObject>();
            foreach(JObject jobj in jarr) {
                PostedSubscribeObject pso  = PostedSubscribeObject.FromJson(jobj);
                subs.Add(pso);
            }
            return subs;
        }
    }

}