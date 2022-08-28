using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Transactions.Behaviors
{
    public interface IExpectCommit
    {
        Task PerformCommitAsync(CancellationToken cancellationToken = default);
    }
}