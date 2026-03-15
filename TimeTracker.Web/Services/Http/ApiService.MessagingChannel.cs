using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<MessagingChannelDto?> MessagingChannelCreateAsync(Guid workspaceId, string slug)
        {
            return await PostAsync<MessagingChannelDto>(
                ApiUrl.MessagingChannelCreate,
                new CreateRequest()
                {
                    WorkspaceId = workspaceId,
                    Slug = slug
                }
            );
        }
        
        public async Task<MessagingChannelDto?> MessagingChannelInitAsync(Guid workspaceId)
        {
            return await PostAsync<MessagingChannelDto>(
                ApiUrl.MessagingChannelInit,
                new InitRequest()
                {
                    WorkspaceId = workspaceId
                }
            );
        }
        
        public async Task<GetListResponse?> MessagingChannelGetListAsync(Guid workspaceId)
        {
            return await PostAsync<GetListResponse>(ApiUrl.MessagingChannelGetList, new GetListRequest() { WorkspaceId = workspaceId});
        }
    }
}
