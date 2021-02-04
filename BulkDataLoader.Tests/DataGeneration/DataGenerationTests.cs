using BulkDataLoader.Lists;
using Moq;
using System;
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
    }
}
