using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BulkDataLoader
{
    public class Configuration
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string TableName { get; set; }
        public IEnumerable<Column> Columns { get; set; }

        public string OutputFileLocation { get; set; }
        public string DefaultConnectionString { get; set; }

        [JsonIgnore]
        public static DirectoryInfo Location { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        public static async Task<Configuration> Load(string name)
        {
            var configFile = new FileInfo($@"{Location}\Configurations\{name}.json");

            if (!configFile.Exists)
            {
                throw new ArgumentException($"Specified configuration '{name}' does not exist.");
            }

            using var stream = configFile.OpenText();

            var configJson = await stream.ReadToEndAsync();
            var config = JsonConvert.DeserializeObject<Configuration>(configJson, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            EnrichLocalSetting(config);
            return config;
        }

        private static void EnrichLocalSetting(Configuration config)
        {
            var localConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            config.OutputFileLocation = localConfig.GetSection("OutputFileLocation").Value;
            config.DefaultConnectionString = localConfig.GetSection("DefaultConnection").Value;
        }

        public async Task<string> GetSecureLocationAsync()
        {
            using var connection = new MySqlConnection(DefaultConnectionString);

            const string sql = "SHOW VARIABLES LIKE 'secure_file_priv'";
            var result = (await connection.QueryAsync<MySqlVariable>(sql)).FirstOrDefault();

            return result?.Value;
        }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class MySqlVariable
    {
        public string Value { get; set; }
    }
}
