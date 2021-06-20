using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace LLASDecryptor.Core
{
    public class SqlColumn
    {
        public string ColumnName { get; set; }

        public SqliteType Type { get; }

        public SqlColumn(string name, SqliteType type)
        {
            ColumnName = name;
            Type = type;
        }
    }
}
