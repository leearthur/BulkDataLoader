using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class SettingsTask : ApplicationTask
    {
        public SettingsTask(Configuration configuration, IEnumerable<string> settings)
            : base(configuration, settings)
        {
        }

        public override bool DisplayExecutionTime { get; } = false;

        public override async Task ExecuteAsync()
        {
            var secureLocation = await Configuration.GetSecureLocationAsync();
            Log.Information($"MySQL Secure Location: {secureLocation}");
            Log.Information($"Output Location: { Configuration.GetApplicationSetting("OutputFileLocation")}");
        }
    }

    public class MySqlVariable
    {
        public string Value { get; set; }
    }
}
