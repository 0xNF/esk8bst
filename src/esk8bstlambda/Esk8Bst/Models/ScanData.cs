using Google.Cloud.Firestore;
using System;

namespace Esk8Bst.Models {
    [FirestoreData]
    public class ScanData {
        [FirestoreProperty]
        public DateTimeOffset LastScanDate { get; set; }

        [FirestoreProperty]
        public DateTimeOffset MostRecentlySeen { get; set; }
    }


}
