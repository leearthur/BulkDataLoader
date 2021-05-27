using BulkDataLoader.Exceptions;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public class CreateTask : ApplicationTask
    {
        public bool Overwrite { get; }

        private readonly TableInformation _tableInformation;

        public CreateTask(Configuration configuration, IEnumerable<string> settings)
            : base(configuration, settings)
        {
            if (string.IsNullOrWhiteSpace(Configuration.TableName))
            {
                throw new RequestValidationException("{table-name} not specified in request");
            }

            _tableInformation = new TableInformation(Configuration.TableName);
            Overwrite = SettingExists("Overwrite");
        }

        public override async Task ExecuteAsync()
        {
            Log.Information("Starting configuration creation...");

            var nameParts = Configuration.TableName.Split('.').ToArray();
            var columns = (await GetColumnData()).ToArray();
            Log.Information($"[ ] {columns.Length} column details loaded");

            Configuration.Name = _tableInformation.TableName;
            Configuration.Columns = columns.Select(col => new Column
            {
                Name = col.ColumnName,
                Type = MapDataType(col),
                Value = MapValue(col),
                Default = MapDefault(col),
                Properties = MapDefaultProperties(col)
            });

            if (await WriteConfigurationAsync())
            {
                Log.Information("Configuration file creation complete");
            }
        }

        private async Task<IEnumerable<SchemaColumn>> GetColumnData()
        {
            var sql =
                "SELECT COLUMN_NAME ColumnName, DATA_TYPE DataType, IS_NULLABLE = 'NO' NotNull, COLUMN_DEFAULT DefaultValue  " +
                "FROM information_schema.COLUMNS " +
                "WHERE EXTRA != 'auto_increment' " +
                "AND TABLE_NAME = @TableName ";

            if (_tableInformation.HasSchemaName)
            {
                sql += "AND TABLE_SCHEMA = @SchemaName ";
            }

            using var connection = new MySqlConnection(Configuration.DefaultConnectionString);
            return await connection.QueryAsync<SchemaColumn>($"{sql};", new
            {
                _tableInformation.TableName,
                _tableInformation.SchemaName
            });
        }

        private async Task<bool> WriteConfigurationAsync()
        {
            var json = JsonConvert.SerializeObject(Configuration, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
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

        private static string MapDataType(SchemaColumn column)
        {
            return column.DataType switch
            {
                "text" or "varchar" => "string",
                "date" or "datetime" or "timestamp" => "date",
                "bit" => "boolean",
                "tinyint" or "smallint" or "mediumint" or "int" or "bigint" or "decimal" or "double" => "numeric",
                _ => "unknown",
            };
        }

        private static string MapValue(SchemaColumn column)
        {
            return column.DataType switch
            {
                "text" or "varchar" => column.NotNull ? string.Empty : null,
                "date" or "datetime" or "timestamp" => "NOW",
                _ => null,
            };
        }

        private static object MapDefault(SchemaColumn column)
        {
            if (column.DefaultValue == null)
            {
                return null;
            }

            return column.DataType switch
            {
                "text" or "varchar" or "date" or "datetime" or "timestamp" => column.DefaultValue,
                "bit" or "tinyint" or "smallint" or "mediumint" or "int" => int.Parse(column.DefaultValue),
                "bigint" => long.Parse(column.DefaultValue),
                "decimal" => decimal.Parse(column.DefaultValue),
                "double" => double.Parse(column.DefaultValue),
                _ => null,
            };
        }

        private static Dictionary<string, object> MapDefaultProperties(SchemaColumn column)
        {
            var properties = new Dictionary<string, object>();
            switch (column.DataType)
            {
                case "bit":
                    AddMinMaxValueProperties(properties, 0, 1);
                    break;

                case "tinyint":
                    AddMinMaxValueProperties(properties, -128, 127);
                    break;

                case "smallint":
                    AddMinMaxValueProperties(properties, short.MinValue, short.MaxValue);
                    break;

                case "mediumint":
                    AddMinMaxValueProperties(properties, -8388608, 8388607);
                    break;

                case "int":
                case "bigint":
                    AddMinMaxValueProperties(properties, int.MinValue, int.MaxValue);
                    break;
            }

            return properties.Count > 0 ? properties : null;
        }

        private static void AddMinMaxValueProperties(Dictionary<string, object> properties, long minValue, long maxValue)
        {
            properties.Add("minValue", minValue);
            properties.Add("maxValue", maxValue);
        }
    }

    internal class SchemaColumn
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool NotNull { get; set; }
        public string DefaultValue { get; set; }
    }
}
