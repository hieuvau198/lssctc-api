using System.ComponentModel.DataAnnotations;

namespace Lssctc.SimulationManagement.Practices.Dtos
{
    public class PracticeDto
    {
        public int Id { get; set; }
        public string PracticeName { get; set; } = null!;
        public string? PracticeDescription { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public string? DifficultyLevel { get; set; }
        public int? MaxAttempts { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreatePracticeDto
    {
        [Required]
        [StringLength(200)]
        public string PracticeName { get; set; } = null!;

        [StringLength(1000)]
        public string? PracticeDescription { get; set; }

        public int? EstimatedDurationMinutes { get; set; }

        [StringLength(50)]
        public string? DifficultyLevel { get; set; }

        public int? MaxAttempts { get; set; }
        public bool? IsActive { get; set; } = true;
    }

    public class UpdatePracticeDto
    {
        [StringLength(200)]
        public string? PracticeName { get; set; }

        [StringLength(1000)]
        public string? PracticeDescription { get; set; }

        public int? EstimatedDurationMinutes { get; set; }

        [StringLength(50)]
        public string? DifficultyLevel { get; set; }

        public int? MaxAttempts { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PracticeQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
