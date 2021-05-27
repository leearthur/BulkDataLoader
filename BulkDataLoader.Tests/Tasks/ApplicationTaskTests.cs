using BulkDataLoader.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace BulkDataLoader.Tests.Tasks
{
    public class ApplicationTaskTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-task-name")]
        public async Task InvalidTaskName_HelpTaskReturned(string taskName)
        {
            var task = await ApplicationTask.GetTaskInstanceAsync(new[] { taskName });

            Assert.IsType<HelpTask>(task);
        }

        [Fact]
        public async Task NullArgs_HelpTaskReturned()
        {
            var task = await ApplicationTask.GetTaskInstanceAsync(null);

            Assert.IsType<HelpTask>(task);
        }
    }
}
