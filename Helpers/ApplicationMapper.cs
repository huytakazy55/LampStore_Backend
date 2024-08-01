using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;

public class ApplicationMapper : Profile
{
    public ApplicationMapper()
    {
        CreateMap<Lamp, LampModel>().ReverseMap();
    }
}