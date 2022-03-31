using System;
using System.Linq;
using System.Text;
using System.Web;

namespace LLASDecryptor.Core
{
    public static class LoveLiveDecryptor
    {
        public static void DecryptFile(byte[] data, int keys_0, int keys_1, int keys_2)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ (((keys_1 ^ keys_0 ^ keys_2) >> 24) & 0xFF));
                keys_0 = (int)(((0x343fd * keys_0) + 0x269ec3) & 0xFFFFFFFF);
                keys_1 = (int)(((0x343fd * keys_1) + 0x269ec3) & 0xFFFFFFFF);
                keys_2 = (int)(((0x343fd * keys_2) + 0x269ec3) & 0xFFFFFFFF);
            }
        }

        public static void DecryptDatabase(byte[] data, string fileName, string databaseKey)
        {
            databaseKey = HttpUtility.UrlDecode(databaseKey);
            byte[] key = Convert.FromBase64String(databaseKey);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);

            System.Security.Cryptography.HMACSHA1 sha = new System.Security.Cryptography.HMACSHA1(key);
            byte[] decryptedData = sha.ComputeHash(fileNameBytes);
            string hashString = Convert.ToHexString(decryptedData);
            string shortHash = hashString.Remove(hashString.Length - 16);

            string hexKey0 = shortHash.Substring(0, shortHash.Length - 16);
            string hexKey1 = shortHash.Substring(0, shortHash.Length - 8).Substring(8,8);
            string hexKey2 = shortHash.Substring(16, shortHash.Length - 16);

            int key0 = BitConverter.ToInt32(Convert.FromHexString(hexKey0).Reverse().ToArray());
            int key1 = BitConverter.ToInt32(Convert.FromHexString(hexKey1).Reverse().ToArray());
            int key2 = BitConverter.ToInt32(Convert.FromHexString(hexKey2).Reverse().ToArray());

            DecryptFile(data, key0, key1, key2);
        }
    }
}
