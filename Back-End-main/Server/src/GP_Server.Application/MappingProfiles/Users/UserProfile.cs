using System;
using AutoMapper;
using GP_Server.Application.DTOs.Users;
using GP_Server.Domain.Entities;

namespace GP_Server.Application.MappingProfiles.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserDTO ,ApplicationUser>().ReverseMap();
        CreateMap<UserDTO, ApplicationUser>().ReverseMap();
        CreateMap<ApplicationUser, GeneralUserDTO>().ReverseMap();
        CreateMap<ApplicationUser, DetailedUserDTO>()
        .ForMember(dest => dest.Roles, opt => opt.Ignore())
        .ForMember(dest => dest.ProfilePicturePath, opt => opt.MapFrom(src => src.ProfilePicturePath))
        .ReverseMap();
    }

}
