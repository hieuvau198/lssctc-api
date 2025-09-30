using AutoMapper;
using Lssctc.LearningManagement.SectionMaterial.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.LearningManagement.SectionMaterial.Mappings
{
    public class SectionMaterialMapper : Profile
    {
        public SectionMaterialMapper()
        {
            // Entity -> DTO
            CreateMap<Entities.SectionMaterial, SectionMaterialDto>();

            // Create DTO -> Entity
            CreateMap<CreateSectionMaterialDto, Entities.SectionMaterial>()
                // nếu Description null thì map thành chuỗi rỗng để tránh lỗi NOT NULL trong DB
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description ?? ""))
               
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));

            // Update DTO -> Entity (bỏ qua các field null)
            CreateMap<UpdateSectionMaterialDto, Entities.SectionMaterial>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
