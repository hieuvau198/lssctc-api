using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Classes.Dtos
{
    public class CreateClassDto
    {
        [Required(ErrorMessage = "Class name is required.")]
        [StringLength(200, ErrorMessage = "Class name cannot exceed 200 characters.")]
        public string Name { get; set; } = null!;

        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int? Capacity { get; set; }

        [Required(ErrorMessage = "Program ID is required.")]
        public int ProgramId { get; set; }
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Class code is required.")]
        [StringLength(50, ErrorMessage = "Class code cannot exceed 50 characters.")]
        public string ClassCode { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required.")]
        public DateTime? EndDate { get; set; }
    }
}
