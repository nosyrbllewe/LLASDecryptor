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

        private const string REPLACE_PLAYERPREFS_KEY = "REPLACE_PLAYERPREFS_KEY";
        private static readonly string PlayerPrefsKey = @"xoapWsrPChRhiatkdrfjPrDd0hKv0H0k%2Fg%2BEkCPHcdg%3D";
            //@"3qDUnqxXY2DKX9mpkKwCv%2FlnWFp%2BLMK%2B2n5RsaOHj3c%3D";

        private static readonly string quickbmsPath = @"C:\Users\bryso\Modding\quickbms\quickbms_4gb_files.exe";

        public Decryptor(string inputFileLocation, string outputFileLocation)
        {
            InputFileDirectory = inputFileLocation;
            OutputFileDirectory = outputFileLocation;
        }

        public void DecryptDatabase()
        {
            var hmac = FindHmacScript();
            DecryptDatabaseFile(hmac);
        }

        public async Task DecryptFiles(params Table[] tables)
        {
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

        private string FindHmacScript()
        {
            string hmac = AppDomain.CurrentDomain.BaseDirectory + "hmac.txt";
            if (!File.Exists(hmac))
            {
                throw new FileNotFoundException("Hmac script file not found.");
            }
            return hmac;
        }

        private Task DecryptAssetFile(string pack_name, int head, int size, int key1, int key2, string outputPath)
        {
            
            return Task.Run(() =>
            {
                string filePath = $"{InputFileDirectory}{Path.DirectorySeparatorChar}pkg{pack_name[0]}{Path.DirectorySeparatorChar}{pack_name}";

                try
                {
                    var tempFile = SplitFile(filePath, outputPath, head, size, key1, key2);
                }
                catch (FileNotFoundException e)
                {
                    ConsoleLog?.Invoke($"ERROR: {e.Message}");

                }

                _filesCompleted++;
                ProgressChanged?.Invoke(CompletePercentage);
            });
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

        private void DecryptDatabaseFile(string hmacPath)
        {
            string fileText = File.ReadAllText(hmacPath);
            fileText = fileText.Replace(REPLACE_PLAYERPREFS_KEY, PlayerPrefsKey);
            var uniqueScriptName = AppDomain.CurrentDomain.BaseDirectory + $"hmac_temp.txt";
            File.WriteAllText(uniqueScriptName, fileText);

            string args = $"{quickbmsPath} -C -F \"*.db\" {uniqueScriptName} {InputFileDirectory} {OutputFileDirectory}";

            RunQuickBMS(args, uniqueScriptName);
        }

        private void RunQuickBMS(string cmdArgs, string tempFileToDelete)
        {
            var process = Process.Start("CMD.exe", $"/C {cmdArgs}");

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                if (File.Exists(tempFileToDelete))
                {
                    try
                    {
                        File.Delete(tempFileToDelete);
                    }
                    catch { }
                }
            };
        }
    }
}