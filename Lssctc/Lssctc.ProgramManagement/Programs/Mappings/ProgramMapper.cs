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
                .ForMember(dest => dest.Prerequisites, opt => opt.MapFrom(src => src.ProgramPrerequisites));

            CreateMap<ProgramCourse, ProgramCourseDto>();
            CreateMap<ProgramPrerequisite, ProgramPrerequisiteDto>();

            // DTO → TrainingProgram
            CreateMap<CreateProgramDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramPrerequisites, opt => opt.Ignore());

            CreateMap<UpdateProgramDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramPrerequisites, opt => opt.Ignore());

            CreateMap<CreateProgramPrerequisiteDto, ProgramPrerequisite>();
            CreateMap<UpdateProgramPrerequisiteDto, ProgramPrerequisite>();
        }
    }
}
