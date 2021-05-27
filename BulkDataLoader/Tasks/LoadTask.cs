using BulkDataLoader.DataWriters;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class LoadTask : ApplicationTask
    {
        private readonly OutputType _outputType;

        public LoadTask(Configuration configuration, IEnumerable<string> settings)
            : base(configuration, settings)
        {
            _outputType = SettingExists("Sql")
                ? OutputType.Sql
                : OutputType.Csv;
        }

        public override async Task ExecuteAsync()
        {
            Log.Information("Starting bulk data load...");

            await LoadDataAsync();

            Log.Information("Bulk data load complete");
        }

        private async Task LoadDataAsync()
        {
            var handler = DataHandler.GetInstance(Configuration, _outputType);

            await handler.LoadAsync();
        }
    }
}
