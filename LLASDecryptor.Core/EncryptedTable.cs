using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public record EncryptedTable : Table
    {
        private readonly string unityFileExtension = ".unity";

        private readonly string defaultFileExtension = string.Empty;

        const string ASSETBUNDLE_SIGNATURE = "UnityFS";

        private byte[] assetBundleSignatureBytes;
        public EncryptedTable(string tableName, string displayName) : base(tableName, displayName) 
        {
            assetBundleSignatureBytes = Encoding.UTF8.GetBytes(ASSETBUNDLE_SIGNATURE);
        }

        public override SqlColumn[] GetColumns()
        {
            return new SqlColumn[]
            {
                new SqlColumn("pack_name", SqliteType.Text),
                new SqlColumn("head", SqliteType.Integer),
                new SqlColumn("size", SqliteType.Integer),
                new SqlColumn("key1", SqliteType.Integer),
                new SqlColumn("key2", SqliteType.Integer),
            };
        }

        public override async Task ProcessRow(string inputPath, string outputPath, object[] data)
        {
            await Task.Run(() =>
            {
                DecryptAssetFile((string)data[0], (int)data[1], (int)data[2], (int)data[3], (int)data[4], inputPath, outputPath);
            });
        }

        private void DecryptAssetFile(string pack_name, int head, int size, int key1, int key2, string inputPath, string outputPath)
        {
            string filePath = $"{inputPath}{Path.DirectorySeparatorChar}pkg{pack_name[0]}{Path.DirectorySeparatorChar}{pack_name}";

            var tempFile = SplitFile(filePath, outputPath, head, size, key1, key2);
        }

        protected virtual string GetFileExtension(byte[] fileData)
        {
            if(IsUnityFile(fileData))
                return unityFileExtension;
            return defaultFileExtension;

        }

        private bool IsUnityFile(byte[] fileData)
        {
            return FileDataChecker.ContainsFileSignature(fileData, assetBundleSignatureBytes);
        }


        private string SplitFile(string path, string outputPath, int head, int size, int key1, int key2)
        {
            var file = File.OpenRead(path);
            file.Position = head;
            byte[] fileBytes = File.ReadAllBytes(path);
            byte[] sectionBytes = new byte[size];
            Array.Copy(fileBytes, head, sectionBytes, 0, size);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            

            LoveLiveDecryptor.DecryptFile(sectionBytes, 12345, key1, key2);

            outputPath = $"{outputPath}{Path.DirectorySeparatorChar}{Path.GetFileName(path)}_{head}{GetFileExtension(sectionBytes)}";

            File.WriteAllBytes(outputPath, sectionBytes);

            return outputPath;
        }

    }
}
