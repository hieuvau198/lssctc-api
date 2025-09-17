using AutoMapper;
using LearnerService.Application.Dtos;
using LearnerService.Domain.Entities;

namespace LearnerService.Application.Mappings;

public class LearnersMappingProfile : Profile
{
    public LearnersMappingProfile()
    {
        CreateMap<Learner, LearnerDto>();
        CreateMap<CreateLearnerDto, Learner>();
        CreateMap<UpdateLearnerDto, Learner>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}