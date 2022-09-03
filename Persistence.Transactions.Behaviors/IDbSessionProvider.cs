using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;

namespace Persistence.Transactions.Behaviors
{
    public interface IDbSessionProvider: IDisposable, IExpectCommit
    {
        ISession CurrentSession { get; }

        ISession CreateSession();
    }
}
