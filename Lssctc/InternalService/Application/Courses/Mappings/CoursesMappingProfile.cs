using AutoMapper;
using InternalService.Application.Courses.Dtos;
using InternalService.Domain.Entities;

namespace InternalService.Application.Courses.Mappings;

public class CoursesMappingProfile : Profile
{
    public CoursesMappingProfile()
    {
        CreateMap<Course, CourseDto>();
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
