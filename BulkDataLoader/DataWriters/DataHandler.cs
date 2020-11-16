using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace BulkDataLoader.DataWriters
{
    public abstract class DataHandler
    {
        public abstract string FileExtension { get; }
        public abstract char QuoteCharacter { get; }

        public Configuration Configuration { get; private set; }
        public DirectoryInfo OutputDirectory { get; private set; }

        public abstract Task WriteAsync(IEnumerable<DataRow> data, FileMode fileMode);
        public abstract Task LoadAsync();

        protected void Init(Configuration configuration)
        {
            Configuration = configuration;
            OutputDirectory = new DirectoryInfo(configuration.OutputFileLocation);
        }

        public static DataHandler GetInstance(Configuration configuration, OutputType outputType)
        {
            var instance = GetTypeIstance(outputType);
            instance.Init(configuration);

            return instance;
        }

        protected FileInfo GetFileInfo(bool createDirectory = false)
        {
            if (createDirectory && !OutputDirectory.Exists)
            {
                OutputDirectory.Create();
            }

            var fileName = $"{Configuration.FileName ?? Configuration.Name}_data";
            return new FileInfo($@"{OutputDirectory}\{fileName}.{FileExtension}");
        }

        private static DataHandler GetTypeIstance(OutputType outputType)
        {
            return outputType switch
            {
                OutputType.Csv => new CsvDataHandler(),
                OutputType.Sql => new SqlDataHandler(),
                _ => throw new InvalidEnumArgumentException($"Invalid OutputType '{outputType}' specified."),
            };
        }
    }
}
