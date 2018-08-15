using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;

namespace esk8lambda.models {
    [FirestoreData]
    class Subscriber {

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

    }

}
