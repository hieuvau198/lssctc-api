using AutoMapper;
using Lssctc.ProgramManagement.Classes.DTOs;
using Entities = Lssctc.Share.Entities;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;

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
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore());
                

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
            CreateMap<ClassEnrollmentCreateDto, ClassRegistration>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ClassRegistrationStatus.Pending));

            CreateMap<ClassRegistration, ClassEnrollmentDto>()
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
            CreateMap<Entities.TrainingResult, TrainingResultDto>();
            CreateMap<CreateTrainingResultDto, Entities.TrainingResult>();
            CreateMap<UpdateTrainingResultDto, Entities.TrainingResult>();

            // TrainingResultType
            CreateMap<TrainingResultType, TrainingResultTypeDto>();




            //Section

            // Section
            CreateMap<SectionCreateDto, Section>()
                .ForMember(dest => dest.ClassesId, opt => opt.MapFrom(src => src.ClassId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)Share.Enums.SectionStatus.Planned));

            CreateMap<Section, SectionDto>()
                .ForMember(dest => dest.Status,
                            opt => opt.MapFrom(src => ((SectionStatus)src.Status).ToString()));

            // Syllabus + SyllabusSection
            CreateMap<SyllabusSectionCreateDto, Syllabuse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SyllabusName))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.CourseCode));

            CreateMap<SyllabusSectionCreateDto, SyllabusSection>()
                .ForMember(dest => dest.SectionTitle, opt => opt.MapFrom(src => src.SectionTitle))
                .ForMember(dest => dest.SectionDescription, opt => opt.MapFrom(src => src.SectionDescription))
                .ForMember(dest => dest.SectionOrder, opt => opt.MapFrom(src => src.SectionOrder))
                .ForMember(dest => dest.EstimatedDurationMinutes, opt => opt.MapFrom(src => src.EstimatedDurationMinutes));

            CreateMap<SyllabusSection, SyllabusSectionDto>()
                .ForMember(dest => dest.SyllabusName, opt => opt.MapFrom(src => src.Syllabus.Name))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Syllabus.CourseName))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Syllabus.CourseCode));
        }
    }
}
