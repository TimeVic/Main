using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Constants.Notes;

namespace TimeTracker.Api.Shared.Dto.Entity.Notes;

public class NoteTreeNodeDto : IResponse
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public NoteNodeType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid? LastContentId { get; set; }

    public NoteVisibility Visibility { get; set; }

    public int SortOrder { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
