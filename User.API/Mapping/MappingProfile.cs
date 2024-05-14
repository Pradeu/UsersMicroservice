using AutoMapper;
using User.API.Models;
using User.DB.Entities;

namespace User.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<DbUser, DtoUser>().ReverseMap();
        }
    }
}
