namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
        // Nếu entity có thêm trường (IsMultipleChoice, etc.) bạn có thể bổ sung vào đây
    }

    public class CreateQuizQuestionDto
    {
        public string Name { get; set; } = null!;
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateQuizQuestionDto
    {
        public string? Name { get; set; }
        public decimal? QuestionScore { get; set; }
        public string? Description { get; set; }
    }
}
