using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag
{
    public class UpdateRequest : IRequest<TagDto>
    {
        [Required]
        [IsPositive]
        public long TagId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; }
        
        [IsColor]
        public string? Color { get; set; }
    }
}
