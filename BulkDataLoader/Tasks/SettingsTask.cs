using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class SettingsTask : ApplicationTask
    {
        public SettingsTask(Configuration configuration, IEnumerable<string> settings)
            : base(configuration, settings)
        {
        }

        public override bool DisplayExecutionTime { get; } = false;

        public override async Task Execute()
        {
            var secureLocation = await Configuration.GetSecureLocation();
            Log.Information($"MySQL Secure Location: {secureLocation}");
            Log.Information($"Output Location: { Configuration.GetApplicationSetting("OutputFileLocation")}");
        }
    }

    public class MySqlVariable
    {
        public string Value { get; set; }
    }
}
