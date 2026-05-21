using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNotesTreeResponse : IResponse
{
    public ICollection<NoteTreeNodeDto> Nodes { get; set; } = new List<NoteTreeNodeDto>();
}
