using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace BulkDataLoader.DataWriters
{
    public class CsvDataWriter : DataWriter
    {
        public override string FileExtension => "csv";
        public override char QuoteCharacter => '"';

        public override async Task Write(IEnumerable<DataRow> data, string fileName, FileMode fileMode)
        {
            var outputFile = GetFileInfo(fileName);
            Log.Information($"[ ] Writing CSV file {outputFile.FullName}");

            using (var stream = outputFile.Open(fileMode, FileAccess.Write))
            {
                var encoder = new UTF8Encoding(true);
                var csvData = data.Select(row => row.ToCsvRow());
                var dataString = string.Join(Environment.NewLine, csvData) + Environment.NewLine;
            
                var bytes = encoder.GetBytes(dataString);
                await stream.WriteAsync(bytes);
            }
        }
    }
}
