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
        public DirectoryInfo OutputDirectory { get; private set;  }

        public abstract Task Write(IEnumerable<DataRow> data, FileMode fileMode);
        public abstract Task Load(string connectionString);

        protected void Init(Configuration configuration, string outputDirectory)
        {
            Configuration = configuration;
            OutputDirectory = new DirectoryInfo(outputDirectory);
        }

        public static DataHandler GetInstance(Configuration configuration, OutputType outputType, string outputDirectory)
        {
            var instance = GetTypeIstance(outputType);
            instance.Init(configuration, outputDirectory);

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
            switch (outputType)
            {
                case OutputType.Csv: return new CsvDataHandler();
                case OutputType.Sql: return new SqlDataHandler();
            }

            throw new InvalidEnumArgumentException($"Invalid OutputType '{outputType.ToString()}' specified.");
        }
    }
}
