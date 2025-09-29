using AutoMapper;
using Lssctc.LearningManagement.SectionPartition.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.LearningManagement.SectionPartition.Mappings
{
    public class SectionPartitionMapper : Profile
    {
        public SectionPartitionMapper()
        {
            CreateMap<Entities.SectionPartition, SectionPartitionDto>();
            CreateMap<CreateSectionPartitionDto, Entities.SectionPartition>();
            CreateMap<UpdateSectionPartitionDto, Entities.SectionPartition>();
        }

        
    }
}
