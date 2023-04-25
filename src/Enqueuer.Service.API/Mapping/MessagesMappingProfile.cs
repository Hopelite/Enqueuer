using AutoMapper;

namespace Enqueuer.Service.API.Mapping;

public class MessagesMappingProfile : Profile
{
    public MessagesMappingProfile()
    {
        CreateMap<Messages.Models.Group, Persistence.Models.Group>();
        CreateMap<Persistence.Models.Group, Messages.Models.Group>();
    }
}
