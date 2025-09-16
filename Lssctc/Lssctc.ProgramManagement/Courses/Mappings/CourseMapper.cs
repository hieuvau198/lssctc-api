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

            // DTO → Entity
            CreateMap<CreateCourseDto, Course>();
            CreateMap<UpdateCourseDto, Course>();
        }
    }
}
