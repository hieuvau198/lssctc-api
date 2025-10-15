using AutoMapper;
using Lssctc.LearningManagement.QuizQuestions.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.LearningManagement.QuizQuestions.Mappings
{
    public class QuizQuestionMapper : Profile
    {
        public QuizQuestionMapper()
        {
            // QuizQuestion -> QuizQuestionDto
            CreateMap<QuizQuestion, QuizQuestionDto>();

            // QuizQuestion -> QuizQuestionNoOptionsDto
            CreateMap<QuizQuestion, QuizQuestionNoOptionsDto>();

            // QuizQuestion -> QuizDetailQuestionDto
            CreateMap<QuizQuestion, QuizDetailQuestionDto>();

            // QuizQuestion -> QuizTraineeQuestionDto
            CreateMap<QuizQuestion, QuizTraineeQuestionDto>();

            // CreateQuizQuestionDto -> QuizQuestion (bây gi? IsMultipleAnswers ???c map t? DTO)
            CreateMap<CreateQuizQuestionDto, QuizQuestion>();

            // UpdateQuizQuestionDto -> QuizQuestion (only map non-null values)
            CreateMap<UpdateQuizQuestionDto, QuizQuestion>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}