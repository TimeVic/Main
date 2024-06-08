using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    [Class(Table = "goals_tracker_items")]
    public class GoalsTrackerItemEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 200, NotNull = true)]
        public virtual string Name { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "number_of_times", NotNull = true)]
        public virtual int NumberOfTimes { get; set; }

        [Property(NotNull = true)]
        [Column(Name = "position", NotNull = true)]
        public virtual int Position { get; set; } = 0;
        
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
            ClassType = typeof(GoalsTrackerEntity), 
            Column = "goals_tracker_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual GoalsTrackerEntity Tracker { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "none"
        )]
        [Key(Column = "goals_tracker_item_id")]
        [OneToMany(ClassType = typeof(GoalsTrackerCompletionMarkerEntity))]
        public virtual ICollection<GoalsTrackerCompletionMarkerEntity> CompletionMarkers { get; set; } = new List<GoalsTrackerCompletionMarkerEntity>();
        
        // public virtual void SetClient(ClientEntity? client)
        // {
        //     if (Client?.Id == client?.Id)
        //     {
        //         return;
        //     }
        //
        //     Client = client;
        //     client?.Projects.Add(this);
        // }
    }
}
