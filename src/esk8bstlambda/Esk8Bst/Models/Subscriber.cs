using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Esk8Bst.Models {
    [FirestoreData]
    public class Subscriber {

        [FirestoreProperty("matches")]
        public List<Match> Matches { get; set; }

        public string DocumentId { get; set; }

        public static Subscriber FromPostedSubscriber(PostedSubscribeObject sub) {
            Subscriber s = new Subscriber() {
                DocumentId = sub.Email.ToLowerInvariant(),
                Matches = sub.Matches.Select(x => Match.FromPostedMatch(x)).ToList(),
            };
            return s;
        }

        public JObject ToJson() {
            JArray matchArr = new JArray();
            foreach(Match m in Matches) {
                matchArr.Add(m.ToJson());
            }
            Dictionary<string, JToken> keys = new Dictionary<string, JToken>() {
                {"email", DocumentId.ToLowerInvariant() },
                {"matches", matchArr}
            };
            return JObject.FromObject(keys);
        }

    }


}
