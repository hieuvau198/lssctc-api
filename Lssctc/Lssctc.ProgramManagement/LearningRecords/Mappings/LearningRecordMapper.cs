using AutoMapper;
using Lssctc.ProgramManagement.LearningRecords.DTOs;
using Entities = Lssctc.Share.Entities;
namespace Lssctc.ProgramManagement.LearningRecords.Mappings
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
