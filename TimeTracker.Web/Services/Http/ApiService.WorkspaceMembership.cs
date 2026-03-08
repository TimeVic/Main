using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<WorkspaceMembershipDto?> WorkspaceMembershipAddAsync(Guid workspaceId, string email)
        {
            return await PostAsync<WorkspaceMembershipDto>(
                ApiUrl.WorkspaceMembershipAdd,
                new AddRequest()
                {
                    WorkspaceId = workspaceId,
                    Email = email
                }
            );
        }
        
        public async Task<WorkspaceMembershipDto?> WorkspaceMembershipUpdateAsync(UpdateRequest request)
        {
            return await PostAsync<WorkspaceMembershipDto>(
                ApiUrl.WorkspaceMembershipUpdate,
                request
            );
        }
        
        public async Task<GetListResponse?> WorkspaceMembershipGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse>(ApiUrl.WorkspaceMembershipList, model);
        }
        
        public async Task WorkspaceMembershipDeleteAsync(Guid membershipId)
        {
            await PostAsync<object>(
                ApiUrl.WorkspaceMembershipDelete,
                new DeleteRequest()
                {
                    MembershipId = membershipId
                }
            );
        }
    }
}
