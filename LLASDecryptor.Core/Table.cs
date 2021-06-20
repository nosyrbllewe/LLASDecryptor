using System.Threading.Tasks;

namespace LLASDecryptor.Core
{
    public abstract record Table
    {
        public string TableName { get; init; }
        public string DisplayName { get; init; }

        public Table(string tableName, string displayName)
        {
            TableName = tableName;
            DisplayName = displayName;
        }

        public abstract SqlColumn[] GetColumns();

        public abstract Task ProcessRow(string inputPath, string outputPath, object[] data);
    }
}
