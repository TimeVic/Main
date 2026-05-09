using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;
using GetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message.GetListRequest;
using GetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message.GetListResponse;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task MessagingMessageSendAsync(Guid workspaceId, string text, Guid? receiverId = null, Guid? channelId = null)
        {
            await PostAsync<object>(
                ApiUrl.MessagingMessageSend,
                new SendRequest()
                {
                    Text = text,
                    ReceiverId = receiverId,
                    ChannelId = channelId
                }
            );
        }
        
        public async Task<GetListResponse?> MessagingMessageGetListAsync(Guid channelId, int page)
        {
            return await PostAsync<GetListResponse>(ApiUrl.MessagingMessageGetList, new GetListRequest()
            {
                ChannelId = channelId,
                Page = page
            });
        }
    }
}
