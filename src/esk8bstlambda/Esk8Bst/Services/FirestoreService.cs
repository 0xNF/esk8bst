using System;
using System.Collections.Generic;
using Esk8Bst.Models;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Esk8Bst.Services {
    public class FirestoreService {

        // Constants

        private const string PROJECT = "esk8bst";
        private const string SCANDATA = "notify/ScanData";
        private const string SUBSCRIBERS = "matches";
        private const string PRECONFIRMED = "preconfirmed";

        // Fields

        private FirestoreDb DBHandle = null;
        private DocumentReference ScanDataReference;
        private readonly ILogger Logger;

        public FirestoreService(ILogger logger) {
            this.Logger = logger;
        }

        // Methods

        /// <summary>
        /// Sets up the Firestore connection
        /// </summary>
        /// <returns></returns>
        private async Task FirestoreSetup() {
            DBHandle = DBHandle ?? await FirestoreDb.CreateAsync(PROJECT);
        }

        /// <summary>
        /// Gets the last known Scan Data object
        /// </summary>
        /// <returns></returns>
        public async Task<ScanData> GetScanData() {
            await FirestoreSetup();
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
        public async Task<List<Subscriber>> GetSubscribers() {
            await FirestoreSetup();
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
        public async Task UpdateScanTime(ScanData sd) {
            await FirestoreSetup();
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
        public async Task<ScanData> CreateScanData(DateTimeOffset? LastScanDate = null, DateTimeOffset? MostRecentlySeen = null) {
            await FirestoreSetup();
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
        public async Task DeleteSubscriber(string subid) {
            await FirestoreSetup();
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
        public async Task UpsertSubscriber(PostedSubscribeObject sub) {
            await FirestoreSetup();
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
                Logger.Log($"Some error ocurred while Upserting a subscriber: {e.Message}");
                // some error ocurred while trying to create or update a subscriber
            }
        }

        /// <summary>
        /// Checks out preconfirmed email table to see if a user has already opted in.
        /// This way they wont have to opt-in twice if they resubscribe later.
        /// It also prevents us from knowing too much about our users - we can't reverse this hash and we don't store unsubscribed emails.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsPreconfirmed(string email) {
            await FirestoreSetup();
            try {
                string hashed = EncryptorService.OneWayHash(email.ToLowerInvariant());
                hashed = EncryptorService.Base64Encode(hashed);
                DocumentReference doc = DBHandle.Collection(PRECONFIRMED).Document(hashed);
                DocumentSnapshot ds = await doc.GetSnapshotAsync();
                return ds.Exists;
            } catch (Exception e) {
                Logger.Log($"Some error ocurred while checking if the supplied email was preconfirmed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Oneway hashes an email and inserts it into the preconfirmed collection
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task InsertPreconfirmed(string email) {
            await FirestoreSetup();
            try {
                string hashed = EncryptorService.OneWayHash(email.ToLowerInvariant());
                hashed = EncryptorService.Base64Encode(hashed);
                DocumentReference doc = DBHandle.Collection(PRECONFIRMED).Document(hashed);
                await DBHandle.RunTransactionAsync(async transaction => {
                    transaction.Create(doc, new Preconfirmed());
                });
            }
            catch (Exception e) {
                Logger.Log($"Some error ocurred while inserting a preconfirmed hash: {e.Message}");
            }
        }

        /// <summary>
        /// A user can request to be expunged from the system. 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task GDPRDelete(string email) {
            await FirestoreSetup();
            try {
                string hashed = EncryptorService.OneWayHash(email.ToLowerInvariant());
                hashed = EncryptorService.Base64Encode(hashed);
                DocumentReference doc = DBHandle.Collection(PRECONFIRMED).Document(hashed);
                await DBHandle.RunTransactionAsync(async transaction => {
                    transaction.Delete(doc);
                });
            }
            catch (Exception e) {
                Logger.Log($"Some error ocurred while deleteing a preconfirmed hash: {e.Message}");
            }
        }
    }

}
