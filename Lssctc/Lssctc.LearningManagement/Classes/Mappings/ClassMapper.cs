using AutoMapper;
using Entities = Lssctc.Share.Entities;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.LearningManagement.Classes.DTOs;

namespace Lssctc.LearningManagement.Classes.Mappings
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {
            // Class
            CreateMap<Class, ClassDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Instructors,
                            opt => opt.MapFrom(src => src.ClassInstructors))
                .ForMember(dest => dest.Members,
                            opt => opt.MapFrom(src => src.ClassMembers));

            CreateMap<ClassDto, Class>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<ClassStatus>(src.Status, true)))
                .ForMember(dest => dest.ClassInstructors,
                           opt => opt.Ignore()) 
                .ForMember(dest => dest.ClassMembers,
                           opt => opt.Ignore());

            CreateMap<ClassCreateDto, Class>()
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore());

            CreateMap<ClassUpdateDto, Class>()
                .ForMember(dest => dest.ClassCode, opt => opt.Ignore());
            // ClassMember
            CreateMap<ClassMember, ClassMemberDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Trainee,
                       opt => opt.MapFrom(src => src.Trainee))
                .ReverseMap()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<ClassStatus>(src.Status, true)));

            CreateMap<Trainee, TraineeDto>()
                .ForMember(dest => dest.TraineeCode, opt => opt.MapFrom(src => src.TraineeCode))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.IdNavigation.Fullname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.IdNavigation.Email))
                .ReverseMap();


            CreateMap<ClassCode, ClassCodeDto>().ReverseMap();


            CreateMap<ClassInstructor, ClassInstructorDto>()
            .ForMember(dest => dest.Instructor,
                       opt => opt.MapFrom(src => src.Instructor))
            .ReverseMap();

            CreateMap<Instructor, InstructorDto>()
                // From Instructor entity itself
                .ForMember(dest => dest.InstructorCode, opt => opt.MapFrom(src => src.InstructorCode))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate))
                // From InstructorProfile
                .ForMember(dest => dest.ExperienceYears, opt => opt.MapFrom(src => src.InstructorProfile != null ? src.InstructorProfile.ExperienceYears : null))
                .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.InstructorProfile != null ? src.InstructorProfile.Biography : null))
                .ForMember(dest => dest.ProfessionalProfileUrl, opt => opt.MapFrom(src => src.InstructorProfile != null ? src.InstructorProfile.ProfessionalProfileUrl : null))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.InstructorProfile != null ? src.InstructorProfile.Specialization : null))
                // From User (IdNavigation)
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.IdNavigation.Fullname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.IdNavigation.Email))
                .ReverseMap();

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




            

            // Section
            CreateMap<SectionCreateDto, Section>()
                .ForMember(dest => dest.ClassesId, opt => opt.MapFrom(src => src.ClassId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)SectionStatus.Planned));

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
