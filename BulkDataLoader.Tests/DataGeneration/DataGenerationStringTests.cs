using BulkDataLoader.Lists;
using Moq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.DataGeneration
{
    public class DataGenerationStringTests
    {
        private readonly Mock<IListCollection> _listCollectionMock;

        public DataGenerationStringTests()
        {
            _listCollectionMock = new Mock<IListCollection>();
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
            var values = value.StripQuotes().Split('_');
            Assert.NotEqual(values[0], values[1]);
        }
    }
}
