using BulkDataLoader.Exceptions;
using BulkDataLoader.Tasks;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BulkDataLoader
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                var task = await ApplicationTask.GetTaskInstanceAsync(args);
                var timer = new Stopwatch();

                timer.Start();
                await task.ExecuteAsync();
                timer.Stop();

                if (task.DisplayExecutionTime)
                {
                    var executionTime = Math.Round((decimal)timer.ElapsedMilliseconds / 1000, 4);
                    Log.Information($"Execution Time: {executionTime} seconds");
                }
            }
            catch (RequestValidationException ex)
            {
                Log.Error($"Request Validation Exception:\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "System Error:");
            }
        }
    }
}
