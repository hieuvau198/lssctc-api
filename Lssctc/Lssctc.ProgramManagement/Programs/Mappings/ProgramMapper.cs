using AutoMapper;
using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Programs.Mappings
{
    public class ProgramMapper : Profile
    {
        public ProgramMapper()
        {
            // TrainingProgram → DTO
            CreateMap<TrainingProgram, ProgramDto>()
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.ProgramCourses))
                .ForMember(dest => dest.Prerequisites, opt => opt.MapFrom(src => src.ProgramEntryRequirements));

            CreateMap<ProgramCourse, ProgramCourseDto>();
            CreateMap<ProgramEntryRequirement, EntryRequirementDto>();

            // DTO → TrainingProgram
            CreateMap<CreateProgramDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramEntryRequirements, opt => opt.Ignore());

            CreateMap<UpdateProgramDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramEntryRequirements, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateProgramPrerequisiteDto, ProgramEntryRequirement>();
            CreateMap<UpdateProgramPrerequisiteDto, ProgramEntryRequirement>();
        }
    }
}
