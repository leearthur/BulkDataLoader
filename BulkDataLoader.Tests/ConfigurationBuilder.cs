using System.Collections.Generic;

namespace BulkDataLoader.Tests
{
    public class ConfigurationBuilder
    {
        private readonly Configuration _configuration;
        private readonly List<Column> _columns;

        public ConfigurationBuilder()
        {
            _configuration = new Configuration();
        }

        public ConfigurationBuilder(string name)
        {
            _configuration = new Configuration
            {
                Name = name
            };
            _columns = new List<Column>();
        }

        public ConfigurationBuilder WithTableName(string tableName)
        {
            _configuration.TableName = tableName;
            return this;
        }

        public ConfigurationBuilder AddColumn(string name, string type) => AddColumn(name, type, null);

        public ConfigurationBuilder AddColumn(string name, string type, string value) => AddColumn(name, type, value, null);

        public ConfigurationBuilder AddColumn(string name, string type, string value, Dictionary<string, object> properties)
        {
            var column = new Column
            {
                Name = name,
                Type = type,
                Value = value,
                Properties = properties
            };
            _columns.Add(column);

            return this;
        }

        public Configuration Build()
        {
            _configuration.Columns = _columns;
            return _configuration;
        }
    }
}
