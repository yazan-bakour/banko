using AutoMapper;
using Banko.Models;
using Banko.Models.DTOs;

namespace Banko.Mapping;

public class UserMappingProfile : Profile
{
  public UserMappingProfile()
  {
    CreateMap<User, UserSettingsDto>()
    // Only need to specify mappings for non-matching properties or those needing transformation
    .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.PasswordHash))
  ;
  }
}