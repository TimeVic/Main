using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "workspaces")]
    public class WorkspaceEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "name", Length = 200, NotNull = true)]
        public virtual string Name { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "is_default", NotNull = true)]
        public virtual bool IsDefault { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [ManyToOne(
            ClassType = typeof(UserEntity), 
            Column = "user_id", 
            Lazy = Laziness.False,
            Cascade = "none"
        )]
        public virtual UserEntity User { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "all-delete-orphan"
        )]
        [Key(Column = "workspace_id")]
        [OneToMany(ClassType = typeof(ClientEntity))]
        public virtual ICollection<ClientEntity> Clients { get; set; } = new List<ClientEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.True,
            Cascade = "all-delete-orphan"
        )]
        [Key(Column = "workspace_id")]
        [OneToMany(ClassType = typeof(ProjectEntity))]
        public virtual ICollection<ProjectEntity> Projects { get; set; } = new List<ProjectEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.True,
            Cascade = "none"
        )]
        [Key(Column = "workspace_id")]
        [OneToMany(ClassType = typeof(TimeEntryEntity))]
        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.True,
            Cascade = "none"
        )]
        [Key(Column = "workspace_id")]
        [OneToMany(ClassType = typeof(WorkspaceSettingsClickUpEntity))]
        public virtual ICollection<WorkspaceSettingsClickUpEntity> SettingsClickUp { get; set; } = new List<WorkspaceSettingsClickUpEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.True,
            Cascade = "all-delete-orphan"
        )]
        [Key(Column = "workspace_id")]
        [OneToMany(ClassType = typeof(WorkspaceMembershipEntity))]
        public virtual ICollection<WorkspaceMembershipEntity> Memberships { get; set; } = new List<WorkspaceMembershipEntity>();
        
        #region Other

        public virtual WorkspaceSettingsClickUpEntity? GetClickUpSettings(long userId)
        {
            return SettingsClickUp.FirstOrDefault(
                item => item.User.Id == userId
            );
        }

        public virtual WorkspaceSettingsClickUpEntity? GetClickUpSettings(UserEntity user)
        {
            return GetClickUpSettings(user.Id);
        }
        
        public virtual bool IsIntegrationClickUpActive(long userId)
        {
            var foundIntegration = GetClickUpSettings(userId);
            if (foundIntegration != null)
            {
                return !string.IsNullOrEmpty(foundIntegration.SecurityKey)
                    && !string.IsNullOrEmpty(foundIntegration.TeamId);
            }

            return false;
        }
        
        public virtual bool ContainsProject(ProjectEntity? project)
        {
            if (project == null)
            {
                return false;
            }
            return Projects.Any(item => item.Id == project.Id);
        }
        
        public virtual bool IsOwner(UserEntity user)
        {
            return User.Id == user.Id;
        }
        
        #endregion
    }
}
