using Google.Cloud.Firestore;

namespace Esk8Bst.Models {
    [FirestoreData]
    public class Preconfirmed {

        [FirestoreProperty("exists")]
        public bool Exists { get; set; } = true;
    }


}
