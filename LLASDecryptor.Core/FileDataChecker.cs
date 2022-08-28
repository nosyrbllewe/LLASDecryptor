using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public static class FileDataChecker
    {
        public static bool ContainsFileSignature(byte[] fileData, byte[] signature)
        {
            if(fileData == null)
                throw new ArgumentNullException(nameof(fileData));

            if (fileData.Length < signature.Length)
                return false;

            for(int i = 0; i < signature.Length; i++)
            {
                if (fileData[i] != signature[i])
                    return false;
            }
            return true;
        }
    }
}
