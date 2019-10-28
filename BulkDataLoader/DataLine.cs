using System.Collections.Generic;
using System.Linq;

namespace BulkDataLoader
{
    public class DataRow
    {
        public IReadOnlyList<DataColumn> Columns { get; }

        public DataRow(IReadOnlyList<DataColumn> columns)
        {
            Columns = columns;
        }

        public string ToCsvRow()
        {
            return string.Join(",", Columns.Select(col => col.Value));
        }
    }

    public class DataColumn
    {
        public Column Column { get; }
        public string Value { get; }

        public DataColumn(Column column, string value)
        {
            Column = column;
            Value = value;
        }
    }
}
