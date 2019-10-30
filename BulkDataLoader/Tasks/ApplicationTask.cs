using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public abstract class ApplicationTask : ITask
    {
        public Configuration Configuration { get; }
        public string[] Settings { get; }
        public virtual bool DisplayExecutionTime { get; } = true;

        protected ApplicationTask(Configuration configuration, IEnumerable<string> settings)
        {
            Configuration = configuration;
            Settings = settings?.ToArray() ?? new string[0];
        }

        public abstract Task Execute();

        public static ITask GetTaskInstance(string[] args)
        {
            var taskName = args.Length > 0 ? args[0].ToLower() : null;
            switch (taskName)
            {
                case "-generate":
                    return new GenerateDataTask(Configuration.Load(args[1]), args.Skip(2));

                case "-load": 
                    return new DataLoadTask(Configuration.Load(args[1]), args.Skip(2));

                case "-create":
                    return new CreateConfigurationTask(new Configuration
                    {
                        TableName = args[1]
                    }, args.Skip(2));

                case "-settings": 
                    return new SettingsTask(Configuration.Load(args[1]), args.Skip(1));

                default:
                    return new HelpTask();
            }
        }

        protected bool SettingExists(string name)
        {
            return Settings.Any(a => a.Equals($"-{name}", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}