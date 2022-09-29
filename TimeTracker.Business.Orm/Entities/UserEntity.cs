using Domain.Abstractions;
using NHibernate.Mapping.Attributes;
using NHibernate.Type;
using TimeTracker.Business.Common.Exceptions.Common;

namespace TimeTracker.Business.Orm.Entities
{
    [Class(Table = "users")]
    public class UserEntity: IEntity
    {
        [Id(Name = "Id", Generator = "native")]
        [Column(Name = "id", SqlType = "bigint", NotNull = true)]
        public virtual long Id { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "user_name", Length = 200, NotNull = false)]
        public virtual string? UserName { get; set; }
        
        [Property(NotNull = false)]
        [Column(Name = "email", Length = 200, NotNull = true)]
        public virtual string Email { get; set; }

        [Property(NotNull = false)]
        [Column(Name = "verification_token", Length = 512, NotNull = false)]
        public virtual string? VerificationToken { get; set; }

        [Property(NotNull = false, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "verification_time", SqlType = "datetime", NotNull = false)]
        public virtual DateTime? VerificationTime { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "password_salt", SqlType = "bytea", NotNull = true)]
        public virtual byte[]? PasswordSalt { get; set; }
        
        [Property(NotNull = true)]
        [Column(Name = "password_hash", SqlType = "bytea", NotNull = true)]
        public virtual byte[]? PasswordHash { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "create_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime CreateTime { get; set; }
        
        [Property(NotNull = true, TypeType = typeof(UtcDateTimeType))]
        [Column(Name = "update_time", SqlType = "datetime", NotNull = true)]
        public virtual DateTime UpdateTime { get; set; }
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "all-delete-orphan"
        )]
        [Key(Column = "user_id")]
        [OneToMany(ClassType = typeof(WorkspaceEntity))]
        public virtual ICollection<WorkspaceEntity> Workspaces { get; set; } = new List<WorkspaceEntity>();
        
        [Bag(
            Inverse = true,
            Lazy = CollectionLazy.Extra,
            Cascade = "all-delete-orphan"
        )]
        [Key(Column = "user_id")]
        [OneToMany(ClassType = typeof(TimeEntryEntity))]
        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
        
        public virtual bool IsActivated => VerificationTime.HasValue;
        
        public virtual WorkspaceEntity DefaultWorkspace => Workspaces
            .AsQueryable()
            .First(item => item.IsDefault);

        public virtual WorkspaceEntity? GetWorkspaceById(long workspaceId)
        {
            return Workspaces.FirstOrDefault(item => item.Id == workspaceId);
        }
        
        public virtual void EnsureThatHasWorkspace(WorkspaceEntity workspace)
        {
            if (GetWorkspaceById(workspace.Id) == null)
            {
                throw new DataInconsistencyException(
                    $"User does not have workspace: UserId: {Id}, WorkspaceId: {workspace.Id}"
                );
            }
        }
    }
}
