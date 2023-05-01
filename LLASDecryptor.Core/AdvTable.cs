using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLASDecryptor.Core;
public sealed record AdvTable : EncryptedTable
{
    public AdvTable(string tableName, string displayName) : base(tableName, displayName)
    {
    }

    public override string GetFileName(string path, string asset_path, int headByteCount)
    {
        return asset_path;
    }
}
