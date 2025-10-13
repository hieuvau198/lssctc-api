using System.ComponentModel.DataAnnotations;

namespace Lssctc.SimulationManagement.SectionPractice.Dtos
{
    public class SectionPracticeDto
    {
        public int Id { get; set; }
        public int SectionPartitionId { get; set; }
        public int PracticeId { get; set; }
        public DateTime? CustomDeadline { get; set; }
        public string? CustomDescription { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CreateSectionPracticeDto
    {
        [Range(1, int.MaxValue)]
        public int SectionPartitionId { get; set; }

        [Range(1, int.MaxValue)]
        public int PracticeId { get; set; }
        public DateTime? CustomDeadline { get; set; }

        [MaxLength(2000)]
        public string? CustomDescription { get; set; }

        public int? Status { get; set; }      
        public bool? IsActive { get; set; }   
    }

    public class UpdateSectionPracticeDto
    {
        [Range(1, int.MaxValue)]
        public int? SectionPartitionId { get; set; }

        [Range(1, int.MaxValue)]
        public int? PracticeId { get; set; }

        public DateTime? CustomDeadline { get; set; }

        [MaxLength(2000)]
        public string? CustomDescription { get; set; }

        public int? Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SectionPracticeListDto
    {
        public int Id { get; set; }                    // section_practices.id
        public int SectionId { get; set; }             // sections.id
        public string SectionName { get; set; } = "";  // sections.name
        public int SectionOrder { get; set; }          // sections.[order]

        public int SectionPartitionId { get; set; }    // section_partitions.id
        public string? PartitionName { get; set; }     // section_partitions.name
        public int PartitionTypeId { get; set; }       // section_partitions.partition_type_id

        public int PracticeId { get; set; }            // practices.id
        public string PracticeName { get; set; } = ""; // practices.practice_name

        public DateTime? CustomDeadline { get; set; }  // section_practices.custom_deadline
        public string? CustomDescription { get; set; } // section_practices.custom_description
        public int Status { get; set; }                // section_practices.status
        public bool IsActive { get; set; }             // section_practices.is_active
        public bool IsDeleted { get; set; }            // section_practices.is_deleted
    }
}
