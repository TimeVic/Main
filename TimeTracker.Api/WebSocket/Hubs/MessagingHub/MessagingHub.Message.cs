using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using TimeTracker.Api.WebSocket.Services;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Messaging;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;

public partial class MessagingHub
{
    [Authorize]
    public async Task SendMessage(
        Guid workspaceId,
        string messageText,
        Guid? receiverId = null,
        Guid? channelId = null
    )
    {
        await ExecuteInScopeAsync(async sp =>
        {
            var messagingDao = sp.GetRequiredService<IMessagingDao>();
            var workspaceDao = sp.GetRequiredService<IWorkspaceDao>();
            var securityManager = sp.GetRequiredService<ISecurityManager>();
            var userDao = sp.GetRequiredService<IUserDao>();
            var hubMessagingService = sp.GetRequiredService<IHubMessagingService>();

            UserEntity? receiver = null;
            MessagingChannelEntity? channel = null;
            
            var currentUser = await GetCurrentUser(sp);
            var workspace = await workspaceDao.GetById(workspaceId);
            DataValidationException.ThrowIfNull(workspace);
            await securityManager.CheckAccess(AccessLevel.Read, currentUser, workspace);

            if (receiverId != null)
            {
                receiver = await userDao.GetById(receiverId.Value);
                DataValidationException.ThrowIfNull(receiver);
                await securityManager.CheckAccess(AccessLevel.Read, receiver, workspace);
            }
            else if (channelId != null)
            {
                 channel = await messagingDao.GetChannelBy(channelId.Value);
                 DataValidationException.ThrowIfNull(channel);
                 await securityManager.CheckAccess(AccessLevel.Read, currentUser, channel);
            }
            else
            {
                throw new DataValidationException("ChannelId or ReceiverId are required");
            }
        
            await hubMessagingService.SendMessage(
                workspace,
                currentUser, 
                messageText,
                receiver,
                channel
            );
        });
    }
}
