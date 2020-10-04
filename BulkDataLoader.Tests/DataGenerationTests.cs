using BulkDataLoader.Lists;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests
{
    public class DataGenerationTests
    {
        private Configuration _configuration;
        private Mock<IListCollection> _listCollectionMock;

        public DataGenerationTests()
        {
            _configuration = new Configuration();
            _listCollectionMock = new Mock<IListCollection>();
        }

        [Fact]
        public async Task GenerateCsvRows_InvalidColumnType_ThrowsArgumentException()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CustomerId", "invalid_type");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);
            await Assert.ThrowsAsync<ArgumentException>(() => target.GenerateRowsAsync(1, '"'));
        }

        [Fact]
        public async Task GenerateCsvRows_ZeroRows_EmptyArrayReturns()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserName", "string");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(0, '"');

            Assert.Empty(response);
        }

        [Fact]
        public async Task GenerateCsvRows_String_SingleRow()
        {
            const string testValue = "--Test Value--";
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "string", testValue);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal($"\"{testValue}\"", data.Value);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_SingleRow()
        {
            var testDateTime = DateTime.Now;
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", testDateTime.ToString());

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal($"\"{testDateTime.ToString(DataGenerator.DateTimeFormat)}\"", data.Value);
        }       

        [Fact]
        public async Task GenerateCsvRows_NumericRange_SingleRow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "numeric", null, new Dictionary<string, object>
                {
                    { "minValue", 100 },
                    { "maxValue", 200 }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(1, '"')).ToArray();

            var data = response.Single().Columns.Single();
            Assert.InRange(int.Parse(data.Value), 100, 200);
        }

        [Fact]
        public async Task GenerateCsvRows_NumericIndex_MultipleRows()
        {
            const int rowCount = 5;
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "numeric", "##INDEX##");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(rowCount, '"')).ToArray();

            Assert.Equal(rowCount, response.Length);
            for (var index = 0; index < rowCount; index++)
            {
                var actual = response[index].Columns.Single().Value;
                Assert.Equal(index, int.Parse(actual));
            }
        }

        [Fact]
        public async Task GenerateCsvRows_GuidGenerated_SingleRow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("RowIdentifier", "guid");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.StartsWith("\"", data.Value);
            Assert.EndsWith("\"", data.Value);
            Guid.Parse(data.Value[1..^1]);
        }

        [Fact]
        public async Task GenerateCsvRows_GuidIndex_MultipleRows()
        {
            const int rowCount = 3;
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "guid", "INDEXED");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(rowCount, '"')).ToArray();

            Assert.Equal(rowCount, response.Length);
            for (var index = 0; index < rowCount; index++)
            {
                var expected = $"\"{index.ToString().PadLeft(8, '0')}-0000-0000-0000-000000000000\"";
                var actual = response[index].Columns.Single().Value;
                Assert.Equal(expected, actual);
                Guid.Parse(actual[1..^1]);
            }
        }

        [Fact]
        public async Task GenerateCsvRows_ListSingle_SingleRow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserSurname", "list", "{company-name}");

            _listCollectionMock.Setup(m => m.Get("company-name")).Returns("Acme Corp");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal($"\"Acme Corp\"", data.Value);
        }

        [Fact]
        public async Task GenerateCsvRows_ListMultiple_SingleRow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserSurname", "list", "{first-name} {surname}");

            _listCollectionMock.Setup(m => m.Get("first-name")).Returns("Lee");
            _listCollectionMock.Setup(m => m.Get("surname")).Returns("Richardson");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal($"\"Lee Richardson\"", data.Value);
        }
    }
}
