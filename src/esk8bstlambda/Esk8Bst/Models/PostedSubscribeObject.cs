using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Esk8Bst.Models {
    public class PostedSubscribeObject {
        public string Email { get; set; }
        public List<PostedMatchObject> Matches { get; set; }

        public JObject ToJson() {
            JArray matchArr = new JArray();
            foreach(PostedMatchObject pmo in Matches) {
                matchArr.Add(pmo.ToJson());
            }
            Dictionary<string, JToken> keys = new Dictionary<string, JToken>() {
                {"email", Email.ToLowerInvariant() },
                {"matches", matchArr }
            };
            return JObject.FromObject(keys);
        }

        public static PostedSubscribeObject FromJson(JObject jobj) {
            string email = "";
            List<PostedMatchObject> pmos = new List<PostedMatchObject>();
            if(jobj.TryGetValue("email", out JToken JTEmail)) {
                email = JTEmail.Value<string>();
                if(String.IsNullOrWhiteSpace(email)) {
                    throw new Exception("Supplied field's value wasn't valid for fields type");
                }
            }
            else {
                throw new Exception("A required field 'email' was missing from this object");
            }


            if(jobj.TryGetValue("matches", out JToken JTMatches)) {
                if (!(JTMatches is JArray matchArr) || matchArr.Count == 0) {
                    throw new Exception("Supplied field's value wasn't valid for fields type");
                }
                foreach(JToken JTMatch in matchArr) {
                    JObject matchObj = JTMatch as JObject;
                    PostedMatchObject pmo = PostedMatchObject.FromJson(matchObj);
                    if(pmo == null) {
                        throw new Exception("Failed  deserialzing match objects");
                    }
                    pmos.Add(pmo);
                }
            } else {
                throw new Exception("A required field 'matches' was missing from this object");
            }

            PostedSubscribeObject pso = new PostedSubscribeObject() {
                Email = email,
                Matches = pmos
            };
            return pso;
        }
    }


}
