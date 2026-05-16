using System.Drawing;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<TagEntity, TagDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new TagDto
            {
                Id = src.Id,
                Name = src.Name,
                Color = src.Color?.ToHexString(),
                TextColor = src.Color?.GetTextColorBasedOn().ToHexString() ?? string.Empty
            });
        CreateMap<UpdateRequest, TagEntity>()
            .ForMember(
                dto => dto.Color,
                builder => builder.MapFrom(
                    entity => ColorTranslator.FromHtml(entity.Color!)
                )
            );
    }
}
