using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;

namespace TimeTracker.Business.Orm.Entities.WorkspaceAccess
{
    [Class(Table = "workspace_memberships")]
    public class WorkspaceMembershipEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "membership_access_type_id", SqlType = "int", NotNull = true)]
        public virtual MembershipAccessType Access { get; set; }
        
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
            Fetch = FetchMode.Join,
            Cascade = "none"
        )]
        public virtual WorkspaceEntity Workspace { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.Proxy,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "save-update"
        )]
        [Key(Column = "workspace_membership_id")]
        [OneToMany(ClassType = typeof(WorkspaceMembershipProjectAccessEntity))]
        public virtual ICollection<WorkspaceMembershipProjectAccessEntity> ProjectAccesses { get; set; } = new List<WorkspaceMembershipProjectAccessEntity>();
    }
}
