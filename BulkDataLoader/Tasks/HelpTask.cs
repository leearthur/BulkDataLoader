using System;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class HelpTask : ApplicationTask
    {
        public HelpTask()
            : base(null, null)
        {
        }

        public override bool DisplayExecutionTime { get; } = false;

        public override Task ExecuteAsync()
        {
            const string helpText =
                "Usage: BulkDataLoader.exe [-{task-name}]\n" +
                "Tasks:\n" +
                "  -generate {configureation-name} {record-count} [-Append] [-Sql]\n" +
                "  -load {configuration-name} [-Sql]\n" +
                "  -create {table-name} [-Overwrite]\n";

            Console.WriteLine(helpText);
            return Task.CompletedTask;
        }
    }
}
