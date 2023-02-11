using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks
{
    public class UpdateTaskRequest : AddTaskRequest
    {
        [Required]
        [IsPositive]
        public long TaskId { get; set; }
    }
}
