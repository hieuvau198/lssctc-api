namespace Lssctc.LearningManagement.Quizzes.DTOs
{
    public class QuizQuestionOptionDto
    {
        public int Id { get; set; }
        public int QuizQuestionId { get; set; }
        public string? Name { get; set; }          
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
        public string? Explanation { get; set; }   // lời giải/giải thích (nếu có cột này)
    }

    public class CreateQuizQuestionOptionDto
    {
        public string? Name { get; set; }
        public bool IsCorrect { get; set; }
        public int? DisplayOrder { get; set; }     
        public string? Explanation { get; set; }
    }

    public class UpdateQuizQuestionOptionDto
    {
        public string? Name { get; set; }
        public bool? IsCorrect { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Explanation { get; set; }
    }
}
