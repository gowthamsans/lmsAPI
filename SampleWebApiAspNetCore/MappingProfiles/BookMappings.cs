using AutoMapper;
using SampleWebApiAspNetCore.Dtos;
using SampleWebApiAspNetCore.Entities;

namespace SampleWebApiAspNetCore.MappingProfiles
{
    public class BookMappings : Profile
    {
        public BookMappings()
        {
            CreateMap<BookEntity, BookDto>().ReverseMap();
            CreateMap<BookEntity, BookUpdateDto>().ReverseMap();
            CreateMap<BookEntity, BookCreateDto>().ReverseMap();
        }
    }
}
