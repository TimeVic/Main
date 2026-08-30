using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<WorkspaceMemberDto?> WorkspaceMemberAddAsync(Guid workspaceId, string email, MembershipAccessType access = MembershipAccessType.User)
        {
            return await PostAsync<WorkspaceMemberDto>(
                ApiUrl.WorkspaceMemberAdd,
                new AddRequest()
                {
                    Email = email,
                    Access = access
                }
            );
        }
        
        public async Task<WorkspaceMemberDto?> WorkspaceMemberUpdateAsync(UpdateRequest request)
        {
            return await PostAsync<WorkspaceMemberDto>(
                ApiUrl.WorkspaceMemberUpdate,
                request
            );
        }
        
        public async Task<GetListResponse?> WorkspaceMemberGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse>(ApiUrl.WorkspaceMemberList, model);
        }
        
        public async Task WorkspaceMemberDeleteAsync(Guid memberId)
        {
            await PostAsync<object>(
                ApiUrl.WorkspaceMemberDelete,
                new DeleteRequest()
                {
                    MemberId = memberId
                }
            );
        }
    }
}
