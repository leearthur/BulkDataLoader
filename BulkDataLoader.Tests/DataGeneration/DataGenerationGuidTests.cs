using BulkDataLoader.Lists;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.DataGeneration
{
    public class DataGenerationGuidTests
    {
        private readonly Configuration _configuration;
        private readonly Mock<IListCollection> _listCollectionMock;

        public DataGenerationGuidTests()
        {
            _configuration = new Configuration();
            _listCollectionMock = new Mock<IListCollection>();
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
                var expected = $"\"00000000-000{index}-0000-0000-000000000000\"";
                var actual = response[index].Columns.Single().Value;
                Assert.Equal(expected, actual);
                Guid.Parse(actual[1..^1]);
            }
        }

        [Fact]
        public async Task GenerateCsvRows_GuidGeneratedWithProperties_SingleRow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("RowIdentifier", "guid", "INDEXED", new()
                {
                    { "indexStartValue", "10" },
                    { "guidIndex", "1" }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var expected = "\"00000000-000a-0000-0000-000000000001\"";
            var actual = response.Single().Columns.Single().Value;
            Assert.Equal(expected, actual);
            Guid.Parse(actual[1..^1]);
        }
    }
}
