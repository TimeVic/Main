using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class ClientPaymentProfile : Profile
{
    public ClientPaymentProfile()
    {
        CreateMap<ClientPaymentEntity, ClientPaymentDto>();
    }
}
