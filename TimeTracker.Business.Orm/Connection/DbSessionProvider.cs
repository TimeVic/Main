using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Connection.Interceptors;

namespace TimeTracker.Business.Orm.Connection
{
    public class DbSessionProvider : IDbSessionProvider
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<IDbConnectionFactory> _logger;
        private readonly IConfiguration _configuration;

        private ISession _session { get; set; }
        
        private ISessionFactory _sessionFactory { get; set; }

        private bool _isShowSql { get; }
        
        public ISession CurrentSession {
            get {
                if (_session == null || !_session.IsOpen)
                {
                    _session = CreateSession();
                }

                lock (_session)
                {
                    if (_transaction == null || !_transaction.IsActive)
                    {
                        _transaction = _session.BeginTransaction(
                            IsolationLevel.ReadCommitted    
                        );
                    }    
                }
                return _session;
            }
        }

        private ITransaction _transaction;

        public DbSessionProvider(
            IDbConnectionFactory dbConnectionFactory, 
            ILogger<IDbConnectionFactory> logger,
            IConfiguration configuration
        )
        {
            _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
            _sessionFactory = _dbConnectionFactory.GetSessionFactoryAsync().Result;
            _isShowSql = _configuration.GetValue<bool>("Hibernate:IsShowSql", false);
        }

        ~DbSessionProvider()
        {
            //Dispose();
        }

        public async Task PerformCommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null && _transaction.IsActive && _session.IsOpen)
            {
                try
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    await _transaction.RollbackAsync(cancellationToken);
                    throw e;
                }
            }
            _transaction?.Dispose();
            _transaction = null;
        }

        public ISession CreateSession()
        {
            if (_isShowSql)
            {
                return _sessionFactory.WithOptions()
                    .Interceptor(new SqlQueryInterceptor())
                    .OpenSession();
            }
            return _sessionFactory.OpenSession();
        }
        
        #region IDisposable implementation
        public void Dispose()
        {
            if (_session != null)
            {
                _session.Close();
                _session.Dispose();
                _session = null;
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
