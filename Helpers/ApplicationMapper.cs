using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;

public class ApplicationMapper : Profile
{
    public ApplicationMapper()
    {
        CreateMap<LampModel, Lamp>().ReverseMap();
        CreateMap<LampImageModel, LampImage>().ReverseMap();
        CreateMap<CategoryModel, Category>().ReverseMap();
    }
}