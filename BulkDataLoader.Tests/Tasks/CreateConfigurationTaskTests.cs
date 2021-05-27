using BulkDataLoader.Exceptions;
using BulkDataLoader.Tasks;
using System;
using Xunit;

namespace BulkDataLoader.Tests.Tasks
{
    public class CreateConfigurationTaskTests
    {
        #region Validation Tests
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void InvalidTable_ReuqestValidationExceptionThrown(string tableName)
        {
            var config = new ConfigurationBuilder().WithTableName(tableName).Build();
            Assert.Throws<RequestValidationException>(() => new CreateConfigurationTask(config, Array.Empty<string>()));
        }
        #endregion
    }
}
