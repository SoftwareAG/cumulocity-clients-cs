using System.Threading;
using System.Threading.Tasks;
namespace Cumulocity.SDK.Microservices.Utils.Scheduling
{
    public interface IScheduledTask
    {
        string Schedule { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
