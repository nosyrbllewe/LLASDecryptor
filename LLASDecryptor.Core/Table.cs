namespace LLASDecryptor.Core
{
    public record Table
    {
        public string TableName { get; init; }
        public string DisplayName { get; init; }
        public string FileExtension { get; init; } = ".unity";

        public Table(string tableName, string displayName)
        {
            TableName = tableName;
            DisplayName = displayName;
        }
    }
}
