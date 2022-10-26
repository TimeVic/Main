using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Constants;

namespace TimeTracker.Business.Orm.Entities.WorkspaceAccess
{
    [Class(Table = "workspace_membership_project_accesses")]
    public class WorkspaceMembershipProjectAccessEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "hourly_rate", NotNull = false)]
        public virtual decimal? HourlyRate { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(WorkspaceMembershipEntity), 
            Column = "workspace_membership_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual WorkspaceMembershipEntity WorkspaceMembership { get; set; }
        
        [ManyToOne(
            ClassType = typeof(ProjectEntity), 
            Column = "project_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual ProjectEntity Project { get; set; }
    }
}
