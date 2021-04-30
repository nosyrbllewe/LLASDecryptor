using Microsoft.Data.Sqlite;
using System;
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

        private long _totalFiles = 0;
        private long _filesCompleted = 0;

        public double CompletePercentage => _totalFiles == 0 ? 0 : (double)_filesCompleted / _totalFiles;

        public event Action<double> ProgressChanged;

        private const string REPLACE_PLAYERPREFS_KEY = "REPLACE_PLAYERPREFS_KEY";
        private static readonly string PlayerPrefsKey = @"gMzd%2FzWivy4OSa7epeBsugBA9zcP1yYkrue0zS%2FzZac%3D";
        //@"2Nfboa8IQvYEUVG9O9wZm%2FsVyw%2Fvu8Mxpiga%2B1WOf8E%3D";

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

        public async Task DecryptFiles(params string[] tables)
        {
            var databasePath = GetDatabaseFilePath();
            var dec = FindDecScript();

            string cs = $"Data Source=file:{databasePath}";
            using var con = new SqliteConnection(cs);

            con.Open();

            foreach (var table in tables)
                CalculateTotalCount(table, con);

            foreach (var table in tables)
            {
                await DecryptTable(table, con, dec);
            }
        }

        private string GetDatabaseFilePath()
        {
            var database = TryGetDatabaseFileInfo();
            if (database == null)
            {
                DecryptDatabase();
                database = TryGetDatabaseFileInfo();
            }

            if (database == null)
                throw new FileNotFoundException("Could not find database file");

            return database.FullName;
        }

        private FileInfo TryGetDatabaseFileInfo() => new DirectoryInfo(OutputFileDirectory).GetFiles("asset_a_ja.db*.sqlite").FirstOrDefault();

        private string FindDecScript()
        {
            string dec = AppDomain.CurrentDomain.BaseDirectory + "dec.txt";
            if (!File.Exists(dec))
            {
                throw new FileNotFoundException("Dec script file not found.");
            }
            return dec;
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

        private void CalculateTotalCount(string table, SqliteConnection con)
        {
            using var countTable = new SqliteCommand($"SELECT Count(*) FROM {table}", con);
            long rowCount = (long)countTable.ExecuteScalar();
            _totalFiles += rowCount;
        }

        private async Task DecryptTable(string table, SqliteConnection con, string decPath)
        {
            string packNameCMD = $"SELECT asset_path, pack_name, head, size, key1, key2 FROM {table}";
            using var cmd = new SqliteCommand(packNameCMD, con);

            using SqliteDataReader rdr = cmd.ExecuteReader();
            string outputPath = OutputFileDirectory + "\\Packages" + Path.DirectorySeparatorChar + table;

            while (rdr.Read())
            {
                await DecryptAssetFile(rdr.GetString(1), rdr.GetInt32(2), rdr.GetInt32(3), rdr.GetInt32(4), rdr.GetInt32(5), outputPath, decPath);
            }
        }

        private Task DecryptAssetFile(string pack_name, int head, int size, int key1, int key2, string outputPath, string decPath)
        {
            return Task.Run(() =>
            {
                string filePath = $"{InputFileDirectory}{Path.DirectorySeparatorChar}pkg{pack_name[0]}{Path.DirectorySeparatorChar}{pack_name}";

                var tempFile = SplitFile(filePath, outputPath, head, size, key1, key2);

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

        public static bool ExecuteApplication(string Address, string workingDir, string arguments, bool showWindow)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = Address;
            proc.StartInfo.WorkingDirectory = workingDir;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            return proc.Start();
        }
    }
}