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
                new SqlColumn("asset_path", SqliteType.Text),
                new SqlColumn("pack_name", SqliteType.Text),
                new SqlColumn("head", SqliteType.Integer),
                new SqlColumn("size", SqliteType.Integer),
                new SqlColumn("key1", SqliteType.Integer),
                new SqlColumn("key2", SqliteType.Integer),
            };
        }

        public override async Task ProcessRow(string inputPath, string packageOutputPath, object[] data)
        {
            await Task.Run(() =>
            {
                DecryptAssetFile((string)data[0], (string)data[1], (int)data[2], (int)data[3], (int)data[4], (int)data[5], inputPath, packageOutputPath);
            });
        }

        private void DecryptAssetFile(string asset_path, string pack_name, int head, int size, int key1, int key2, string inputPath, string packageOutputPath)
        {
            string filePath = $"{inputPath}{Path.DirectorySeparatorChar}pkg{pack_name[0]}{Path.DirectorySeparatorChar}{pack_name}";

            SplitFileAndDecryptFile(filePath, packageOutputPath, asset_path, head, size, key1, key2);
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


        private void SplitFileAndDecryptFile(string inputFilePath, string packageOutputPath, string asset_path, int head, int size, int key1, int key2)
        {
            var file = File.OpenRead(inputFilePath);
            file.Position = head;
            byte[] fileBytes = File.ReadAllBytes(inputFilePath);
            byte[] sectionBytes = new byte[size];
            Array.Copy(fileBytes, head, sectionBytes, 0, size);

            if (!Directory.Exists(packageOutputPath))
                Directory.CreateDirectory(packageOutputPath);

            LoveLiveDecryptor.DecryptFile(sectionBytes, 12345, key1, key2);
            string fileName = GetFileName(inputFilePath, asset_path, head);
            string fileOutputPath = $"{packageOutputPath}{Path.DirectorySeparatorChar}{fileName}{GetFileExtension(sectionBytes)}";

            string finalOutputFolder = Path.GetDirectoryName(fileOutputPath);
            if (!Directory.Exists(finalOutputFolder))
                Directory.CreateDirectory(finalOutputFolder);

            File.WriteAllBytes(fileOutputPath, sectionBytes);
        }

        public virtual string GetFileName(string filePath, string asset_path, int headByteCount)
        {
            return $"{Path.GetFileName(filePath)}_{headByteCount}";
        }
    }
}
