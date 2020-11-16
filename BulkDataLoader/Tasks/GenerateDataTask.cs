using BulkDataLoader.DataWriters;
using BulkDataLoader.Lists;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class GenerateDataTask : ApplicationTask
    {
        private readonly DataGenerator _dataGenerator;
        private readonly int _count;
        private readonly FileMode _fileMode;
        private readonly OutputType _outputType;

        public GenerateDataTask(Configuration configuration, IEnumerable<string> settings, IListCollection listCollection)
            : base(configuration, settings)
        {
            _count = int.Parse(Settings[0]);

            _fileMode = SettingExists("Append")
                ? FileMode.Append
                : FileMode.Create;

            _outputType = SettingExists("Sql")
                ? OutputType.Sql
                : OutputType.Csv;

            _dataGenerator = new DataGenerator(configuration, listCollection, _outputType);
        }

        public override async Task ExecuteAsync()
        {
            Log.Information("Starting data generation...");

            var writer = DataHandler.GetInstance(Configuration, _outputType);
            var data = await _dataGenerator.GenerateRowsAsync(_count, writer.QuoteCharacter);

            await writer.WriteAsync(data, _fileMode);

            Log.Information("Data generation complete");
        }
    }
}
