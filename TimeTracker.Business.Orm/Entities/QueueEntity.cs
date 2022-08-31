using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Constants;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "queues")]
    public class QueueEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "status", SqlType = "int", NotNull = true)]
        public virtual QueueStatus Status { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "channel", SqlType = "int", NotNull = true)]
        public virtual QueueChannel Channel { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "error", Length = 1000, NotNull = false)]
        public virtual string Error { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "context_type", Length = 512, NotNull = true)]
        public virtual string ContextType { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "context_data", NotNull = true)]
        public virtual string ContextData { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
    }
}
