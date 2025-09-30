using AutoMapper;
using Lssctc.ProgramManagement.SectionPartitions.DTOs;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.SectionPartitions.Mappings
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
