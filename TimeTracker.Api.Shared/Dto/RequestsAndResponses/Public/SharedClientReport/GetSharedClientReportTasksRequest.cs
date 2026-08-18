using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.SharedClientReport;

public class GetSharedClientReportTasksRequest : IRequest<GetSharedClientReportTasksResponse>
{
    public string Token { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    [Required]
    [IsPositive]
    public int Page { get; set; } = 1;
}
