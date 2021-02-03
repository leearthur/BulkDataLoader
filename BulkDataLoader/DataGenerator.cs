﻿using BulkDataLoader.Lists;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BulkDataLoader
{
    public class DataGenerator
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly Configuration _configuration;
        private readonly IListCollection _listCollection;
        private readonly OutputType _outputType;
        private readonly Regex _randomRegex;
        private readonly Random _random;

        public DataGenerator(Configuration configuration, IListCollection listCollection, OutputType outputType)
        {
            _configuration = configuration;
            _outputType = outputType;
            _randomRegex = new Regex(@"##RANDOM\((\d+),\s*(\d+)\)##");
            _random = new Random();
            _listCollection = listCollection;
        }

        public async Task<IEnumerable<DataRow>> GenerateRowsAsync(int count, char quote)
        {
            Log.Information($"[ ] Generating {count} records for {_configuration.Name}");

            if (count == 0)
            {
                return new DataRow[0];
            }

            await _listCollection.LoadAsync(_configuration);

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
            return column.Type switch
            {
                "string" => SurroundWithQuotes(Parse(GetFixedValue(column, lineIndex)), quote),
                "date" => SurroundWithQuotes(GetDateTime(column), quote),
                "guid" => SurroundWithQuotes(GenerateGuid(column, lineIndex), quote),
                "numeric" => GetNumericValue(column, lineIndex),
                "list" => SurroundWithQuotes(Parse(GetListValue(column, lineIndex)), quote),
                "boolean" => GetBooleanValue(column),
                _ => throw new ArgumentException($"Unkown parameter type '{column.Type}' for column '{column.Name}' in configuration '{_configuration.Name}'."),
            };
        }

        private static string GetDateTime(Column column)
        {
            return (column.Value.ToString().ToUpper()) switch
            {
                "NOW" => DateTime.Now.ToString(DateTimeFormat),
                "YESTERDAY" => DateTime.Now.AddDays(-1).ToString(DateTimeFormat),
                "TOMORROW" => DateTime.Now.AddDays(1).ToString(DateTimeFormat),
                _ => DateTime.Parse(column.Value.ToString()).ToString(DateTimeFormat),
            };
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
            var maxValue = column.Properties.Get("maxValue", int.MaxValue - 1);

            return _random.Next(minValue, maxValue + 1).ToString();
        }

        private string GetBooleanValue(Column column)
        {
            if (column.Value == null)
            {
                return "0";
            }
            
            return ((string)column.Value).ToLower()  == "true" || (string)column.Value == "1" ? "1" : "0";
        }

        private string GetFixedValue(Column column, int index)
        {
            if (column.Value == null)
            {
                return null;
            }

            var value = column.Value
                .ToString()
                .Replace("##INDEX##", GetIndexValue(column, index));

            foreach (Match match in _randomRegex.Matches(value))
            {
                var newValue = _random.Next(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)).ToString();
                value = value.ReplaceFirst(match.Value, newValue);
            }

            return value;
        }

        private string GetListValue(Column column, int index)
        {
            var value = column.Value.ToString();
            var items = ListCollection.ExtractListNames(value);

            foreach (var listName in items)
            {
                value = value.Replace(listName, _listCollection.Get(listName[1..^1]));
            }

            return value;
        }

        private string GetIndexValue(Column column, int index)
        {
            var startValue = column.Properties.Get("indexStartValue", 0);
            var maxValue = column.Properties.Get("indexMaxValue", int.MaxValue);

            var result = Math.Min(startValue + index, maxValue);
            return result.ToString();
        }

        private string Parse(string value)
        {
            if (value == null)
            {
                return null;
            }

            return _outputType == OutputType.Sql
                ? value.Replace("'", "''")
                : value;
        }

        private string SurroundWithQuotes(string value, char quote)
        {
            return value != null
                ? $"{quote}{value}{quote}"
                : "NULL";
        }
    }
}
