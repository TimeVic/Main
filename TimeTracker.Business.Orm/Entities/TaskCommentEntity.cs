using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "task_comments")]
    public class TaskCommentEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "comment", Length = 5048, NotNull = true)]
        public virtual string Comment { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "is_archived", NotNull = true)]
        public virtual bool IsArchived { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual UserEntity? User { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "updated_user_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual UserEntity? UpdatedUser { get; set; }
        
        [ManyToOne(
            ClassType = typeof(TaskEntity), 
            Column = "task_id", 
            Lazy = Laziness.Proxy,
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual TaskEntity Task { get; set; }
    }
}
