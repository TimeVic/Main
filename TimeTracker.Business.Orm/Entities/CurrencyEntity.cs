using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities
{
    public class CurrencyEntity: AEntity
    {
        public virtual required string Code { get; set; }
        public virtual required string Symbol { get; set; }
    }
}
