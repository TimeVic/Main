using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<WorkspaceMembershipDto> WorkspaceMembershipAddAsync(long workspaceId, string email)
        {
            var response = await PostAuthorizedAsync<WorkspaceMembershipDto>(
                ApiUrl.WorkspaceMembershipAdd,
                new AddRequest()
                {
                    WorkspaceId = workspaceId,
                    Email = email
                }
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<WorkspaceMembershipDto> WorkspaceMembershipUpdateAsync(
            long membershipId,
            MembershipAccessType access,
            ICollection<long> projectIds
        )
        {
            var response = await PostAuthorizedAsync<WorkspaceMembershipDto>(
                ApiUrl.WorkspaceMembershipUpdate,
                new UpdateRequest()
                {
                    MembershipId = membershipId,
                    Access = access,
                    ProjectIds = projectIds.ToArray()
                }
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> WorkspaceMembershipGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.WorkspaceMembershipList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task WorkspaceMembershipDeleteAsync(long membershipId)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(
                ApiUrl.WorkspaceMembershipDelete,
                new DeleteRequest()
                {
                    MembershipId = membershipId
                }
            );
            if (response == null)
            {
                throw new ServerErrorException();
            }
        }
    }
}
