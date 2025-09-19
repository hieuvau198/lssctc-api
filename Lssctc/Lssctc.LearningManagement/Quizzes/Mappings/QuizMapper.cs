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


            // QuizQuestion
            CreateMap<QuizQuestion, QuizQuestionDto>();
            CreateMap<CreateQuizQuestionDto, QuizQuestion>();
            CreateMap<UpdateQuizQuestionDto, QuizQuestion>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));

            // QuizQuestionOption

            CreateMap<QuizQuestionOption, QuizQuestionOptionDto>();
            CreateMap<CreateQuizQuestionOptionDto, QuizQuestionOption>()
                .ForMember(d => d.DisplayOrder, o => o.Condition(s => s.DisplayOrder.HasValue));
            CreateMap<UpdateQuizQuestionOptionDto, QuizQuestionOption>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));



        }


    }
}
