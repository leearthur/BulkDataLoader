using System;
using Xunit;

namespace BulkDataLoader.Tests
{
    public class TableInformationTests
    {
        private const string TestTableName = "test_table";
        private const string TestSchemaName = "test_schema";

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("	")]
        public void NullOrWhiteSpaceInput_ArgumentExceptionThrown(string tableName)
        {
            Assert.Throws<ArgumentException>(() => new TableInformation(tableName));
        }

        [Fact]
        public void OnlyTableNameSpecified_SchemaIsNull()
        {
            var target = new TableInformation(TestTableName);

            Assert.Equal(TestTableName, target.TableName);
            Assert.Null(target.SchemaName);
            Assert.False(target.HasSchemaName);
        }

        [Fact]
        public void TableAndSchmeaNamesSpecified_SchemaIsSet()
        {
            var target = new TableInformation($"{TestSchemaName}.{TestTableName}");

            Assert.Equal(TestTableName, target.TableName);
            Assert.Equal(TestSchemaName, target.SchemaName);
            Assert.True(target.HasSchemaName);
        }
    }
}
