using System;
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

        public static void GetHmacSha1(string databaseKey, byte[] shaKey)
        {
            System.Security.Cryptography.HMACSHA1 sha = new System.Security.Cryptography.HMACSHA1(shaKey);
            databaseKey = HttpUtility.UrlEncode(databaseKey);
            var bytes = Encoding.UTF8.GetBytes(databaseKey);
            databaseKey = Convert.ToBase64String(bytes);
        }
    }
}
