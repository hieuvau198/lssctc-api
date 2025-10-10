namespace Lssctc.ProgramManagement.Learnings.Dtos
{
    public class LearningsSectionQuizDto
    {
        public int SectionQuizId { get; set; } // SectionQuiz
        public int QuizId { get; set; } // Quiz
        public int? LearningRecordPartitionId { get; set; } // LearningRecordPartition
        public int? SectionQuizAttemptId { get; set; }  // SectionQuizAttempt
        public string? QuizName { get; set; } // Quiz
        public decimal? PassScoreCriteria { get; set; } // Quiz
        public int? TimelimitMinute { get; set; } // Quiz
        public decimal? TotalScore { get; set; } // Quiz
        public string? Description { get; set; } // Quiz
        public bool IsCompleted { get; set; }   // LearningRecordPartition
        public decimal? AttemptScore { get; set; }  // LearningRecordPartition
        public bool? LastAttemptIsPass { get; set; }    // SectionQuizAttempt
        public DateTime? LastAttemptDate { get; set; }  // SectionQuizAttempt
    }

    public class CreateLearningsSectionQuizAttemptDto
    {
        public int SectionQuizId { get; set; } // SectionQuiz
        public int QuizId { get; set; } // Quiz
        public List<CreateLearningsSectionQuizAttemptAnswerDto> Answers { get; set; } = new();
    }
    public class CreateLearningsSectionQuizAttemptAnswerDto
    {
        public int QuestionId { get; set; } // QuizQuestion
        public List<int> SelectedOptionIds { get; set; } = new(); // QuizQuestionOption
    }
}
