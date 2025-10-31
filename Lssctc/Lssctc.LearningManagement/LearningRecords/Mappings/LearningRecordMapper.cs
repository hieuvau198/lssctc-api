using AutoMapper;
using Lssctc.LearningManagement.LearningRecords.DTOs;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.LearningManagement.LearningRecords.Mappings
{
    public class LearningRecordMapper : Profile
    {
        public LearningRecordMapper()
        {
            // Entity -> DTO (dùng cho Get/Paged)
            CreateMap<Entities.LearningRecord, LearningRecordDto>();

            CreateMap<CreateLearningRecordDto, Entities.LearningRecord>();


            CreateMap<UpdateLearningRecordDto, Entities.LearningRecord>();
               
        }
    }
}
