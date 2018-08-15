using Google.Cloud.Firestore;
using System;

namespace esk8lambda.models {
    [FirestoreData]
    class ScanData {
        [FirestoreProperty]
        public DateTimeOffset LastScanDate { get; set; }

        [FirestoreProperty]
        public DateTimeOffset MostRecentlySeen { get; set; }
    }


}
