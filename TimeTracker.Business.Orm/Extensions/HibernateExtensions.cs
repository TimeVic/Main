using System.Collections;
using NHibernate.Collection;

namespace TimeTracker.Business.Orm.Extensions
{
    public static class HibernateExtensions
    {
        public static bool IsHibernateLazy(this ICollection collection)
        {
            if (collection is ILazyInitializedCollection)
            {
                return true;
            }
            return false;
        }
        
        public static bool IsHibernateLazy<T>(this ICollection<T> collection)
        {
            if (collection is ILazyInitializedCollection)
            {
                return true;
            }
            return false;
        }
    }
}
