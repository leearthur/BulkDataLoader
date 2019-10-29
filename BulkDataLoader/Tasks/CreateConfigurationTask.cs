using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class CreateConfigurationTask : ApplicationTask
    {
        public bool Overwrite { get; }

        private const int SchemaPos = 0;
        private const int TablePos = 1;

        public CreateConfigurationTask(Configuration configuration, IEnumerable<string> settings) 
            : base(configuration, settings)
        {
            Overwrite = SettingExists("Overwrite");
        }

        public override async Task Execute()
        {
            Log.Information("Starting configuration creation...");

            var nameParts = Configuration.TableName.Split('.').ToArray();
            var columns = (await GetColumnData(nameParts[SchemaPos], nameParts[TablePos])).ToArray();
            Log.Information($"[ ] {columns.Length} column details loaded");

            Configuration.Name = nameParts[TablePos];
            Configuration.Columns = columns.Select(col => new Column
            {
                Name = col.ColumnName,
                Type = MapDataType(col.DataType)
            });

            if (await WriteConfiguration())
            {
                Log.Information("Configuration file creation complete");
            }
        }

        private async Task<IEnumerable<SchemaColumn>> GetColumnData(string schema, string table)
        {
            const string sql =
                "SELECT COLUMN_NAME ColumnName, DATA_TYPE DataType " +
                "FROM information_schema.COLUMNS " +
                "WHERE TABLE_SCHEMA = @schema " +
                "AND TABLE_NAME = @table;";

            using (var connection = new MySqlConnection(GetConnectionString("CallCentreDb")))
            {
                return await connection.QueryAsync<SchemaColumn>(sql, new {schema, table});
            }
        }

        private async Task<bool> WriteConfiguration()
        {
            var json = JsonConvert.SerializeObject(Configuration, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            var configFile = new FileInfo($@"{Configuration.Location}\Configurations\{Configuration.Name}.json");

            if (!Overwrite && configFile.Exists)
            {
                Log.Warning($"[X] Configuration file '{configFile.Name}' already exists");
                return false;
            }

            using (var stream = configFile.CreateText())
            {
                Log.Information($"[ ] Saving configuration: {configFile.FullName}");
                await stream.WriteAsync(json);
            }

            return true;
        }

        private static string MapDataType(string dataType)
        {
            switch (dataType)
            {
                case "text" : 
                case "varchar" : 
                    return "string";
                
                case "datetime" :
                case "timestamp" : 
                    return "date";

                default: return "value";
            }
        }
    }

    internal class SchemaColumn
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }
}
