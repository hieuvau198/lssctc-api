using AutoMapper;
using Lssctc.LearningManagement.LearningMaterials.DTOs;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.LearningManagement.LearningMaterials.Mappings
{
    public class LearningMaterialMapper : Profile
    {
        public LearningMaterialMapper()
        {
            // Entity -> DTO
            CreateMap<Entities.LearningMaterial, LearningMaterialDto>();

            // DTO -> Entity (Create)
            CreateMap<CreateLearningMaterialDto, Entities.LearningMaterial>();

            // DTO -> Entity (Update, chỉ map các field != null)
            CreateMap<UpdateLearningMaterialDto, Entities.LearningMaterial>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
