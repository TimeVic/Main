using System.Reflection;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.Attributes;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;

namespace TimeTracker.Business.Orm.Connection
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _dbNamespace = "TimeTracker.Business.Orm.Hibernate";
        private readonly IConfiguration _configuration;
        private ISessionFactory? _sessionFactory = null;

        public DbConnectionFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public Task<ISessionFactory> GetSessionFactoryAsync()
        {
            if (_sessionFactory == null)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var properties = new Dictionary<string, string>
                {
                    { "connection.connection_string", connectionString },
                    { "dialect", "NHibernate.Dialect.PostgreSQL83Dialect" }
                };
                var isShowSql = _configuration.GetValue<bool>("Hibernate:IsShowSql", false);
                if (isShowSql)
                {
                    properties.Add("show_sql", "true");            
                    properties.Add("format_sql", "true");
                }
                _sessionFactory = BuildFactory(properties);
            }
            return Task.FromResult(_sessionFactory);
        }

        public void Dispose()
        {
            _sessionFactory?.Close();
        }

        private ISessionFactory BuildFactory(IDictionary<string, string> properties)
        {
            // Logging

            var currentAssembly = Assembly.GetExecutingAssembly();
            var hibernateConfiguration = new Configuration()
                .AddProperties(properties)
                .Configure(currentAssembly, $"{_dbNamespace}.hibernate.hbm.xml");

            // Enable validation (optional)
            HbmSerializer.Default.Validate = true;

            // Import all entities and queries
            hibernateConfiguration.AddInputStream(HbmSerializer.Default.Serialize(currentAssembly));
            // Import all mapping files
            var classes = currentAssembly.GetManifestResourceNames()
                .Where(resourceName => resourceName.StartsWith($"{_dbNamespace}.Queries"));
            foreach (var resourceName in classes)
            {
                var filePath = IoUtils.GetResourcePath(currentAssembly, resourceName);
                hibernateConfiguration.AddInputStream(currentAssembly.GetManifestResourceStream(filePath));
            }
            return hibernateConfiguration.BuildSessionFactory();
        }
    }
}
