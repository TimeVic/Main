using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        CreateMap<ClientEntity, ClientDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                return new ClientDto
                {
                    Id = src.Id,
                    Name = src.Name
                };
            });
    }
}
