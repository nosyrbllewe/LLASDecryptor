using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace LLASDecryptor.Core
{
    public sealed record AudioTable : Table
    {
        public string StartsWithNameFilter { get; } = string.Empty;

        public AudioTable(string tableName, string displayName, string startsWithFilter = null) : base(tableName, displayName)
        {
            StartsWithNameFilter = startsWithFilter;
        }

        public override SqlColumn[] GetColumns()
        {
            return new SqlColumn[]
            {
                new SqlColumn("sheet_name", SqliteType.Text),
                new SqlColumn("acb_pack_name", SqliteType.Text),
                new SqlColumn("awb_pack_name", SqliteType.Text),
            };

        }

        public override async Task ProcessRow(string inputPath, string outputPath, object[] data)
        {
            await Task.Run(() =>
            {
                CopyAudioFiles((string)data[0], (string)data[1], (string)data[2], inputPath, outputPath);
            });
        }

        private void CopyAudioFiles(string sheet_name, string acb_pack_name, string awb_pack_name, string inputPath, string outputPath)
        {
            // Only process the file if it is the right type.
            if (!string.IsNullOrEmpty(StartsWithNameFilter) && !sheet_name.StartsWith(StartsWithNameFilter))
                return;

            string dir = $"{outputPath}{Path.DirectorySeparatorChar}";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);


            if(acb_pack_name != null)
            {
                string acbFilePath = $"{inputPath}{Path.DirectorySeparatorChar}pkg{acb_pack_name[0]}{Path.DirectorySeparatorChar}{acb_pack_name}";
                string acbOutputFile = $"{outputPath}{Path.DirectorySeparatorChar}{sheet_name}.acb";

                if (!File.Exists(acbOutputFile))
                    File.Copy(acbFilePath, acbOutputFile, false);
            }

            if (awb_pack_name != null)
            {
                string awbFilePath = $"{inputPath}{Path.DirectorySeparatorChar}pkg{awb_pack_name[0]}{Path.DirectorySeparatorChar}{awb_pack_name}";
                string awbOutputFile = $"{outputPath}{Path.DirectorySeparatorChar}{sheet_name}.awb";
                if (!File.Exists(awbOutputFile))
                    File.Copy(awbFilePath, awbOutputFile, false);
            }
        }
    }
}
