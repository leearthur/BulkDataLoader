﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BulkDataLoader.Lists
{
    public interface IListCollection
    {
        Task LoadAsync(Configuration configuration);
        string Get(string listName);
    }

    public class ListCollection : IListCollection
    {
        public Dictionary<string, IEnumerable<string>> Lists { get; } = new Dictionary<string, IEnumerable<string>>();
        private readonly Random _random = new();

        public async Task LoadAsync(Configuration configuration)
        {
            var lists = configuration.Columns
                .Where(c => c.Type == "list")
                .SelectMany(c => ExtractListNames(c.Value.ToString()))
                .Distinct();

            foreach (var list in lists)
            {
                await LoadListAsync(list[1..^1]);
            }
        }

        public string Get(string listName)
        {
            if (!Lists.ContainsKey(listName))
            {
                throw new ArgumentException($"Invalid list '{listName}' specified");
            }

            var list = Lists[listName];
            var index = _random.Next(0, list.Count() - 1);
            return list.Skip(index).First();
        }

        public static IEnumerable<string> ExtractListNames(string value)
        {
            var pattern = @"(?<!{){[^{}]+}(?!})";
            return Regex.Matches(value, pattern).Select(m => m.ToString());
        }

        private async Task LoadListAsync(string name)
        {
            var configFile = new FileInfo($@"{GetLocation()}\Lists\{name}.json");

            if (!configFile.Exists)
            {
                throw new ArgumentException($"Specified list '{name}' does not exist.");
            }

            if (Lists.ContainsKey(name))
            {
                Lists.Remove(name);
            }

            using var stream = new StreamReader(configFile.FullName, Encoding.UTF8);

            var listJson = await stream.ReadToEndAsync();
            var list = JsonConvert.DeserializeObject<IEnumerable<string>>(listJson);

            Lists.Add(name, list);
        }

        private static DirectoryInfo GetLocation()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        }
    }
}
