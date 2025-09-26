using AutoMapper;
using Lssctc.ProgramManagement.Programs.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Programs.Mappings
{
    public class ProgramMapper : Profile
    {
        public ProgramMapper()
        {
            // --- Entity → DTO ---
            CreateMap<TrainingProgram, ProgramDto>()
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.ProgramCourses))
                .ForMember(dest => dest.EntryRequirements, opt => opt.MapFrom(src => src.ProgramEntryRequirements));

            CreateMap<ProgramCourse, ProgramCourseDto>();
            CreateMap<ProgramEntryRequirement, EntryRequirementDto>();

            // --- DTO → Entity ---
            // Create Program
            CreateMap<CreateProgramDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramEntryRequirements, opt => opt.Ignore());

            // Update Program Info (does not touch collections)
            CreateMap<UpdateProgramInfoDto, TrainingProgram>()
                .ForMember(dest => dest.ProgramCourses, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramEntryRequirements, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Update Program Courses (only used in service, not direct mapping to TrainingProgram)
            CreateMap<ProgramCourseOrderDto, ProgramCourse>()
                .ForMember(dest => dest.CoursesId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.CourseOrder, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.ProgramId, opt => opt.Ignore()) // set in service
                .ForMember(dest => dest.Name, opt => opt.Ignore())      // loaded from Course
                .ForMember(dest => dest.Description, opt => opt.Ignore());

            // Update Program Entry Requirements
            CreateMap<CreateProgramPrerequisiteDto, ProgramEntryRequirement>();
            CreateMap<UpdateEntryRequirementDto, ProgramEntryRequirement>()
                .ForMember(dest => dest.ProgramId, opt => opt.Ignore()); // set in service
        }
    }
}
