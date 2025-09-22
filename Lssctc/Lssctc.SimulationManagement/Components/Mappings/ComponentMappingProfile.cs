using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.SimulationManagement.Components.Dtos;

namespace Lssctc.SimulationManagement.Components.Mappings
{
    public class ComponentMappingProfile : Profile
    {
        public ComponentMappingProfile()
        {
            CreateMap<SimulationComponent, SimulationComponentDto>();

            CreateMap<CreateSimulationComponentDto, SimulationComponent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PracticeStepComponents, opt => opt.Ignore());

            CreateMap<UpdateSimulationComponentDto, SimulationComponent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.PracticeStepComponents, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
