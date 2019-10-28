using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace BulkDataLoader.DataWriters
{
    public abstract class DataWriter
    {
        public abstract string FileExtension { get; }
        public abstract char QuoteCharacter { get; }

        public Configuration Configuration { get; private set; }
        public DirectoryInfo OutputDirectory { get; private set;  }

        public abstract Task Write(IEnumerable<DataRow> data, string fileName, FileMode fileMode);

        protected void Init(Configuration configuration, string outputDirectory)
        {
            Configuration = configuration;
            OutputDirectory = new DirectoryInfo(outputDirectory);
        }

        public static DataWriter GetInstance(Configuration configuration, OutputType outputType, string outputDirectory)
        {
            var instance = GetTypeIstance(outputType);
            instance.Init(configuration, outputDirectory);

            return instance;
        }

        protected FileInfo GetFileInfo(string fileName)
        {
            if (!OutputDirectory.Exists)
            {
                OutputDirectory.Create();
            }

            return new FileInfo($@"{OutputDirectory}\{fileName}.{FileExtension}");
        }

        private static DataWriter GetTypeIstance(OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Csv: return new CsvDataWriter();
                case OutputType.Sql: return new SqlDataWriter();
            }

            throw new InvalidEnumArgumentException($"Invalid OutputType '{outputType.ToString()}' specified.");
        }
    }
}
