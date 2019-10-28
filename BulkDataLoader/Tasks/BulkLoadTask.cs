using System;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class BulkLoadTask : ApplicationTask
    {
        private readonly SettingsTask _settingsTask;
        public BulkLoadTask()
        {
            _settingsTask = new SettingsTask();
        }

        public override async Task Execute()
        {
            Log.Information("Starting bulk data load...");

            await LoadData();

            Log.Information("Bulk data load complete");
        }

        private async Task LoadData()
        {
            using (var connection = new MySqlConnection(GetConnectionString("CallCentreDb")))
            {
                var secureLocation = await _settingsTask.GetSecureLocation();

                var loader = new MySqlBulkLoader(connection)
                {
                    FileName = $@"{secureLocation}\{GetFileName(OutputType.Csv)}",
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    LineTerminator = Environment.NewLine,
                    TableName = Configuration.TableName,
                };
                loader.Columns.AddRange(Configuration.Columns.Select(col => col.Name));

                Log.Information($"[ ] Loading data from file {loader.FileName}");
                connection.Open();
                await loader.LoadAsync();
                connection.Close();
            }
        }
    }
}
