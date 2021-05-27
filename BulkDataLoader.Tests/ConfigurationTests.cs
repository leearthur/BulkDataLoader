using BulkDataLoader.Exceptions;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests
{
    public class ConfigurationTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-config-file")]
        public async Task InvalidConfigurationName_RequestValidationExcepitonThrown(string configName)
        {
            await Assert.ThrowsAsync<RequestValidationException>(() => Configuration.Load(configName));
        }
    }
}
