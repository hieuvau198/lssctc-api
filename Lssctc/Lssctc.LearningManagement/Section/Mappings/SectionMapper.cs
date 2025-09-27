using AutoMapper;
using Lssctc.LearningManagement.Section.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.LearningManagement.Section.Mappings
{
    public class SectionMapper : Profile
    {
        public SectionMapper()
        {
            CreateMap<Entities.Section, SectionDto>();
            CreateMap<Entities.Section, SectionListItemDto>();

            CreateMap<CreateSectionDto, Entities.Section>()
                .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate ?? DateTime.UtcNow));

            CreateMap<UpdateSectionDto, Entities.Section>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
