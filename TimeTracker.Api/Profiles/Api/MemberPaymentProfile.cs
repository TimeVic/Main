using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class MemberPaymentProfile : Profile
{
    public MemberPaymentProfile()
    {
        CreateMap<MemberPaymentEntity, MemberPaymentDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new MemberPaymentDto
            {
                Id = src.Id,
                PaymentTime = src.PaymentTime,
                Description = src.Description,
                Amount = src.Amount,
                Project = mapper.Mapper.Map<ProjectDto>(src.Project),
                Client = src.Project?.Client != null ? mapper.Mapper.Map<ClientDto>(src.Project.Client) : null,
                Member = mapper.Mapper.Map<WorkspaceMemberDto>(src.Member)
            });
    }
}
