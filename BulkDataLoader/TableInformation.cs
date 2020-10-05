using System;
using System.Linq;

namespace BulkDataLoader
{
    public class TableInformation
    {
        public string SchemaName { get; }
        public string TableName { get; }
        public bool HasSchemaName => !string.IsNullOrWhiteSpace(SchemaName);

        public TableInformation(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Invalid table name specified", nameof(tableName));
            }

            var nameParts = tableName.Split('.').ToArray();
            var tableNamePos = nameParts.Length > 1 ? 1 : 0;

            TableName = nameParts[tableNamePos];
            if (tableNamePos > 0)
            {
                SchemaName = nameParts[0];
            }
        }
    }
}
