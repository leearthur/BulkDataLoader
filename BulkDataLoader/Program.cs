﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BulkDataLoader.Tasks;
using Serilog;

namespace BulkDataLoader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                var task = ApplicationTask.GetTaskInstance(args);
                var timer = new Stopwatch();

                timer.Start();
                Task.Run(() => task.Execute()).GetAwaiter().GetResult();
                timer.Stop();

                if (task.DisplayExecutionTime)
                {
                    var executionTime = Math.Round((decimal) timer.ElapsedMilliseconds / 1000, 4);
                    Log.Information($"Execution Time: {executionTime} seconds");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured");
            }
        }
    }
}