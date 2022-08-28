using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;

namespace Persistence.Transactions.Behaviors
{
    public interface IDbSessionProvider: IDisposable, IExpectCommit
    {
        ISession CurrentSession { get; }

        /// <summary>
        /// Re-read the state of the given instance from the underlying database.
        /// </summary>
        /// <param name="obj">A persistent instance</param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        Task RefreshEntityAsync(object entity, CancellationToken cancellationToken = default);
    }
}