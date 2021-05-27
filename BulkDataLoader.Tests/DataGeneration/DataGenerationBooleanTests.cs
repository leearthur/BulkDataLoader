using BulkDataLoader.Lists;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.DataGeneration
{
    public class DataGenerationBooleanTests
    {
        private readonly Mock<IListCollection> _listCollectionMock;

        public DataGenerationBooleanTests()
        {
            _listCollectionMock = new Mock<IListCollection>();
        }

        [Theory]
        [InlineData("true")]
        [InlineData("1")]
        [InlineData("TRUE")]
        public async Task GenerateCsvRows_Boolean_Truthy(string value)
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("IsActive", "boolean", value);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal("1", data.Value);
        }

        [Theory]
        [InlineData("false")]
        [InlineData("0")]
        [InlineData("FALSE")]
        [InlineData(null)]
        [InlineData("SOMETHING_EKSE")]
        public async Task GenerateCsvRows_Boolean_Falsey(string value)
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("IsActive", "boolean", value);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal("0", data.Value);
        }
    }
}
