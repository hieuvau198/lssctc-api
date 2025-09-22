using AutoMapper;
using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Entities;
using Lssctc.Share.Enum;

namespace Lssctc.ProgramManagement.Classes.Mappings
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {
            // Class
            CreateMap<Class, ClassDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ClassDto, Class>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<ClassStatus>(src.Status, true)));

            CreateMap<ClassCreateDto, Class>()
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore())
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<ClassStatus>(src.Status, true)));

            // ClassMember
            CreateMap<ClassMember, ClassMemberDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<ClassStatus>(src.Status, true)));

            CreateMap<Trainee, TraineeDto>().ReverseMap();
            CreateMap<ClassCode, ClassCodeDto>().ReverseMap();
            CreateMap<ClassInstructor, ClassInstructorDto>().ReverseMap();
            CreateMap<Instructor, InstructorDto>().ReverseMap();

            // ClassEnrollment
            CreateMap<ClassEnrollmentCreateDto, ClassEnrollment>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ClassEnrollmentStatus.Pending));

            CreateMap<ClassEnrollment, ClassEnrollmentDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.Name))
                .ForMember(dest => dest.TraineeCode, opt => opt.MapFrom(src => src.Trainee.TraineeCode));

            // TrainingProgress
            CreateMap<TrainingProgress, TrainingProgressDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateTrainingProgressDto, TrainingProgress>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<TrainingProgressStatus>(src.Status, true)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateTrainingProgressDto, TrainingProgress>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<TrainingProgressStatus>(src.Status, true)))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // TrainingResult
            CreateMap<TrainingResult, TrainingResultDto>();
            CreateMap<CreateTrainingResultDto, TrainingResult>();
            CreateMap<UpdateTrainingResultDto, TrainingResult>();

            // TrainingResultType
            CreateMap<TrainingResultType, TrainingResultTypeDto>();
        }
    }
}
