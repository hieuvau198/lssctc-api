using AutoMapper;
using Lssctc.SimulationManagement.PracticeStepComponent.Dtos;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.SimulationManagement.PracticeStepComponent.Mappings
{
    public class PracticeStepComponentMapper : Profile
    {
        public PracticeStepComponentMapper()
        {
            CreateMap<Entities.PracticeStepComponent, PracticeStepComponentDto>();

            CreateMap<CreatePracticeStepComponentDto, Entities.PracticeStepComponent>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ComponentOrder, o => o.MapFrom(s => s.ComponentOrder ?? 0));

            CreateMap<UpdatePracticeStepComponentDto, Entities.PracticeStepComponent>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
