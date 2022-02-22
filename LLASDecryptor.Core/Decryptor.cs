using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public class Decryptor
    {
        public string InputFileDirectory { get; set; }
        public string OutputFileDirectory { get; set; }

        public string StatusText { get; private set; }

        private long _totalFiles = 0;
        private long _filesCompleted = 0;

        public double CompletePercentage => _totalFiles == 0 ? 0 : (double)_filesCompleted / _totalFiles;

        public event Action<double> ProgressChanged;

        public event Action<string> ConsoleLog;

        public string PlayerPrefsKey { get; }

        public Decryptor(string inputFileLocation, string outputFileLocation, string playerPrefsKey)
        {
            InputFileDirectory = inputFileLocation;
            OutputFileDirectory = outputFileLocation;
            PlayerPrefsKey = playerPrefsKey;
        }

        public async Task DecryptFiles(params Table[] tables)
        {
            DecryptDatabaseFiles();

            _filesCompleted = 0;
            _totalFiles = 0;

            using var databaseReader = new DatabaseReader(OutputFileDirectory);
            databaseReader.Open();

            foreach (var table in tables)
               _totalFiles += databaseReader.GetRowsInTable(table.TableName);

            foreach (var table in tables)
            {
                var columns = table.GetColumns();
                string outputPath = OutputFileDirectory + "\\Packages" + Path.DirectorySeparatorChar + table.TableName;
                foreach (var data in databaseReader.DecryptTable(table.TableName, columns))
                {
                    await ProcessData(data, table, outputPath);
                }
            }
        }

        async Task ProcessData(List<dynamic> data, Table table, string outputPath)
        {
            try
            {
                await table.ProcessRow(InputFileDirectory, outputPath, data.ToArray());
            }
            catch (FileNotFoundException e)
            {
                ConsoleLog?.Invoke($"ERROR: {e.Message}");
            }

            _filesCompleted++;
            ProgressChanged?.Invoke(CompletePercentage);
        }

        private static string SplitFile(string path, string outputPath, int head, int size, int key1, int key2)
        {
            var file = File.OpenRead(path);
            file.Position = head;
            byte[] fileBytes = File.ReadAllBytes(path);
            byte[] sectionBytes = new byte[size];
            Array.Copy(fileBytes, head, sectionBytes, 0, size);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            outputPath = $"{outputPath}{Path.DirectorySeparatorChar}{Path.GetFileName(path)}_{head}.unity";

            LoveLiveDecryptor.DecryptFile(sectionBytes, 12345, key1, key2);

            File.WriteAllBytes(outputPath, sectionBytes);

            return outputPath;
        }

        private void DecryptDatabaseFiles()
        {

            if(!Directory.Exists(OutputFileDirectory))
            {
                Directory.CreateDirectory(OutputFileDirectory);
            }

            FileInfo[] files = new DirectoryInfo(InputFileDirectory).GetFiles("*.db");
            foreach (FileInfo file in files)
            {
                string path = file.FullName;
                byte[] data = File.ReadAllBytes(path);
                LoveLiveDecryptor.DecryptDatabase(data, file.Name, PlayerPrefsKey);

                string newFilePath = $"{OutputFileDirectory}{Path.DirectorySeparatorChar}{file.Name}.sqlite";
                File.WriteAllBytes(newFilePath, data);
            }
        }
    }
}