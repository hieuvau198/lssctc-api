using AutoMapper;
using Lssctc.ProgramManagement.Sections.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Sections.Mappings
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
