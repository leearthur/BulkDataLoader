using BulkDataLoader.Lists;
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
            Settings = settings?.ToArray() ?? new string[0];
        }

        public abstract Task ExecuteAsync();

        public static async Task<IApplicationTask> GetTaskInstanceAsync(string[] args)
        {
            var taskName = args.Length > 0 ? args[0].ToLower() : null;
            var listCollection = new ListCollection();
            return taskName switch
            {
                "-generate" => new GenerateDataTask(await Configuration.Load(args[1]), args.Skip(2), listCollection),
                "-load" => new DataLoadTask(await Configuration.Load(args[1]), args.Skip(2)),
                "-create" => new CreateConfigurationTask(new Configuration
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