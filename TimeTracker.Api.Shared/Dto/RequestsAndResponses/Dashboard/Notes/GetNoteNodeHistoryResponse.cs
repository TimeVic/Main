using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Notes;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;

public class GetNoteNodeHistoryResponse : IResponse
{
    public ICollection<NoteNodeHistoryDto> History { get; set; } = new List<NoteNodeHistoryDto>();
}
