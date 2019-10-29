using System.Collections.Generic;
using System.Threading.Tasks;
using BulkDataLoader.DataWriters;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class DataLoadTask : ApplicationTask
    {
        private readonly SettingsTask _settingsTask;

        public DataLoadTask(Configuration configuration, IEnumerable<string> settings)
            : base(configuration, settings)
        {
            _settingsTask = new SettingsTask();
        }

        public override async Task Execute()
        {
            Log.Information("Starting bulk data load...");

            await LoadData();

            Log.Information("Bulk data load complete");
        }

        private async Task LoadData()
        {
            var secureLocation = await _settingsTask.GetSecureLocation();
            var handler = DataHandler.GetInstance(Configuration, OutputType.Csv, secureLocation);

            await handler.Load(GetConnectionString("CallCentreDb"));
        }
    }
}
