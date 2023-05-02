using AutoMapper;

namespace Enqueuer.Service.API.Mapping;

public class MessagesMappingProfile : Profile
{
    public MessagesMappingProfile()
    {
        CreateMap<Messages.Models.User, Persistence.Models.User>();
        CreateMap<Persistence.Models.User, Messages.Models.User>();
        CreateMap<Persistence.Models.User, Messages.Models.UserInfo>()
            .ForMember(i => i.Groups, opts => opts.MapFrom(u => u.ParticipatesIn));

        CreateMap<Messages.Models.Group, Persistence.Models.Group>();
        CreateMap<Persistence.Models.Group, Messages.Models.Group>();

        CreateMap<Messages.Models.Queue, Persistence.Models.Queue>();
        CreateMap<Persistence.Models.Queue, Messages.Models.Queue>();
        CreateMap<Persistence.Models.Queue, Messages.Models.QueueInfo>();

        CreateMap<Persistence.Models.QueueMember, Messages.Models.QueueMember>();
    }
}
