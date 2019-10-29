using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Serilog;

namespace BulkDataLoader.Tasks
{
    public class SettingsTask : ApplicationTask
    {
        public override bool DisplayExecutionTime { get; } = false;

        public override async Task Execute()
        {
            var secureLocation = await GetSecureLocation();
            Log.Information($"MySQL Secure Location: {secureLocation}");
            Log.Information($"Output Location: {GetApplicationSetting("OutputFileLocation")}");
        }

        public async Task<string> GetSecureLocation()
        {
            using (var connection = new MySqlConnection(GetConnectionString("CallCentreDb")))
            {
                const string sql = "SHOW VARIABLES LIKE 'secure_file_priv'";
                var result = (await connection.QueryAsync<MySqlVariable>(sql)).FirstOrDefault();

                return result?.Value;
            }
        }
    }

    public class MySqlVariable
    {
        public string Value { get; set; }
    }
}
