using System.Threading.Tasks;

namespace BulkDataLoader.Tasks
{
    public interface ITask
    {
        bool DisplayExecutionTime { get; }

        Task Execute();
    }
}
