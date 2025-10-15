using AutoMapper;
using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;
using Lssctc.ProgramManagement.QuizQuestions.DTOs;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Quizzes.Mappings
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

            //== QuizDetail
            CreateMap<Quiz, QuizDetailDto>()
           .ForMember(d => d.Questions,
               o => o.MapFrom(s => s.QuizQuestions.OrderBy(q => q.Id)));


            // QuizQuestion
            CreateMap<QuizQuestion, QuizQuestionDto>();
            CreateMap<CreateQuizQuestionDto, QuizQuestion>();
            CreateMap<UpdateQuizQuestionDto, QuizQuestion>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));

            //== QuizDetailQuestion
            CreateMap<QuizQuestion, QuizDetailQuestionDto>()
            .ForMember(d => d.Options,
                o => o.MapFrom(s => s.QuizQuestionOptions
                    .OrderBy(opt => opt.DisplayOrder)));



            // QuizQuestionOption

            CreateMap<QuizQuestionOption, QuizQuestionOptionDto>();
            CreateMap<CreateQuizQuestionOptionDto, QuizQuestionOption>()
                .ForMember(d => d.DisplayOrder, o => o.Condition(s => s.DisplayOrder.HasValue));
            CreateMap<UpdateQuizQuestionOptionDto, QuizQuestionOption>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));


            CreateMap<Quiz, QuizDetailDto>()
    .ForMember(d => d.Questions,
        o => o.MapFrom(s => s.QuizQuestions.OrderBy(q => q.Id)));

            CreateMap<QuizQuestion, QuizDetailQuestionDto>()
                .ForMember(d => d.Options,
                    o => o.MapFrom(s => s.QuizQuestionOptions.OrderBy(opt => opt.DisplayOrder)));

            CreateMap<QuizQuestionOption, QuizDetailQuestionOptionDto>();



            //== Quiz option detail

            CreateMap<QuizQuestionOption, QuizDetailQuestionOptionDto>();


            //==== quiz trainee detail
            // Quiz -> QuizTraineeDetailDto
            CreateMap<Quiz, QuizTraineeDetailDto>()
                .ForMember(d => d.Questions,
                    o => o.MapFrom(s => s.QuizQuestions.OrderBy(q => q.Id)));

            // QuizQuestion -> QuizTraineeQuestionDto
            CreateMap<QuizQuestion, QuizTraineeQuestionDto>()
                .ForMember(d => d.Options,
                    o => o.MapFrom(s => s.QuizQuestionOptions.OrderBy(opt => opt.DisplayOrder)));

            // QuizQuestionOption -> QuizTraineeQuestionOptionDto (không map IsCorrect)
            CreateMap<QuizQuestionOption, QuizTraineeQuestionOptionDto>();


            //====

        }


    }
}
