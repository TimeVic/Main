using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List
{
    public class ArchiveTaskListRequest : IRequest
    {
        [Required]
        [IsPositive]
        public long TaskListId { get; set; }
    }
}
