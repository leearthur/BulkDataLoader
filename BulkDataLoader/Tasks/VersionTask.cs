using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class VersionTask : ApplicationTask
    {
        public VersionTask()
            : base(null, null)
        {
        }

        public override bool DisplayExecutionTime => false;

        public override Task ExecuteAsync()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine($"BulkDataLoader {assembly.Version}");
            return Task.CompletedTask;
        }
    }
}
