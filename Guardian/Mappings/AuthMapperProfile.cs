using AutoMapper;
using Guardian.Models.Auth.v1;
using Microsoft.AspNetCore.Identity;

namespace Guardian.Mappings;

public class AuthMapperProfile : Profile
{
    public AuthMapperProfile()
    {
        CreateMap<IdentityUser, UserDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));
    }
}
