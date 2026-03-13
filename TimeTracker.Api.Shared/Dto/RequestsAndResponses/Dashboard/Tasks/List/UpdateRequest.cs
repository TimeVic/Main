using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List
{
    public class UpdateRequest : AddRequest
    {
        [Required]
        public Guid TaskListId { get; set; }
    }
}
