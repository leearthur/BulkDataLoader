using BulkDataLoader.Lists;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.DataGeneration
{
    public class DataGenerationDateTests
    {
        private Configuration _configuration;
        private Mock<IListCollection> _listCollectionMock;

        public DataGenerationDateTests()
        {
            _configuration = new Configuration();
            _listCollectionMock = new Mock<IListCollection>();
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
        public async Task GenerateCsvRows_Date_Now()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", "NOW");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);
            var expectedDate = DateTime.Now;

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(responseDate.Date, expectedDate.Date);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_AddDays()
        {
            var testDate = DateTime.Now.RemoveMilliseconds();

            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", testDate.ToString(DataGenerator.DateTimeFormat), new Dictionary<string, object>
                {
                    { "addDays", "5" }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(testDate.AddDays(5), responseDate);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_AddHours()
        {
            var testDate = DateTime.Now.RemoveMilliseconds();

            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", testDate.ToString(DataGenerator.DateTimeFormat), new Dictionary<string, object>
                {
                    { "addHours", "6" }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(testDate.AddHours(6), responseDate);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_Yesterday()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", "YESTERDAY");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);
            var expectedDate = DateTime.Now.AddDays(-1);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(responseDate.Date, expectedDate.Date);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_YesterdayAddDays()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", "YESTERDAY", new Dictionary<string, object>
                {
                    { "addDays", "-3" }
                });

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);
            var expectedDate = DateTime.Now.AddDays(-4);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(responseDate.Date, expectedDate.Date);
        }

        [Fact]
        public async Task GenerateCsvRows_Date_Tomorrow()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("CreatedDate", "date", "TOMORROW");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);
            var expectedDate = DateTime.Now.AddDays(1);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            var responseDate = ParseDateString(data);

            Assert.Equal(responseDate.Date, expectedDate.Date);
        }

        private DateTime ParseDateString(DataColumn data)
        {
            return DateTime.ParseExact(data.Value.StripQuotes(), DataGenerator.DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}
