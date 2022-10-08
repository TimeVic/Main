using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "payments")]
    public class PaymentEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "payment_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime PaymentTime { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "description", Length = 512, NotNull = false)]
        public virtual string? Description { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "amount", NotNull = true)]
        public virtual decimal Amount { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ProjectEntity), 
            Column = "project_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual ProjectEntity? Project { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ClientEntity), 
            Column = "client_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual ClientEntity Client { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        [ManyToOne(
            ClassType = typeof(WorkspaceEntity), 
            Column = "workspace_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
    }
}
