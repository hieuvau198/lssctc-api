using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Sections.DTOs
{
    public class SectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ClassesId { get; set; }
        public int SyllabusSectionId { get; set; }
        public int? DurationMinutes { get; set; }
        public int Order { get; set; }
        public DateTime StartDate { get; set; }    // UTC or local? giữ nguyên theo DB
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
    }

    public class SectionListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int ClassesId { get; set; }
        public int SyllabusSectionId { get; set; }
        public int Order { get; set; }
        public int Status { get; set; }
        public int? DurationMinutes { get; set; }
    }

    public class CreateSectionDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int ClassesId { get; set; }

        [Range(1, int.MaxValue)]
        public int SyllabusSectionId { get; set; }

        [Range(1, 720)]
        public int? DurationMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int Order { get; set; }

        public DateTime? StartDate { get; set; }   // nếu null -> BE set = UtcNow
        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int Status { get; set; } = 1;       // Planned
    }

    public class UpdateSectionDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int? ClassesId { get; set; }

        [Range(1, int.MaxValue)]
        public int? SyllabusSectionId { get; set; }

        [Range(0, int.MaxValue)]
        public int? DurationMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int? Order { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int? Status { get; set; }
    }

    public class SectionQueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

       
    }
}
