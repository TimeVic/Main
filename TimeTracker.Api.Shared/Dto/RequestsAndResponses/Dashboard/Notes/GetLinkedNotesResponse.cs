using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetLinkedNotesResponse : IResponse
{
    public ICollection<NoteTreeNodeDto> Notes { get; set; } = new List<NoteTreeNodeDto>();
}
