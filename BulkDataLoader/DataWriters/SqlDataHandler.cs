using Dapper;
using MySql.Data.MySqlClient;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataLoader.DataWriters
{
    public class SqlDataHandler : DataHandler
    {
        public override string FileExtension => "sql";

        public override char QuoteCharacter => '\'';

        public override async Task WriteAsync(IEnumerable<DataRow> data, FileMode fileMode)
        {
            var outputFile = GetFileInfo(true);
            Log.Information($"[ ] Writing CSV file {outputFile.FullName}");

            using var stream = outputFile.Open(fileMode, FileAccess.Write);
            var rows = data.ToArray();
            if (!rows.Any())
            {
                return;
            }

            var encoder = new UTF8Encoding(true);
            var sql = new StringBuilder();

            var columnNames = string.Join(", ", rows.First().Columns.Select(col => $"`{col.Column.Name}`"));
            sql.AppendLine($"INSERT INTO {Configuration.TableName} ({columnNames}) ");

            sql.AppendLine("VALUES ");
            foreach (var row in rows)
            {
                var insertSql = string.Join(", ", row.Columns.Select(col => col.Value));
                sql.AppendLine($"  ({insertSql}),");
            }

            sql.Remove(sql.Length - 3, 3);
            sql.Append(";");

            var bytes = encoder.GetBytes(sql.ToString());
            await stream.WriteAsync(bytes);
        }

        public override async Task LoadAsync()
        {
            using var connection = new MySqlConnection(Configuration.DefaultConnectionString);

            var file = GetFileInfo();
            var sql = await File.ReadAllTextAsync(file.FullName, Encoding.UTF8);

            Log.Information($"[ ] Loading data from SQL file {file.FullName}");

            await connection.ExecuteAsync(sql);
        }
    }
}
