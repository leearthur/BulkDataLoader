using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public abstract class ApplicationTask : IApplicationTask
    {
        public Configuration Configuration { get; }
        public string[] Settings { get; }
        public virtual bool DisplayExecutionTime { get; } = true;
        public IConfigurationRoot ConfigurationRoot { get; private set; }

        protected ApplicationTask(Configuration configuration, IEnumerable<string> settings)
        {
            Configuration = configuration;
            Settings = settings?.ToArray() ?? Array.Empty<string>();
        }

        public abstract Task ExecuteAsync();

        public static async Task<IApplicationTask> GetTaskInstanceAsync(string[] args)
        {
            if (args == null || !args.Any())
            {
                return new HelpTask();
            }

            var taskName = args[0]?.ToLower();
            return taskName switch
            {
                "--version" => new VersionTask(),
                "-generate" => new GenerateTask(await Configuration.Load(args[1]), args.Skip(2)),
                "-load" => new LoadTask(await Configuration.Load(args[1]), args.Skip(2)),
                "-create" => new CreateTask(new Configuration
                {
                    TableName = args[1]
                }, args.Skip(2)),
                _ => new HelpTask(),
            };
        }

        protected bool SettingExists(string name)
        {
            return Settings.Any(a => a.Equals($"-{name}", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}