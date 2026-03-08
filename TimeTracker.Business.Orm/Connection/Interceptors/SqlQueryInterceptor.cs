using NHibernate;
using NHibernate.SqlCommand;
using Serilog;

namespace TimeTracker.Business.Orm.Connection.Interceptors
{
    public class SqlQueryInterceptor : EmptyInterceptor
    {
        private readonly string[] Exclusions = [
            "SELECT",
            "EXEC"
        ];
        
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            var trimmedSql = sql.Trim();
            var isShouldBeLogged = !Exclusions.Any(item => trimmedSql.StartsWithCaseInsensitive(item));
            if (isShouldBeLogged)
            {
                Log.Debug($"NHibernate: {sql}");
            }
            return base.OnPrepareStatement(sql);
        }
    }
}
