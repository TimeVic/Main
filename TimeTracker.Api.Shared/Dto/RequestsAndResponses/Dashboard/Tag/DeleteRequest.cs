using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag
{
    public class DeleteRequest : IRequest
    {
        [Required]
        [IsPositive]
        public long TagId { get; set; }
    }
}
