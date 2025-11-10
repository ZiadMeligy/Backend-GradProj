using AutoMapper;
using GP_Server.Application.DTOs.Roles;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.MappingProfiles.Roles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role,RoleDTO>().ReverseMap();
        CreateMap<Role,CreateRoleDTO>()
        .ForMember(dest => dest.Users, opt => opt.Ignore())
        .ReverseMap();
        CreateMap<Role,GeneralRoleDTO>().ReverseMap();
        CreateMap<Role,DetailedRoleDTO>()
        .ForMember(dest => dest.Users, opt => opt.Ignore());
        CreateMap<string, RoleDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src)); 
    }

}
