using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class MoveNoteNodeRequest : IRequest<NoteTreeNodeDto>
{
    [RequiredNonEmpty]
    public Guid NoteId { get; set; }

    public Guid? ParentId { get; set; }

    public int? SortOrder { get; set; }
}
