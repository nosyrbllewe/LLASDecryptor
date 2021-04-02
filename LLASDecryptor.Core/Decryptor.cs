using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace LLASDecryptor.Core
{
    public class Decryptor
    {
        public string InputFileDirectory { get; set; }
        public string OutputFileDirectory { get; set; }

        private const int MAX_CONCURRENT_PROGRAMS = 40;

        private int currentPrograms = 0;

        private const string REPLACE_KEY1 = "REPLACE_KEY1";
        private const string REPLACE_KEY2 = "REPLACE_KEY2";
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

        public void DecryptFiles(params string[] tables)
        {
            var databasePath = GetDatabaseFilePath();
            var dec = FindDecScript();

            string cs = $"Data Source=file:{databasePath}";
            using var con = new SqliteConnection(cs);

            con.Open();
            foreach (var table in tables)
            {
                DecryptTable(table, con, dec);
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

        private void DecryptTable(string table, SqliteConnection con, string decPath)
        {
            string packNameCMD = $"SELECT asset_path, pack_name, head, size, key1, key2 FROM {table}";
            using var cmd = new SqliteCommand(packNameCMD, con);

            using SqliteDataReader rdr = cmd.ExecuteReader();
            string outputPath = OutputFileDirectory + "\\Packages" + Path.DirectorySeparatorChar + table;

            while (rdr.Read())
            {
                DecryptAssetFile(rdr.GetString(1), rdr.GetInt32(2), rdr.GetInt32(3), rdr.GetInt32(4), rdr.GetInt32(5), outputPath, decPath);
            }
        }

        private void DecryptAssetFile(string pack_name, int head, int size, int key1, int key2, string outputPath, string decPath)
        {
            string filePath = $"{InputFileDirectory}{Path.DirectorySeparatorChar}pkg{pack_name[0]}{Path.DirectorySeparatorChar}{pack_name}";
            string strCmdText;

            var tempFile = SplitFile(filePath, head, size);

            string fileText = File.ReadAllText(decPath);
            fileText = fileText.Replace(REPLACE_KEY1, key1.ToString());
            fileText = fileText.Replace(REPLACE_KEY2, key2.ToString());
            var uniqueScriptName = AppDomain.CurrentDomain.BaseDirectory + $"{pack_name}_{head}_unity3d.txt";
            File.WriteAllText(uniqueScriptName, fileText);

            while (currentPrograms > MAX_CONCURRENT_PROGRAMS) ;

            currentPrograms++;
            strCmdText = $"-C -Y -Q -o -F . {uniqueScriptName} {tempFile} {outputPath}";
            var process = StartProcess(quickbmsPath, strCmdText);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                currentPrograms--;
                //Use Try Catch if multiple of the same file are in the list and are thus being used at the same time.
                if (Path.GetFileName(tempFile).Contains("_"))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }
                if (File.Exists(uniqueScriptName))
                {
                    try
                    {
                        File.Delete(uniqueScriptName);
                    }
                    catch { }
                }
            };
        }

        private static string SplitFile(string path, int head, int size)
        {
            var file = File.OpenRead(path);
            file.Position = head;
            byte[] fileBytes = File.ReadAllBytes(path);
            byte[] sectionBytes = new byte[size];
            Array.Copy(fileBytes, head, sectionBytes, 0, size);

            string outputPath = $"{path}_{head}";

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
            currentPrograms++;
            var process = Process.Start("CMD.exe", $"/C {cmdArgs}");

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                currentPrograms--;
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

        private Process StartProcess(string filePath, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = arguments
            };
            return Process.Start(psi);
        }
    }
}