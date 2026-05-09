using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class MemberPaymentProfile : Profile
{
    public MemberPaymentProfile()
    {
        CreateMap<MemberPaymentEntity, MemberPaymentDto>()
            .ForMember(dto => dto.Client, options => options.MapFrom(entity => entity.Project.Client));
    }
}
