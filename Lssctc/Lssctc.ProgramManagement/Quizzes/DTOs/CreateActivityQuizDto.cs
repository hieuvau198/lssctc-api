using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.DTOs
{
    public class CreateActivityQuizDto
    {
        [Required(ErrorMessage = "Quiz ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quiz ID must be greater than 0.")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Activity ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Activity ID must be greater than 0.")]
        public int ActivityId { get; set; }
    }
}
