using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public record TextureTable : EncryptedTable
    {
        public TextureTable(string tableName, string displayName) : base(tableName, displayName)
        {
        }

        protected override string GetFileExtension(byte[] fileData)
        {
            if (fileData == null || fileData.Length <= 2)
                return string.Empty;

            uint first4Bytes = BitConverter.ToUInt32(fileData.Take(4).Reverse().ToArray(), 0);

            if (first4Bytes == 0x89504E47) //‰PNG
                return ".png";
            else if (first4Bytes == 0xFFD8FFDB) //
                return ".jpg";
            else
                return string.Empty;

        }

    }
}
