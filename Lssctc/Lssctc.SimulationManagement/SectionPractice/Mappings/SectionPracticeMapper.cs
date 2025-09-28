using AutoMapper;
using Lssctc.SimulationManagement.SectionPractice.Dtos;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.SimulationManagement.SectionPractice.Mappings
{
    public class SectionPracticeMapper : Profile
    {
        public SectionPracticeMapper()
        {
            CreateMap<Entities.SectionPractice, SectionPracticeDto>();

            CreateMap<CreateSectionPracticeDto, Entities.SectionPractice>();

            // Chỉ map các field != null khi update
            CreateMap<UpdateSectionPracticeDto, Entities.SectionPractice>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
