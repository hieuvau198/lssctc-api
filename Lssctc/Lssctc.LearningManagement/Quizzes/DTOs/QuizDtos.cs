namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateQuizDto
    {
        public string Name { get; set; } = null!;
        public decimal? PassScoreCriteria { get; set; }
        public int? TimelimitMinute { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateQuizDto : CreateQuizDto { }
}
