using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Core;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Entities.Messaging
{
    public class MessagingChannelMemberEntity: AEntity
    {
        #region Relationships

        public virtual required MessagingChannelEntity Channel { get; set; }
        public virtual required UserEntity Member { get; set; }

        #endregion
    }
}
