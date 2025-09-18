using AutoMapper;
using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Courses.Mappings
{
    public class CourseMapper: Profile
    {
        public CourseMapper()
        {
            // Entity → DTO
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.Level != null ? src.Level.Name : null));
            //CourseSyllabus
            CreateMap<CourseSyllabuse, CourseSyllabusDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Syllabus.Name));

            CreateMap<CourseSyllabusCreateDto, CourseSyllabuse>();
            // DTO → Entity
            CreateMap<CreateCourseDto, Course>();
            CreateMap<UpdateCourseDto, Course>();
        }
    }
}
