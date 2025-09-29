using AutoMapper;
using Lssctc.Share.Entities;
using Lssctc.SimulationManagement.PracticeSteps.Dtos;

namespace Lssctc.SimulationManagement.PracticeSteps.Mappings
{
    public class PracticeStepMappingProfile : Profile
    {
        public PracticeStepMappingProfile()
        {
            CreateMap<PracticeStep, PracticeStepDto>();
            CreateMap<CreatePracticeStepDto, PracticeStep>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdatePracticeStepDto, PracticeStep>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PracticeId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
