using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Dtos
{
    public class SubmitAnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public List<int> SelectedOptionIds { get; set; } = new List<int>();
    }

    public class SubmitQuizDto
    {
        [Required]
        public int ActivityRecordId { get; set; }

        [Required]
        public List<SubmitAnswerDto> Answers { get; set; } = new List<SubmitAnswerDto>();
    }

    public class QuizAttemptAnswerDto
    {
        public int Id { get; set; }
        public int? QuizOptionId { get; set; }
        public bool IsCorrect { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class QuizAttemptQuestionDto
    {
        public int Id { get; set; }
        public int? QuestionId { get; set; }
        public decimal? AttemptScore { get; set; }
        public decimal? QuestionScore { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsMultipleAnswers { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<QuizAttemptAnswerDto> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswerDto>();
    }

    public class QuizAttemptDto
    {
        public int Id { get; set; }
        public int ActivityRecordId { get; set; }
        public int? QuizId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? AttemptScore { get; set; }
        public decimal? MaxScore { get; set; }
        public DateTime QuizAttemptDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? AttemptOrder { get; set; }
        public bool? IsPass { get; set; }
        public bool IsCurrent { get; set; }
        public ICollection<QuizAttemptQuestionDto> QuizAttemptQuestions { get; set; } = new List<QuizAttemptQuestionDto>();
    }
}
