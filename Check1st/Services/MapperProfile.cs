using AutoMapper;
using Check1st.Models;

namespace Check1st.Services;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<AssignmentInputModel, Assignment>()
                .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) =>
                    dest.IsPublished ? dest.TimePublished : src.TimePublished?.ToUniversalTime())) // skip if already published
                .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToUniversalTime()));
        CreateMap<Assignment, AssignmentInputModel>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) => src.TimePublished?.ToLocalTime()))
            .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToLocalTime()));
    }
}
