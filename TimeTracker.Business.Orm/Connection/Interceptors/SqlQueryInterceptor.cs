using NHibernate;
using NHibernate.SqlCommand;
using Serilog;

namespace TimeTracker.Business.Orm.Connection.Interceptors
{
    public class SqlQueryInterceptor : EmptyInterceptor
    {
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            Log.Debug($"NHibernate: {sql}");
            return base.OnPrepareStatement(sql);
        }
    }
}
