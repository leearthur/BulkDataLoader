using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BulkDataLoader
{
    public class Configuration
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string TableName { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        [JsonIgnore]
        public DirectoryInfo Location { get; } = GetLocation();

        private static DirectoryInfo GetLocation()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        }

        public static Configuration Load(string name)
        {
            var configFile = new FileInfo($@"{GetLocation()}\Configurations\{name}.json");

            if (!configFile.Exists)
            {
                throw  new ArgumentException($"Specified configuration '{name}' does not exist.");
            }

            using (var stream = configFile.OpenText())
            {
                var configJson = stream.ReadToEnd();
                var config = JsonConvert.DeserializeObject<Configuration>(configJson, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                return config;
            }
        }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }
}
