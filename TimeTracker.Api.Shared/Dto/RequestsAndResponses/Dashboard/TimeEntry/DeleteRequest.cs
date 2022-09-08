using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry
{
    public class DeleteRequest : IRequest
    {
        [Required]
        [IsPositive]
        public long TimeEntryId { get; set; }
    }
}
