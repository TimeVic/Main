using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<TagEntity, TagDto>()
            .ForMember(
                dto => dto.Color,
                builder => builder.MapFrom(
                    entity => entity.Color.ToHexString()
                )
            );
        CreateMap<UpdateRequest, TagEntity>()
            .ForMember(
                dto => dto.Color,
                builder => builder.MapFrom(
                    entity => entity.Color
                )
            );
    }
}
