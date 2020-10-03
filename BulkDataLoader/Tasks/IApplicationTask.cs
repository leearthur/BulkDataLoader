using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public interface IApplicationTask
    {
        bool DisplayExecutionTime { get; }

        Task ExecuteAsync();
    }
}
