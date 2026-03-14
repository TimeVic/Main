using TimeTracker.Business.Common.Constants.Messaging;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingChannelEntity: AEntity
    {
        public virtual MessagingChannelType Type { get; set; }
        public virtual required string Name { get; set; }

        #region Relationships

        public virtual required WorkspaceEntity Workspace { get; set; }
        public virtual required UserEntity CreatedBy { get; set; }
        public virtual required List<MessagingMessageEntity> Messages { get; set; } = new();
        public virtual required ISet<MessagingChannelMemberEntity> Members { get; set; } = new HashSet<MessagingChannelMemberEntity>();
        
        #endregion
    }
}
