using Esk8Bst.Models;
using Esk8Bst.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Esk8Bst.Tests {
    public class EncryptorServiceTests {
        readonly LaunchSettingsFixture lf = new LaunchSettingsFixture();
        readonly ILogger logger = new TestLogger();

        public EncryptorServiceTests() {

        }

        private static PostedSubscribeObject pso = new PostedSubscribeObject() {
            Email = "test@test.com",
            Matches = new List<PostedMatchObject>() {
                    new PostedMatchObject() {
                        BST = "BST",
                        Companies = new List<string>(),
                    }
                }
        };


        [Fact]
        public void TestOneWayHash() {
            string x = "the quick brown fox jumped over the lazy dog";
            string hash = EncryptorService.OneWayHash(x);
            Assert.Equal(" ��-��efeX(�g�\u0016���;ž@Ibt�͵�&5�", hash);
        }

        [Fact]
        public void TestPayloadDecryption() {
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string encrypted = "joXgj8J6MML+2/PZ7Avj0jO7To7XD7Rfrji0vcaRI9UpO8aCF8iTRnFOiFNBjx8gvRwDzt6i72lHhOQ/nI4XE/xPYlII+cJDyb8gMtzrw9pov4gNjQBWt/AnHM2Itla0ZsRxL82qfHhj3/PvTaKeSa2hrnV21eo//y5aX7QQEuj0nGA+708JXSzzH/2qXSQu";
            string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, key);
            JObject j = JObject.Parse(decrypted);
            PostedSubscribeObject pso = PostedSubscribeObject.FromJson(j);
        }


        [Fact]
        public void TestEncryption() {
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string email = "test@gtest.com";
            string encrypted = AESThenHMAC.SimpleEncryptWithPassword(email, key);

            Assert.NotEqual(encrypted, email);
            string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, key);
            Assert.Equal(decrypted, email);

            return;
        }


        [Fact]
        public async Task TestDecryptConfirmKey() {
            string b64 = "cwA2AGkAegBzAGgANQBYAE4ASgBQADYAWABhAEYAMQAxAEUAZQA4AEcAZQBqAGYAOABrAEYARwBzAEoAeQBLADAAUwB1AGkAegBUAEQAVABsAGwAVQBjAEUAVwBxAHcANwBoAEYARABlADcAUQBYAG4AWgAvADAANgBwAHAAdgBQAGUAVQB3AG8AaQBlADEAdABiAHQAZgBjAE8AcwBwAFIAQgBsAEoAbwBlAHYATQBtAGsAegBGADUAbgBFAEwARgBIAFYAUwBwAE4AMgBSAG4AQgBYAHMAYQBjAC8AYgAxADgANwBoAGcAbABtAE0ASwBLAGwAawBFAGQASgAyAGkAcgBsAGsAZwA1AHAAUQBkAC8AMAB2AFgAdgBhAG8AYQBvAHcAawBVAGwARQB1ADUAWgA3AEQARgBjADgAWgBoAG4AWgBuAGsAQwBSAHUAbgB5ADAAOABVAEkANgBmACsAbgAyAFMAYQBIAEoAZwB5ADUARABLAHIASgBBAEoARgBoAGoAYgBFADYAbgB6AEYAbwAvADcAeQArAC8AdABwADIAeQBMAHEASwBUAGwAMgBIAHAARgByAEEASgBYAFgAQQBLADAAMQBlAGsAawBPAGoAZAAvAFUANABaAEcASwBsAHoAegAxAEwANABaAHMAcAB4ADcAUQBaAHQAVwBNAG0AWABjAGcAYQBhADMAMgBHAGkALwBSAFkARQBjADQATwBIAFgAcQBnAD0APQA(";
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string decrypted = EncryptorService.DecryptConfirmKey(b64, key);
            JObject jobj = JObject.Parse(decrypted);
            PostedSubscribeObject decryptedPso = PostedSubscribeObject.FromJson(jobj);
                        
            Assert.Equal(decryptedPso.Email, pso.Email);
            PostedMatchObject postedMatch = decryptedPso.Matches.FirstOrDefault();
            PostedMatchObject staticMatch = pso.Matches.FirstOrDefault();
            Assert.Equal(postedMatch.BST, staticMatch.BST);
            Assert.Equal(postedMatch.Currency, staticMatch.Currency);
            Assert.Equal(postedMatch.Price, staticMatch.Price);
            Assert.Equal(postedMatch.Companies.Count, staticMatch.Companies.Count);
        }

        [Fact]
        public async Task TestCreateConfirmKey() {
            string encryptme = pso.ToJson().ToString();
            string key = Environment.GetEnvironmentVariable("ESK8BST_ENCRYPTION_KEY");
            string confirmKey = EncryptorService.CreateConfirmKey(encryptme, key);
            Assert.Equal("cwA2AGkAegBzAGgANQBYAE4ASgBQADYAWABhAEYAMQAxAEUAZQA4AEcAZQBqAGYAOABrAEYARwBzAEoAeQBLADAAUwB1AGkAegBUAEQAVABsAGwAVQBjAEUAVwBxAHcANwBoAEYARABlADcAUQBYAG4AWgAvADAANgBwAHAAdgBQAGUAVQB3AG8AaQBlADEAdABiAHQAZgBjAE8AcwBwAFIAQgBsAEoAbwBlAHYATQBtAGsAegBGADUAbgBFAEwARgBIAFYAUwBwAE4AMgBSAG4AQgBYAHMAYQBjAC8AYgAxADgANwBoAGcAbABtAE0ASwBLAGwAawBFAGQASgAyAGkAcgBsAGsAZwA1AHAAUQBkAC8AMAB2AFgAdgBhAG8AYQBvAHcAawBVAGwARQB1ADUAWgA3AEQARgBjADgAWgBoAG4AWgBuAGsAQwBSAHUAbgB5ADAAOABVAEkANgBmACsAbgAyAFMAYQBIAEoAZwB5ADUARABLAHIASgBBAEoARgBoAGoAYgBFADYAbgB6AEYAbwAvADcAeQArAC8AdABwADIAeQBMAHEASwBUAGwAMgBIAHAARgByAEEASgBYAFgAQQBLADAAMQBlAGsAawBPAGoAZAAvAFUANABaAEcASwBsAHoAegAxAEwANABaAHMAcAB4ADcAUQBaAHQAVwBNAG0AWABjAGcAYQBhADMAMgBHAGkALwBSAFkARQBjADQATwBIAFgAcQBnAD0APQA(", confirmKey);
        }

    }
}
