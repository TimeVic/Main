using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "projects")]
    public class ProjectEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 200, NotNull = true)]
        public virtual string Name { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "is_billable_by_default", NotNull = true)]
        public virtual bool IsBillableByDefault { get; set; } = true;
        
        [Property(NotNull = false)]
        [Column(Name = "default_hourly_rate", NotNull = false)]
        public virtual decimal? DefaultHourlyRate { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_archived", NotNull = true)]
        public virtual bool IsArchived { get; set; } = false;
        
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
        
        [ManyToOne(
            ClassType = typeof(ClientEntity), 
            Column = "client_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual ClientEntity? Client { get; set; }

        public virtual void SetClient(ClientEntity? client)
        {
            if (Client?.Id == client?.Id)
            {
                return;
            }

            Client = client;
            client?.Projects.Add(this);
        }
    }
}
