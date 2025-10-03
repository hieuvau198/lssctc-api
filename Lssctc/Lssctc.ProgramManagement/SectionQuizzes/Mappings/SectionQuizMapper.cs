using AutoMapper;
using Lssctc.ProgramManagement.SectionQuizzes.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.SectionQuizzes.Mappings
{
    public class SectionQuizMapper : Profile
    {
        public SectionQuizMapper()
        {
            CreateMap<Entities.SectionQuiz, SectionQuizDto>();

            CreateMap<CreateSectionQuizDto, Entities.SectionQuiz>();

            CreateMap<UpdateSectionQuizDto, Entities.SectionQuiz>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
