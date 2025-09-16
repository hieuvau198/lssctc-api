using AutoMapper;
using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.LearningManagement.Quizzes.Mappings
{
    public class QuizMapper : Profile

    {

        public QuizMapper()
        {
            CreateMap<Quiz, QuizDto>();
            CreateMap<CreateQuizDto, Quiz>()
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateQuizDto, Quiz>()
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
