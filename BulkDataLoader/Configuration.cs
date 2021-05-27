using BulkDataLoader.Exceptions;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
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

        public string OutputFileLocation { get; }
        public string DefaultConnectionString { get; }

        public Configuration()
        {
            var localConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            OutputFileLocation = localConfig.GetSection("OutputFileLocation").Value;
            DefaultConnectionString = localConfig.GetSection("DefaultConnection").Value;
        }

        [JsonIgnore]
        public static DirectoryInfo Location { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        public static async Task<Configuration> Load(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new RequestValidationException("No {configureation-name} specified.");
            }

            var configFile = new FileInfo($@"{Location}\Configurations\{name}.json");
            if (!configFile.Exists)
            {
                throw new RequestValidationException($"Specified {{configuration-name}} '{name}' does not exist.");
            }

            using var stream = configFile.OpenText();

            var configJson = await stream.ReadToEndAsync();
            var config = JsonConvert.DeserializeObject<Configuration>(configJson, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            return config;
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
        public object Default { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class MySqlVariable
    {
        public string Value { get; set; }
    }
}
