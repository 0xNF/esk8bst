using Amazon.Lambda.Core;
using esk8lambda.models;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace esk8lambda.services {

    class FirestoreService {

        // Constants

        private const string PROJECT = "esk8bst";
        private const string SCANDATA = "notify/ScanData";
        private const string SUBSCRIBERS = "matches";

        // Fields

        private FirestoreDb DBHandle;
        private DocumentReference ScanDataReference;
        private readonly ILambdaLogger Logger;

        public FirestoreService(ILambdaLogger logger) {
            this.Logger = logger;
        }

        // Methods

        /// <summary>
        /// Sets up the Firestore connection
        /// </summary>
        /// <returns></returns>
        internal protected async Task FirestoreSetup() {
            DBHandle = await FirestoreDb.CreateAsync(PROJECT);
        }

        /// <summary>
        /// Gets the last known Scan Data object
        /// </summary>
        /// <returns></returns>
        internal protected async Task<ScanData> GetScanData() {
            ScanData sd = null;
            try {
                ScanDataReference = DBHandle.Document(SCANDATA);
                if (ScanDataReference == null) {
                    // ScanData vanished - create a new one.
                    return await CreateScanData();
                }
                DocumentSnapshot scanDataSnapshot = await ScanDataReference.GetSnapshotAsync();
                if (scanDataSnapshot.Exists) {
                    sd = scanDataSnapshot.ConvertTo<ScanData>();
                }
                else {
                    Logger.Log("No Scan Data was found, creating a new one and will pick up next invocation");
                    return await CreateScanData();
                }
            }
            catch (Exception e) {
                Logger.Log("An unknown error ocurred while fetching Scan Data. Aborting.");
                // Some error occurred.
            }
            return sd;
        }

        /// <summary>
        /// Returns a list of Subscribers that are looking for matching posts
        /// </summary>
        /// <returns></returns>
        internal protected async Task<List<Subscriber>> GetSubscribers() {

            List<Subscriber> subscribers = new List<Subscriber>();
            try {
                CollectionReference matchCollection = DBHandle.Collection(SUBSCRIBERS);
                QuerySnapshot matchesSnapshot = await matchCollection.GetSnapshotAsync();
                foreach (DocumentSnapshot docss in matchesSnapshot.Documents) {
                    Subscriber match = docss.ConvertTo<Subscriber>();
                    match.DocumentId = docss.Id;
                    subscribers.Add(match);
                }
            }
            catch (Exception e) {
                Logger.Log($"An unknown error ocurred while fetching the subscribers:\n{e.Message} ");
            }
            return subscribers;
        }

        /// <summary>
        /// Updates the ScanData section of our database with new values
        /// </summary>
        /// <param name="sd"></param>
        /// <returns></returns>
        internal protected async Task UpdateScanTime(ScanData sd) {
            try {
                await DBHandle.RunTransactionAsync(async transaction => {
                    transaction.Set(ScanDataReference, sd);
                    await Task.Delay(0);
                });
            }
            catch (Exception e) {
                Logger.Log("Catastrophic failure @ update scan timer");
            }
        }

        /// <summary>
        /// If the ScanData object vanishes, we recreate it with this method.
        /// </summary>
        /// <param name="LastScanDate"></param>
        /// <param name="MostRecentlySeen"></param>
        /// <returns></returns>
        internal protected async Task<ScanData> CreateScanData(DateTimeOffset? LastScanDate = null, DateTimeOffset? MostRecentlySeen = null) {
            ScanData sd = new ScanData() {
                LastScanDate = LastScanDate ?? DateTimeOffset.Now,
                MostRecentlySeen = MostRecentlySeen ?? DateTimeOffset.Now
            };
            try {
                if (ScanDataReference == null) {
                    ScanDataReference = DBHandle.Document(SCANDATA);
                }
                await DBHandle.RunTransactionAsync(async transaction => {
                    transaction.Create(ScanDataReference, sd);
                    await Task.Delay(0);
                });
            }
            catch (Exception e) {
                // Some failure happened while trying to recreate our scan object
            }
            return sd;
        }

        /// <summary>
        /// Unsubscribed a user by their Id (which is their email addr)
        /// </summary>
        /// <param name="subid"></param>
        /// <returns></returns>
        internal protected async Task DeleteSubscriber(string subid) {
            try {
                await DBHandle.RunTransactionAsync(async transaction => {
                    DocumentReference doc = DBHandle.Collection(SUBSCRIBERS).Document(subid);
                    if (doc != null) {
                        transaction.Delete(doc);
                    }
                });
            }
            catch (Exception e) {
                // Some failure ocurred while deleting a subscriber;
            }
        }

        /// <summary>
        /// Creates or Inserts (upserts) a Subscriber based on their email
        /// </summary>
        /// <param name="sub"></param>
        /// <returns></returns>
        internal protected async Task UpsertSubscriber(PostedSubscribeObject sub) {
            Subscriber s = Subscriber.FromPostedSubscriber(sub);
            try {
                DocumentReference doc = DBHandle.Collection(SUBSCRIBERS).Document(s.DocumentId);
                if (doc != null) {
                    DocumentSnapshot ds = await doc.GetSnapshotAsync();
                    if (ds.Exists) {
                        // UPDATE
                        await DBHandle.RunTransactionAsync(async transaction => {
                            transaction.Update(doc, "matches", s.Matches);
                        });
                        return;
                    }
                }
                // CREATE
                await DBHandle.RunTransactionAsync(async transaction => {
                    transaction.Create(doc, s);
                });
                return;

            }
            catch (Exception e) {
                // some error ocurred while trying to create or update a subscriber
            }
        }
    }
}
