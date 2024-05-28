using AutoMapper;
using User.API.Models;
using User.DB.Entities;

namespace User.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<DbUser, DtoUser>().ForMember(dest => dest.GamesList, opt => opt.MapFrom(src => src.UserGames.Select(g => g.GameId).ToList()));
            CreateMap<DtoUser, DbUser>();
            CreateMap<DbUserGame, DtoUserGame>().ReverseMap();
        }
    }
}
