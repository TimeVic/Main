using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class ClientPaymentProfile : Profile
{
    public ClientPaymentProfile()
    {
        CreateMap<ClientPaymentEntity, ClientPaymentDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new ClientPaymentDto
            {
                Id = src.Id,
                PaymentTime = src.PaymentTime,
                Description = src.Description,
                Amount = src.Amount,
                Project = src.Project != null ? mapper.Mapper.Map<ProjectDto>(src.Project) : null,
                Client = mapper.Mapper.Map<ClientDto>(src.Client)
            });
    }
}
