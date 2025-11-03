using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Activities.Dtos
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string ActivityTitle { get; set; } = null!;
        public string? ActivityDescription { get; set; }
        public string? ActivityType { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }

    public class CreateActivityDto
    {
        [Required(ErrorMessage = "Activity title is required.")]
        [StringLength(200, ErrorMessage = "Activity title cannot exceed 200 characters.")]
        public string? ActivityTitle { get; set; }

        [StringLength(1000, ErrorMessage = "Activity description cannot exceed 1000 characters.")]
        public string? ActivityDescription { get; set; }

        [Required(ErrorMessage = "Activity type is required.")]
        [RegularExpression("^(Material|Quiz|Practice)$", ErrorMessage = "ActivityType must be one of: Material, Quiz, Practice.")]
        public string? ActivityType { get; set; }

        [Range(1, 600, ErrorMessage = "Estimated duration must be between 1 and 600 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }
    }

    public class UpdateActivityDto
    {
        [Required(ErrorMessage = "Activity title is required.")]
        [StringLength(200, ErrorMessage = "Activity title cannot exceed 200 characters.")]
        public string? ActivityTitle { get; set; }

        [StringLength(1000, ErrorMessage = "Activity description cannot exceed 1000 characters.")]
        public string? ActivityDescription { get; set; }

        [RegularExpression("^(Material|Quiz|Practice)$", ErrorMessage = "ActivityType must be one of: Material, Quiz, Practice.")]
        public string? ActivityType { get; set; }

        [Range(1, 600, ErrorMessage = "Estimated duration must be between 1 and 600 minutes.")]
        public int? EstimatedDurationMinutes { get; set; }
    }
}
