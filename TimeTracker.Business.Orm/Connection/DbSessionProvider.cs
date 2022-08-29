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
                    if (_isShowSql)
                    {
                        _session = _sessionFactory.WithOptions()
                            .Interceptor(new SqlQueryInterceptor())
                            .OpenSession();
                    }
                    else
                    {
                        _session = _sessionFactory.OpenSession();
                    }
                }

                lock (_session)
                {
                    if (_transaction == null || !_transaction.IsActive)
                    {
                        _transaction = _session.BeginTransaction();
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
        
        public async Task RefreshEntityAsync(object entity, CancellationToken cancellationToken = default)
        {
            if (_session != null && _session.IsOpen)
            {
                try
                {
                    await _session.RefreshAsync(entity, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    throw;
                }
            }
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
