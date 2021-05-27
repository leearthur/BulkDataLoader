using BulkDataLoader.Exceptions;
using BulkDataLoader.Tasks;
using System;
using Xunit;

namespace BulkDataLoader.Tests.Tasks
{
    public class GenerateDataTaskTests
    {
        #region Validation Tests
        [Fact]
        public void NoRecordCount_ReuqestValidationExceptionThrown()
        {
            var config = new ConfigurationBuilder("orders").Build();
            Assert.Throws<RequestValidationException>(() => new GenerateDataTask(config, Array.Empty<string>()));
        }

        [Fact]
        public void InvalidRecordCount_ReuqestValidationExceptionThrown()
        {
            var config = new ConfigurationBuilder("orders").Build();
            Assert.Throws<RequestValidationException>(() => new GenerateDataTask(config, new[] { "A" }));
        }
        #endregion
    }
}
