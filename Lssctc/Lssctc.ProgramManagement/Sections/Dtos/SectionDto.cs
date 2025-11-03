using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Sections.Dtos
{
    public class SectionDto
    {
        public int Id { get; set; }

        public string SectionTitle { get; set; } = null!;

        public string? SectionDescription { get; set; }

        public int? EstimatedDurationMinutes { get; set; }
    }
    public class CreateSectionDto
    {
        [Required(ErrorMessage = "Section title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Section title must be between 3 and 200 characters.")]
        public string? SectionTitle { get; set; }

        [StringLength(1000, ErrorMessage = "Section description cannot exceed 1000 characters.")]
        public string? SectionDescription { get; set; }

        [Required(ErrorMessage = "Estimated duration is required.")]
        [Range(1, 1000, ErrorMessage = "Estimated duration must be between 1 and 1000 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }
    }
    public class UpdateSectionDto
    {
        [Required(ErrorMessage = "Section title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Section title must be between 3 and 200 characters.")]
        public string? SectionTitle { get; set; }

        [StringLength(1000, ErrorMessage = "Section description cannot exceed 1000 characters.")]
        public string? SectionDescription { get; set; }

        [Required(ErrorMessage = "Estimated duration is required.")]
        [Range(1, 1000, ErrorMessage = "Estimated duration must be between 1 and 1000 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }
    }
}
