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
        public Guid TagId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        [IsColor]
        public string? Color { get; set; }
        
        public void Fill(TagDto tag)
        {
            TagId = tag.Id;
            Name = tag.Name;
            Color = tag.Color;
        }
    }
}
