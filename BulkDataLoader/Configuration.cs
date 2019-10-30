using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BulkDataLoader.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
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

        public MySqlConnection GetConnection(string name)
        {
            return new MySqlConnection(ConfigurationManager.ConnectionStrings[name].ConnectionString);
        }

        public string GetApplicationSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        public async Task<string> GetSecureLocation()
        {
            using (var connection = GetConnection("CallCentreDb"))
            {
                const string sql = "SHOW VARIABLES LIKE 'secure_file_priv'";
                var result = (await connection.QueryAsync<MySqlVariable>(sql)).FirstOrDefault();

                return result?.Value;
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
