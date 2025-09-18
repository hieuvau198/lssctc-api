using AutoMapper;
using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Entities;


namespace Lssctc.ProgramManagement.Classes.Mappings
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {
            // Create Class
            CreateMap<ClassCreateDto, Class>()
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore()); 

            // Class <-> ClassDto
            CreateMap<Class, ClassDto>()
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src =>
                    src.ClassCode != null
                        ? new ClassCodeDto { Id = src.ClassCode.Id, Name = src.ClassCode.Name }
                        : null))
                .ForMember(dest => dest.Instructors, opt => opt.MapFrom(src => src.ClassInstructors));
            CreateMap<ClassMember, ClassMemberDto>().ReverseMap();
            CreateMap<Trainee, TraineeDto>().ReverseMap();
            // ClassCode mapping
            CreateMap<ClassCode, ClassCodeDto>().ReverseMap();

            // ClassInstructor mapping
            CreateMap<ClassInstructor, ClassInstructorDto>().ReverseMap();

            // Instructor mapping
            CreateMap<Instructor, InstructorDto>().ReverseMap();

            //ClassEnrollment mapping
            CreateMap<ClassEnrollmentCreateDto, ClassEnrollment>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => 0)); 

            CreateMap<ClassEnrollment, ClassEnrollmentDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name))
                .ForMember(dest => dest.TraineeCode, opt => opt.MapFrom(src => src.Trainee.TraineeCode));
        }
    }
}
