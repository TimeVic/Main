using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.User
{
    public class UserEntity: AEntity
    {
        public virtual string? UserName { get; set; }
        public virtual required string Email { get; set; }
        public virtual required string Timezone { get; set; }
        public virtual string? VerificationToken { get; set; }
        public virtual DateTime? VerificationTime { get; set; }
        public virtual byte[]? PasswordSalt { get; set; }
        public virtual byte[]? PasswordHash { get; set; }

        #region Relationships

        public virtual ICollection<WorkspaceEntity> CreatedWorkspaces { get; set; } = new List<WorkspaceEntity>();
        public virtual ICollection<TimeEntryEntity> TimeEntries { get; set; } = new List<TimeEntryEntity>();
        public virtual ICollection<WorkspaceMemberEntity> WorkspaceMembers { get; set; } = new List<WorkspaceMemberEntity>();
        public virtual ICollection<UserNotificationTokenEntity> NotificationTokens { get; set; } = new List<UserNotificationTokenEntity>();
        public virtual ISet<MessagingCounterEntity> MessageCounters { get; set; } = new HashSet<MessagingCounterEntity>();
        public virtual ICollection<StoredFileEntity> Avatars { get; set; } = new List<StoredFileEntity>();

        #endregion
        
        #region Calculated

        public virtual bool IsActivated => VerificationTime.HasValue;

        public virtual string Name => string.IsNullOrEmpty(UserName) ? Email : UserName;
        
        public virtual WorkspaceEntity DefaultWorkspace => CreatedWorkspaces.First(item => item.IsDefault);
        
        #endregion
        
    }
}
