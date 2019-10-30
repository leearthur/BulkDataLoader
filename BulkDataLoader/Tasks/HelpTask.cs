using System.Threading.Tasks;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class HelpTask : ApplicationTask
    {
        public HelpTask()
            : base(null, null)
        {
        }

        public override bool DisplayExecutionTime { get; } = false;

        public override Task Execute()
        {
            const string helpText = "\n" +
                "Usage dotnet BulkDataLoader.dll [-{task-name}]\n" +
                "Tasks:\n" +
                "  -Generate {configureation-name} {record-count} [-Append] [-Sql]\n" +
                "  -Load {configuration-name}\n" +
                "  -Create {table-name} [-Overwrite]\n" +
                "  -Settings\n";

            Log.Information(helpText);

            return Task.CompletedTask;
        }
    }
}
