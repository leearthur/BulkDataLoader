using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDataLoader.DataWriters
{
    public class CsvDataHandler : DataHandler
    {
        public override string FileExtension => "csv";
        public override char QuoteCharacter => '"';

        public override async Task WriteAsync(IEnumerable<DataRow> data, FileMode fileMode)
        {
            var outputFile = GetFileInfo(true);
            Log.Information($"[ ] Writing CSV file {outputFile.FullName}");

            using var stream = outputFile.Open(fileMode, FileAccess.Write);

            var encoder = new UTF8Encoding(true);
            var csvData = data.Select(row => row.ToCsvRow());
            var dataString = string.Join(Environment.NewLine, csvData) + Environment.NewLine;

            var bytes = encoder.GetBytes(dataString);
            await stream.WriteAsync(bytes);
        }

        public override async Task LoadAsync()
        {
            using var connection = new MySqlConnection(Configuration.DefaultConnectionString);

            var loader = new MySqlBulkLoader(connection)
            {
                FileName = $@"{GetFileInfo().FullName}",
                FieldTerminator = ",",
                FieldQuotationCharacter = '"',
                LineTerminator = Environment.NewLine,
                TableName = Configuration.TableName,
                CharacterSet = "utf8"
            };
            loader.Columns.AddRange(Configuration.Columns.Select(col => col.Name));

            Log.Information($"[ ] Bulk loading data from file {loader.FileName}");

            await connection.OpenAsync();
            await loader.LoadAsync();
            await connection.CloseAsync();
        }
    }
}
