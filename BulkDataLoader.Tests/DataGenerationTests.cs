using BulkDataLoader.Lists;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        public async Task GenerateCsvRows_String_NullInput()
        {
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "string");

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Csv);

            var response = await target.GenerateRowsAsync(1, '"');

            var data = response.Single().Columns.Single();
            Assert.Equal("NULL", data.Value);
        }

        [Fact]
        public async Task GenerateCsvRows_String_SingleRandom()
        {
            const string testValue = "VALUE_##RANDOM(10, 99)##_TEST";
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "string", testValue);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Sql);

            var response = await target.GenerateRowsAsync(1, '\'');

            var testRegex = new Regex(@"^'VALUE_[1-9][0-9]_TEST'$");
            var testResult = testRegex.Match(response.Single().Columns.Single().Value);

            Assert.True(testResult.Success);
            Assert.Single(testResult.Groups);
        }

        [Fact]
        public async Task GenerateCsvRows_String_MultipleRandom()
        {
            const string testValue = "MULTIPLE_##RANDOM(1, 2)##_RANDOM_##RANDOM(3, 4)##_TEST";
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "string", testValue);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Sql);

            var response = await target.GenerateRowsAsync(1, '\'');

            var responseValue = response.Single().Columns.Single().Value;
            var testRegex = new Regex(@"^'MULTIPLE_[1-2]_RANDOM_[3-4]_TEST'$");
            var testResult = testRegex.Match(responseValue);

            Assert.True(testResult.Success, "Result Value: " + responseValue);
            Assert.Single(testResult.Groups);
        }

        [Fact]
        public async Task GenerateCsvRows_String_MultipleRandom_ExpectSameDiffResults()
        {
            const string testValue = "##RANDOM(1, 999999)##_##RANDOM(1, 999999)##";
            var config = new ConfigurationBuilder("Test")
                .AddColumn("UserId", "string", testValue);

            var target = new DataGenerator(config.Build(), _listCollectionMock.Object, OutputType.Sql);

            var response = await target.GenerateRowsAsync(1, '\'');

            var value = response.Single().Columns.Single().Value;
            var values = StripQuotes(value).Split('_');
            Assert.NotEqual(values[0], values[1]);
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
            var responseDate = DateTime.ParseExact(StripQuotes(data.Value), DataGenerator.DateTimeFormat, CultureInfo.InvariantCulture);

            Assert.Equal(responseDate.Date, expectedDate.Date);
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
            var responseDate = DateTime.ParseExact(StripQuotes(data.Value), DataGenerator.DateTimeFormat, CultureInfo.InvariantCulture);

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
            var responseDate = DateTime.ParseExact(StripQuotes(data.Value), DataGenerator.DateTimeFormat, CultureInfo.InvariantCulture);

            Assert.Equal(responseDate.Date, expectedDate.Date);
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

        private string StripQuotes(string value)
        {
            return value[1..^1];
        }
    }
}
