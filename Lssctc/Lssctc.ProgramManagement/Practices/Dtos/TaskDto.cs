using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Practices.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }

        public string TaskName { get; set; } = null!;

        public string? TaskDescription { get; set; }

        public string? ExpectedResult { get; set; }
    }
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task name is required.")]
        [StringLength(200, ErrorMessage = "Task name cannot exceed 200 characters.")]
        public string? TaskName { get; set; }
        [StringLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
        public string? TaskDescription { get; set; }
        [StringLength(1000, ErrorMessage = "Expected result cannot exceed 1000 characters.")]
        public string? ExpectedResult { get; set; }
    }
    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "Task name is required.")]
        [StringLength(200, ErrorMessage = "Task name cannot exceed 200 characters.")]
        public string? TaskName { get; set; }
        [Required(ErrorMessage = "Task description is required.")]
        [StringLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
        public string? TaskDescription { get; set; }
        [Required(ErrorMessage = "Task expected result is required.")]
        [StringLength(1000, ErrorMessage = "Expected result cannot exceed 1000 characters.")]
        public string? ExpectedResult { get; set; }
    }
}
