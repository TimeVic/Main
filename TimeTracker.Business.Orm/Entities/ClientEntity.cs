using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "clients")]
    public class ClientEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 200, NotNull = true)]
        public virtual string Name { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(WorkspaceEntity), 
            Column = "workspace_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "client_id")]
        [OneToMany(ClassType = typeof(ProjectEntity))]
        public virtual ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "client_id")]
        [OneToMany(ClassType = typeof(PaymentEntity))]
        public virtual ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();

        public virtual void AddPayment(PaymentEntity payment)
        {
            Payments.Add(payment);
            payment.Client = this;
        }
    }
}
