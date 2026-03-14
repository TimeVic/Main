using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentEntity, PaymentDto>();
    }
}
