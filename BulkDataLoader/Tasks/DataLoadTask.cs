using System.Collections.Generic;
using System.Threading.Tasks;
using BulkDataLoader.DataWriters;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class DataLoadTask : ApplicationTask
    {
        private readonly OutputType _outputType;

        public DataLoadTask(Configuration configuration, IEnumerable<string> settings)
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
            var outputDirectory = _outputType == OutputType.Sql
                ? Configuration.GetApplicationSetting("OutputFileLocation")
                : await Configuration.GetSecureLocationAsync();
                
            var handler = DataHandler.GetInstance(Configuration, _outputType, outputDirectory);

            await handler.LoadAsync();
        }
    }
}
