using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Notes;

namespace TimeTracker.Api.Profiles.Api.Notes;

public class NoteProfile : Profile
{
    public NoteProfile()
    {
        CreateMap<NoteNodeEntity, NoteTreeNodeDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new NoteTreeNodeDto
            {
                Id = src.Id,
                ParentId = src.Parent?.Id,
                Type = src.Type,
                Title = src.Title,
                Visibility = src.Visibility,
                SortOrder = src.SortOrder,
                UpdatedAt = src.UpdatedAt
            });

        CreateMap<NoteNodeEntity, NoteDocumentDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new NoteDocumentDto
            {
                Id = src.Id,
                ParentId = src.Parent?.Id,
                Title = src.Title,
                MarkdownContent = src.MarkdownContent ?? string.Empty,
                Visibility = src.Visibility,
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt,
                Links = mapper.Mapper.Map<ICollection<NoteLinkDto>>(src.Links.ToList())
            });

        CreateMap<NoteLinkEntity, NoteLinkDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new NoteLinkDto
            {
                Id = src.Id,
                EntityType = src.EntityType,
                EntityId = src.EntityId
            });

        CreateMap<NoteNodeHistoryEntity, NoteNodeHistoryDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new NoteNodeHistoryDto
            {
                Id = src.Id,
                NoteId = src.NoteNode.Id,
                Title = src.Title,
                MarkdownContent = src.MarkdownContent ?? string.Empty,
                SortOrder = src.SortOrder,
                CreatedAt = src.CreatedAt
            });
    }
}
