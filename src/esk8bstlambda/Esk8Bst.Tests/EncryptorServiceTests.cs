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

        private const string key = "test123^*;はハ葉";


        [Fact]
        public void TestOneWayHash() {
            string x = "the quick brown fox jumped over the lazy dog";
            string hash = EncryptorService.OneWayHash(x);
            Assert.Equal(" ��-��efeX(�g�\u0016���;ž@Ibt�͵�&5�", hash);
        }


        [Fact]
        public void TestEncryption() {
            string email = "test@gtest.com";
            string encrypted = AESThenHMAC.SimpleEncryptWithPassword(email, key);
            Assert.NotEqual(encrypted, email);
            string decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, key);
            Assert.Equal(decrypted, email);
        }


        [Fact]
        public async Task TestDecryptConfirmKey() {
            string b64 = "KwB6AEgATwBtAGIAMwBzAGwAQgA4AHQAcQBFAGsAUABPAEYAaQBnAEkARgBXAHAAawBVAFYAawBsAGIATgAzAHkAWAA2AEQAWQB6AHYAaQA5AHcATQBxAG0AbQBZAHUASwB5AEsARABLAE8AOQB4AC8AcQBqAHYAegBSAE8ALwBoAE0ARgBJAEIAdwBEADQAYwBmAFIAWABkAHAAdwB6AFUAegBNAGUATQBhADgARgBoAHoAUwBqADAAdQArAHYAaABnADUAegBMAHIATwA4AFQANABwAFMAaABVAEMAawB3ADIAdQByAG0ALwBGAEkAWgB0ADcAZQBmAEkARQAwAHEANgAyAEYAeQBZAFoASwBUAFcAOQBsADUAcgBiAGgALwAwAFIATwBxAGgARABHAHMASgBRAG0AagAyADEATgBzAGIAbQBTAGsANgBvAFMAOQBOAGoAdgBhAE4ASABJAGcAbABRAGoAUgAyAHgAawBBAGsAagA1AHQAZABOAHUAcQBZAEUAdwBaAFcARwBPAEYALwBjAGcAOQAzADkAcABsAEgAdwAzAHoAYgBhAGwAawBYAC8AcgBwAHgAcQBBAGQARQBOAGcARgBCAFAAcQBpAFoAcQA4AHUANgB0AHkAagBKADgAaQBsAEoAUwBHADcAYQBkAHkANwBuAHoAOABHAHUAWABHAEIAcgBIAFEAMQBJAHkAegB6AHcANQBLAGYAcwBsAFkAMwBRAD0APQA*";
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
            string confirmKey = EncryptorService.CreateConfirmKey(encryptme, key);
            Assert.Equal("KwB6AEgATwBtAGIAMwBzAGwAQgA4AHQAcQBFAGsAUABPAEYAaQBnAEkARgBXAHAAawBVAFYAawBsAGIATgAzAHkAWAA2AEQAWQB6AHYAaQA5AHcATQBxAG0AbQBZAHUASwB5AEsARABLAE8AOQB4AC8AcQBqAHYAegBSAE8ALwBoAE0ARgBJAEIAdwBEADQAYwBmAFIAWABkAHAAdwB6AFUAegBNAGUATQBhADgARgBoAHoAUwBqADAAdQArAHYAaABnADUAegBMAHIATwA4AFQANABwAFMAaABVAEMAawB3ADIAdQByAG0ALwBGAEkAWgB0ADcAZQBmAEkARQAwAHEANgAyAEYAeQBZAFoASwBUAFcAOQBsADUAcgBiAGgALwAwAFIATwBxAGgARABHAHMASgBRAG0AagAyADEATgBzAGIAbQBTAGsANgBvAFMAOQBOAGoAdgBhAE4ASABJAGcAbABRAGoAUgAyAHgAawBBAGsAagA1AHQAZABOAHUAcQBZAEUAdwBaAFcARwBPAEYALwBjAGcAOQAzADkAcABsAEgAdwAzAHoAYgBhAGwAawBYAC8AcgBwAHgAcQBBAGQARQBOAGcARgBCAFAAcQBpAFoAcQA4AHUANgB0AHkAagBKADgAaQBsAEoAUwBHADcAYQBkAHkANwBuAHoAOABHAHUAWABHAEIAcgBIAFEAMQBJAHkAegB6AHcANQBLAGYAcwBsAFkAMwBRAD0APQA*", confirmKey);
        }

    }
}
