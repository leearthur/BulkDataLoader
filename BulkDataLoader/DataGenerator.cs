using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BulkDataLoader.Lists;
using Serilog;

namespace BulkDataLoader
{
    public class DataGenerator
    {
        private readonly Configuration _configuration;
        private readonly OutputType _outputType;
        private readonly Regex _randomRegex;
        private readonly Random _random;
        private readonly ListCollection _listCollection;

        public DataGenerator(Configuration configuration, OutputType outputType)
        {
            _configuration = configuration;
            _outputType = outputType;
            _randomRegex = new Regex(@"##RANDOM\((\d+),\s*(\d+)\)##");
            _random = new Random();
            _listCollection = new ListCollection();
        }

        public IEnumerable<DataRow> GenerateRows(int count, char quote)
        {
            Log.Information($"[ ] Generating {count} records for {_configuration.Name}");

            _listCollection.Load(_configuration);

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
                case "string": return $"{quote}{GetStringValue(column, lineIndex)}{quote}";
                case "date": return $"{quote}{GetDateTime(column)}{quote}";
                case "guid": return $"{quote}{GenerateGuid(column, lineIndex)}{quote}";
                case "numeric": return GetNumericValue(column, lineIndex);
                case "list": return $"{quote}{GetListValue(column, lineIndex)}{quote}";
                default:
                    throw new ArgumentException($"Unkown parameter type '{column.Type}' for column '{column.Name}' in configuration '{_configuration.Name}'.");
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

        private string GetNumericValue(Column column, int index)
        {
            if (column.Value != null)
            {
                return GetFixedValue(column, index);
            }

            var minValue = column.Properties.Get("minValue", int.MinValue);
            var maxValue = column.Properties.Get("maxValue", int.MaxValue);

            return _random.Next(minValue, maxValue).ToString();
        }

        private string GetStringValue(Column column, int index)
        {
            var value = GetFixedValue(column, index);
            return _outputType == OutputType.Sql
                ? SqlParse(value)
                : value;
        }

        private string GetFixedValue(Column column, int index)
        {
            var value = column.Value
                .ToString()
                .Replace("##INDEX##", GetIndexValue(column, index));

            var randomResult = _randomRegex.Match(value);
            if (randomResult.Success)
            {
                value = value.Replace(randomResult.Value, 
                    _random.Next(int.Parse(randomResult.Groups[1].Value), int.Parse(randomResult.Groups[2].Value)).ToString());
            }

            return value;
        }

        private string GetListValue(Column column, int index)
        {
            var value = column.Value.ToString();
            var items = _listCollection.ExtractListNames(value);

            foreach (var listName in items)
            {
                value = value.Replace(listName, _listCollection.Get(listName[1..^1]));
            }

            return _outputType == OutputType.Sql
                ? SqlParse(value)
                : value;
        }

        private string GetIndexValue(Column column, int index)
        {
            var startValue = column.Properties.Get("indexStartValue", 0);
            var maxValue = column.Properties.Get("indexMaxValue", int.MaxValue);

            var result = Math.Min(startValue + index, maxValue);
            return result.ToString();
        }

        private string SqlParse(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
