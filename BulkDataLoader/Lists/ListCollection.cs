using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BulkDataLoader.Lists
{
    public class ListCollection
    {
        public Dictionary<string, IEnumerable<string>> Lists { get; } = new Dictionary<string, IEnumerable<string>>();
        private Random _random = new Random();

        public void Load(Configuration configuration)
        {
            var lists = configuration.Columns
                .Where(c => c.Type == "list")
                .SelectMany(c => ExtractListNames(c.Value.ToString()))
                .Distinct();

            foreach (var list in lists)
            {
                Load(list[1..^1]);
            }
        }

        public void Load(string name)
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

            using (var stream = configFile.OpenText())
            {
                var listJson = stream.ReadToEnd();
                var list = JsonConvert.DeserializeObject<IEnumerable<string>>(listJson);

                Lists.Add(name, list);
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

        public IEnumerable<string> ExtractListNames(string value)
        {
            var pattern = @"(?<!{){[^{}]+}(?!})";
            return Regex.Matches(value, pattern).Select(m => m.ToString());
    }

        private static DirectoryInfo GetLocation()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        }
    }
}
