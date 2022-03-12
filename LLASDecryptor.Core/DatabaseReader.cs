using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public class DatabaseReader : IDisposable
    {
        private readonly string _databaseFolder;
        private bool disposedValue;
        private SqliteConnection connection;

        private const string FILE_PATTERN = "asset_a_*";

        public DatabaseReader(string databaseFolder)
        {
            _databaseFolder = databaseFolder;
        }

        public void Open()
        {
            var databasePath = GetDatabaseFilePath();

            string cs = $"Data Source=file:{databasePath}";

            connection = new SqliteConnection(cs);

            connection.Open();
        }

        public long GetRowsInTable(string table)
        {
            using var countTable = new SqliteCommand($"SELECT Count(*) FROM {table}", connection);
            long rowCount = (long)countTable.ExecuteScalar();
            return rowCount;
        }


        private string GetDatabaseFilePath()
        {
            TryGetDatabaseFileInfo(out FileInfo database);

            if (database == null)
                throw new FileNotFoundException("Could not find database file.");

            return database.FullName;
        }

        private bool TryGetDatabaseFileInfo(out FileInfo database)
        {
            database = new DirectoryInfo(_databaseFolder).GetFiles(FILE_PATTERN).FirstOrDefault();
            return database is not null;
        }

        public IEnumerable<List<object>> DecryptTable(string table, SqlColumn[] columns)
        {
            string sqlColumns = string.Join(", ", columns.Select(c => c.ColumnName));

            string packNameCMD = $"SELECT {sqlColumns} FROM {table}";
            using var cmd = new SqliteCommand(packNameCMD, connection);

            foreach (var col in columns)
            {
                cmd.Parameters.Add(col.ColumnName, col.Type);
            }

            using SqliteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                List<object> dynamicColumns = new List<object>();
                for(int i = 0; i < columns.Length; i++)
                {

                    object value = GetColumnValue(columns[i].Type, rdr, i);
                    dynamicColumns.Add(value);
                }
                yield return dynamicColumns;
            }
        }

        object GetColumnValue(SqliteType type, SqliteDataReader reader, int readerIndex)
        {

            if (reader.IsDBNull(readerIndex))
                return null;

            switch (type)
            {
                case SqliteType.Text:
                    return reader.GetString(readerIndex);
                case SqliteType.Integer:
                    return reader.GetInt32(readerIndex);
                case SqliteType.Real:
                    return reader.GetDouble(readerIndex);
                case SqliteType.Blob:
                    return reader.GetValue(readerIndex);
                default:
                    throw new System.NotImplementedException(type.ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                connection?.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DatabaseReader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
