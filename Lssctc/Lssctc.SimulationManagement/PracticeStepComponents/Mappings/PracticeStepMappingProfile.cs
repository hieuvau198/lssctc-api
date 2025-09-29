using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.SimulationManagement.PracticeStepComponents.Dtos;

namespace Lssctc.SimulationManagement.PracticeStepComponents.Mappings
{
    public class PracticeStepMappingProfile : Profile
    {
        public PracticeStepMappingProfile()
        {
            CreateMap<PracticeStepComponent, PracticeStepComponentDto>()
                .ForMember(dest => dest.PracticeStepId, opt => opt.MapFrom(src => src.StepId))
                .ForMember(dest => dest.SimulationComponentId, opt => opt.MapFrom(src => src.ComponentId))
                .ForMember(dest => dest.SimulationComponentName, opt => opt.MapFrom(src => src.Component.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Component.Description))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Component.ImageUrl));

            CreateMap<CreatePracticeStepComponentDto, PracticeStepComponent>()
                .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => src.PracticeStepId))
                .ForMember(dest => dest.ComponentId, opt => opt.MapFrom(src => src.SimulationComponentId))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdatePracticeStepComponentDto, PracticeStepComponent>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
