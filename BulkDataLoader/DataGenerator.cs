using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace BulkDataLoader
{
    public class DataGenerator
    {
        private readonly Configuration _configuration;
        private readonly Regex _randomRegex;
        private readonly Random _random;

        public DataGenerator(Configuration configuration)
        {
            _configuration = configuration;
            _randomRegex = new Regex(@"##RANDOM\((\d+),\s*(\d+)\)##");
            _random = new Random();
        }

        public IEnumerable<DataRow> GenerateRows(int count, char quote)
        {
            Log.Information($"[ ] Generating {count} records for {_configuration.Name}");

            var data = new List<DataRow>();
            for (var index = 0; index < count; index++)
            {
                var columns = _configuration.Columns.Select(c => new DataColumn(c, GenerateValue(c, index, quote))).ToList();
                data.Add(new DataRow(columns));
            }

            return data;
        }

        private string GenerateValue(Column column, int lineIndex, char quote)
        {
            switch (column.Type)
            {
                case "string": return $"{quote}{GetFixedValue(column, lineIndex)}{quote}";
                case "date": return $"{quote}{GetDateTime(column)}{quote}";
                case "guid": return $"{quote}{GenerateGuid(column, lineIndex)}{quote}";
                case "numeric": return GetFixedValue(column, lineIndex);
                default:
                    throw new ArgumentException($"Unkown parameter type '{column.Name}' in Configuration '{_configuration.Name}'.");
            }
        }

        private static string GetDateTime(Column column)
        {
            const string dateFormat = "yyyy-MM-dd HH:mm:ss";

            switch (column.Value.ToString().ToUpper())
            {
                case "NOW": return DateTime.Now.ToString(dateFormat);
                default: return DateTime.Parse(column.Value.ToString()).ToString(dateFormat);
            }
        }

        private static string GenerateGuid(Column column, int index)
        {
            if (column.Value != null && column.Value.ToString().Equals("INDEXED", StringComparison.CurrentCultureIgnoreCase))
            {
                return index.ToString("x8") + "-0000-0000-0000-000000000000";
            }

            return Guid.NewGuid().ToString();
        }

        private string GetFixedValue(Column column, int index)
        {
            var value = column.Value
                .ToString()
                .Replace("##INDEX##", index.ToString());

            var randomResult = _randomRegex.Match(value);
            if (randomResult.Success)
            {
                value = value.Replace(randomResult.Value, 
                    _random.Next(int.Parse(randomResult.Groups[1].Value), int.Parse(randomResult.Groups[2].Value)).ToString());
            }

            return value;
        }
    }
}
