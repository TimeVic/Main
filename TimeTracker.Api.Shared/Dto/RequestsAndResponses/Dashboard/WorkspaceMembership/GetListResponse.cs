using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;

public class GetListResponse: PaginatedListDto<WorkspaceMembershipDto>
{
    public GetListResponse(
        ICollection<WorkspaceMembershipDto> responseList,
        int totalItems
    ) : base(responseList, totalItems)
    {
    }
}
