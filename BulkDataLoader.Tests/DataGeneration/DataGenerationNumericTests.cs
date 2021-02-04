using BulkDataLoader.Lists;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests
{
    public class DataGenerationNumericTests
    {
        private Configuration _configuration;
        private Mock<IListCollection> _listCollectionMock;

        public DataGenerationNumericTests()
        {
            _configuration = new Configuration();
            _listCollectionMock = new Mock<IListCollection>();
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
        public async Task GenerateCsvRows_NumericRange_FixedValue()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "numeric", "64");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(1, '"')).Single();

            var value = response.Columns.Single().Value;
            Assert.Equal("64", value);
        }

        [Fact]
        public async Task GenerateCsvRows_NumericRange_MaxValueValidation()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "numeric", null, new Dictionary<string, object>
                {
                    { "minValue", 0 },
                    { "maxValue", 1 }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(100, '"')).ToArray();

            var values = response.SelectMany(r => r.Columns.Select(c => c.Value)).GroupBy(v => v);
            Assert.Equal(2, values.Count());
            Assert.Contains(values, v => v.Key == "0");
            Assert.Contains(values, v => v.Key == "1");
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
        public async Task GenerateCsvRows_NumericRange_MaxValueReturnsDifferentValues()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "numeric", null, new Dictionary<string, object>
                {
                    { "minValue", int.MinValue },
                    { "maxValue", int.MaxValue }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = (await target.GenerateRowsAsync(5, '"')).ToArray();

            var data = response.SelectMany(r => r.Columns.Select(c => c.Value));
            var distinct = data.GroupBy(d => d);
            Assert.Equal(5, distinct.Count());
        }
    }
}
