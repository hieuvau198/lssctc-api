using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.SimulationManagement.Practices.Dtos;

namespace Lssctc.SimulationManagement.Practices.Mappings
{
    public class PracticeMappingProfile : Profile
    {
        public PracticeMappingProfile()
        {
            CreateMap<Practice, PracticeDto>();

            CreateMap<CreatePracticeDto, Practice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PracticeSteps, opt => opt.Ignore())
                .ForMember(dest => dest.SectionPractices, opt => opt.Ignore());

            CreateMap<UpdatePracticeDto, Practice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PracticeSteps, opt => opt.Ignore())
                .ForMember(dest => dest.SectionPractices, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        

        }
    }
}
