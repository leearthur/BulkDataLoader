using BulkDataLoader.Lists;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.DataGeneration
{
    public class DataGenerationListTests
    {
        private readonly Mock<IListCollection> _listCollectionMock;

        public DataGenerationListTests()
        {
            _listCollectionMock = new Mock<IListCollection>();
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
