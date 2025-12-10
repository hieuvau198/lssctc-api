using Lssctc.ProgramManagement.Quizzes.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizValidator
    {
        void ValidateCreateQuiz(CreateQuizWithQuestionsDto dto);
        void ValidateUpdateQuiz(UpdateQuizWithQuestionsDto dto);
    }

    public class QuizValidator : IQuizValidator
    {
        public void ValidateCreateQuiz(CreateQuizWithQuestionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            ValidateQuestions(dto.Questions); // Calls the Create overload

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName)) throw new ValidationException("Quiz name is required.");
            if (rawName.Length > 100) throw new ValidationException("Quiz name must be at most 100 characters.");

            if (dto.PassScoreCriteria.HasValue)
            {
                var pass = Math.Round(dto.PassScoreCriteria.Value, 2);
                if (pass <= 0m || pass > 10m) throw new ValidationException("PassScoreCriteria must be between 0 and 10.");
            }
        }

        public void ValidateUpdateQuiz(UpdateQuizWithQuestionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            ValidateQuestions(dto.Questions); // Calls the Update overload

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name.Trim().Length > 100)
                throw new ValidationException("Quiz name must be at most 100 characters.");

            if (dto.PassScoreCriteria.HasValue && (dto.PassScoreCriteria <= 0 || dto.PassScoreCriteria > 10))
                throw new ValidationException("PassScoreCriteria must be between 0 and 10.");

            if (dto.TimelimitMinute.HasValue && (dto.TimelimitMinute < 1 || dto.TimelimitMinute > 600))
                throw new ValidationException("TimelimitMinute must be between 1 and 600 minutes.");

            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");
        }

        // --- Overload 1: For Create DTOs ---
        private void ValidateQuestions(List<CreateQuizQuestionWithOptionsDto> questions)
        {
            if (questions == null || questions.Count == 0) throw new ValidationException("At least one question is required.");
            if (questions.Count > 100) throw new ValidationException("Max 100 questions allowed.");

            decimal totalScore = 0m;
            int idx = 0;

            foreach (var q in questions)
            {
                idx++;
                ValidateSingleQuestion(idx, q.Name, q.Options?.Cast<object>().ToList(), q.IsMultipleAnswers, q.ImageUrl, q.QuestionScore);
                totalScore += q.QuestionScore.HasValue ? Math.Round(q.QuestionScore.Value, 2) : 0;
            }

            if (Math.Abs(totalScore - 10m) > 0.0001m)
                throw new ValidationException($"Total score must be 10. Current: {totalScore:F2}");
        }

        // --- Overload 2: For Update DTOs ---
        private void ValidateQuestions(List<UpdateQuizQuestionWithOptionsDto> questions)
        {
            if (questions == null || questions.Count == 0) throw new ValidationException("At least one question is required.");
            if (questions.Count > 100) throw new ValidationException("Max 100 questions allowed.");

            decimal totalScore = 0m;
            int idx = 0;

            foreach (var q in questions)
            {
                idx++;
                // Note: We cast options to object list to reuse the shared validation logic
                // assuming the option DTOs share the same property names via dynamic or reflection, 
                // BUT for safety here we will pass the list and let the helper handle the 'dynamic' or manual check.
                // To keep it simple and compile-safe, we'll just manually check options here or reuse a helper that takes specific params.

                // Let's rely on a helper that accepts the raw values to avoid code duplication
                // We need to map the Option DTOs to a common structure or check them inside the loop.

                ValidateSingleQuestion(idx, q.Name, q.Options?.Cast<dynamic>().ToList(), q.IsMultipleAnswers, q.ImageUrl, q.QuestionScore);
                totalScore += q.QuestionScore.HasValue ? Math.Round(q.QuestionScore.Value, 2) : 0;
            }

            if (Math.Abs(totalScore - 10m) > 0.0001m)
                throw new ValidationException($"Total score must be 10. Current: {totalScore:F2}");
        }

        // --- Shared Helper Method ---
        private void ValidateSingleQuestion(int idx, string name, List<dynamic> options, bool isMultipleAnswers, string imageUrl, decimal? questionScore)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ValidationException($"Question #{idx}: Name required.");

            var normName = string.Join(' ', name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (normName.Length > 500) throw new ValidationException($"Question #{idx}: Name too long.");

            if (options == null || options.Count < 2) throw new ValidationException($"Question #{idx}: Must have at least 2 options.");
            if (options.Count > 20) throw new ValidationException($"Question #{idx}: Max 20 options.");

            // Check options using dynamic to access Name/IsCorrect regardless of DTO type
            int correctCount = 0;
            foreach (var opt in options)
            {
                if (string.IsNullOrWhiteSpace(opt.Name))
                    throw new ValidationException($"Question #{idx}: Option names cannot be empty.");
                if ((bool)opt.IsCorrect) correctCount++;
            }

            if (correctCount == 0) throw new ValidationException($"Question #{idx}: Must have at least one correct option.");
            if (!isMultipleAnswers && correctCount > 1) throw new ValidationException($"Question #{idx}: Single choice cannot have multiple correct answers.");

            if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.Length > 500)
                throw new ValidationException($"Question #{idx}: ImageUrl exceeds 500 chars.");

            if (!questionScore.HasValue) throw new ValidationException($"Question #{idx}: Score required.");

            var score = Math.Round(questionScore.Value, 2);
            if (score <= 0 || score >= 10) throw new ValidationException($"Question #{idx}: Score must be (0, 10).");
        }
    }
}