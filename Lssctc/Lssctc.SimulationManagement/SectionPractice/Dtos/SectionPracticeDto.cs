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
        public bool? IsDeleted { get; set; }  
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
        public bool? IsDeleted { get; set; }
    }
}
