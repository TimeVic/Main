using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingChannelEntity: AEntity
    {
        public virtual MessagingChannelType Type { get; set; }
        public virtual string Slug { get; set; } = string.Empty;
        
        #region Relationships

        public virtual UserEntity? User { get; set; }
        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity CreatedBy { get; set; }
        public virtual ICollection<MessagingMessageEntity> Messages { get; set; } = new List<MessagingMessageEntity>();
        public virtual ISet<MessagingChannelMemberEntity> Members { get; set; } = new HashSet<MessagingChannelMemberEntity>();
        
        #endregion

        #region Calculated

        public virtual ISet<MessagingChannelMemberEntity> ActiveMembers => Members.Where(item => item.DeactivatedAt == null).ToHashSet();

        #endregion
    }
}
