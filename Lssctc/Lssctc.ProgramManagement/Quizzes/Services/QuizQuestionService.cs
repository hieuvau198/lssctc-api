using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public class QuizQuestionService : IQuizQuestionService
    {
        private readonly IUnitOfWork _uow;

        public QuizQuestionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto)
        {
            // Validate question input data
            if (dto == null) throw new ValidationException("Body is required.");

            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            // Validate and normalize Name
            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 500)
                throw new ValidationException("Name must be at most 500 characters.");

            // Normalize whitespace (combine multiple spaces into one)
            var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            // Check if question name is unique in the quiz (case-insensitive)
            var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x =>
                x.QuizId == quizId &&
                x.Name != null &&
                x.Name.ToLower() == normalizedName.ToLower());

            if (nameExists)
                throw new ValidationException("A question with the same name already exists in this quiz.");

            // Validate Description
            if (dto.Description != null)
            {
                var trimmedDescription = dto.Description.Trim();
                if (trimmedDescription.Length > 2000)
                    throw new ValidationException("Description must be at most 2000 characters.");
                dto.Description = string.IsNullOrWhiteSpace(trimmedDescription) ? null : trimmedDescription;
            }

            // Validate ImageUrl
            if (dto.ImageUrl != null)
            {
                var trimmedImageUrl = dto.ImageUrl.Trim();
                if (trimmedImageUrl.Length > 500)
                    throw new ValidationException("ImageUrl must be at most 500 characters.");
                dto.ImageUrl = string.IsNullOrWhiteSpace(trimmedImageUrl) ? null : trimmedImageUrl;
            }

            // Validate QuestionScore (>= 0, round to 2 decimal places)
            decimal? score = dto.QuestionScore;
            if (score.HasValue)
            {
                if (score.Value < 0m)
                    throw new ValidationException("QuestionScore must be greater than or equal to 0.");

                score = Math.Round(score.Value, 2, MidpointRounding.AwayFromZero);

                // Check if total score would exceed quiz.TotalScore
                if (quiz.TotalScore.HasValue)
                {
                    var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                        .Where(q => q.QuizId == quizId && q.QuestionScore != null)
                        .SumAsync(q => q.QuestionScore) ?? 0m;

                    var willBe = used + score.Value;
                    if (willBe > quiz.TotalScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
                }
            }

            // Create question entity
            var entity = new QuizQuestion
            {
                QuizId = quizId,
                Name = normalizedName,
                QuestionScore = score,
                Description = dto.Description,
                IsMultipleAnswers = dto.IsMultipleAnswers,
                ImageUrl = dto.ImageUrl
            };

            await _uow.QuizQuestionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<PagedResult<QuizQuestionNoOptionsDto>> GetQuestionsByQuizIdPaged(int quizId, int page, int pageSize)
        {
            // Validate quiz exists
            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit maximum page size

            // Get the total count of questions for this quiz
            var totalCount = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .CountAsync(q => q.QuizId == quizId);

            // Get the paginated questions
            var questions = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuizQuestionNoOptionsDto
                {
                    Id = q.Id,
                    QuizId = q.QuizId,
                    Name = q.Name,
                    QuestionScore = q.QuestionScore,
                    Description = q.Description,
                    IsMultipleAnswers = q.IsMultipleAnswers,
                    ImageUrl = q.ImageUrl
                })
                .ToListAsync();

            return new PagedResult<QuizQuestionNoOptionsDto>
            {
                Items = questions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<QuizQuestionNoOptionsDto?> GetQuestionById(int questionId)
        {
            var question = await _uow.QuizQuestionRepository.GetByIdAsync(questionId);
            if (question == null) return null;

            return new QuizQuestionNoOptionsDto
            {
                Id = question.Id,
                QuizId = question.QuizId,
                Name = question.Name,
                QuestionScore = question.QuestionScore,
                Description = question.Description,
                IsMultipleAnswers = question.IsMultipleAnswers,
                ImageUrl = question.ImageUrl
            };
        }

        public async Task<bool> DeleteQuestionById(int questionId)
        {
            var entity = await _uow.QuizQuestionRepository.GetByIdAsync(questionId);
            if (entity == null) return false;

            // Prevent deletion if the quiz (parent) is added to any activity
            var usedInActivity = await _uow.ActivityQuizRepository.ExistsAsync(aq => aq.QuizId == entity.QuizId);
            if (usedInActivity)
                throw new ValidationException("Cannot delete question because its quiz is used in an activity.");

            // Get the quiz to update its total score later
            var quiz = await _uow.QuizRepository.GetByIdAsync(entity.QuizId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz {entity.QuizId} not found.");

            // Store the question score before deletion
            var questionScore = entity.QuestionScore ?? 0m;

            // Get all options associated with this question and delete them
            var options = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
               .Where(o => o.QuizQuestionId == questionId)
               .ToListAsync();

            foreach (var option in options)
            {
                await _uow.QuizQuestionOptionRepository.DeleteAsync(option);
            }

            // Delete the question
            await _uow.QuizQuestionRepository.DeleteAsync(entity);

            // Update quiz's total score by subtracting the deleted question's score
            if (quiz.TotalScore.HasValue && questionScore > 0)
            {
                quiz.TotalScore = Math.Round(quiz.TotalScore.Value - questionScore, 2, MidpointRounding.AwayFromZero);

                // Ensure total score doesn't go below 0
                if (quiz.TotalScore < 0)
                {
                    quiz.TotalScore = 0;
                }

                quiz.UpdatedAt = DateTime.UtcNow;
                await _uow.QuizRepository.UpdateAsync(quiz);
            }

            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuestionsByQuizId(int quizId, BulkUpdateQuizQuestionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");
            if (dto.Questions == null || !dto.Questions.Any())
                throw new ValidationException("Questions list cannot be empty.");

            // Validate quiz exists
            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            // Get all questions for this quiz
            var allQuestions = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            // Validate all question IDs belong to this quiz
            var questionIds = dto.Questions.Select(q => q.Id).ToList();
            var invalidIds = questionIds.Where(id => !allQuestions.Any(q => q.Id == id)).ToList();
            
            if (invalidIds.Any())
                throw new ValidationException($"The following question IDs do not belong to quiz {quizId}: {string.Join(", ", invalidIds)}");

            // Check for duplicate question IDs in the request
            var duplicateIds = questionIds.GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            
            if (duplicateIds.Any())
                throw new ValidationException($"Duplicate question IDs found: {string.Join(", ", duplicateIds)}");

            // Calculate total score if any questions are being updated with new scores
            var totalNewScore = 0m;
            foreach (var updateDto in dto.Questions)
            {
                var question = allQuestions.First(q => q.Id == updateDto.Id);
                var scoreToUse = updateDto.QuestionScore ?? question.QuestionScore ?? 0m;
                totalNewScore += scoreToUse;
            }

            // Add scores of questions not being updated
            var questionsNotBeingUpdated = allQuestions
                .Where(q => !questionIds.Contains(q.Id))
                .ToList();
            
            foreach (var q in questionsNotBeingUpdated)
            {
                totalNewScore += q.QuestionScore ?? 0m;
            }

            // Check if total would exceed quiz total score
            if (quiz.TotalScore.HasValue && totalNewScore > quiz.TotalScore.Value + 0.0001m)
            {
                throw new ValidationException(
                    $"Total question scores ({totalNewScore}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
            }

            // Update each question
            foreach (var updateDto in dto.Questions)
            {
                var entity = allQuestions.First(q => q.Id == updateDto.Id);
                bool scoreChanged = false;
                decimal? newScore = null;

                // Update Name if provided
                if (updateDto.Name != null)
                {
                    var rawName = updateDto.Name.Trim();
                    if (string.IsNullOrWhiteSpace(rawName))
                        throw new ValidationException($"Name cannot be empty for question ID {updateDto.Id}.");

                    if (rawName.Length > 500)
                        throw new ValidationException($"Name must be at most 500 characters for question ID {updateDto.Id}.");

                    var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                    // Check unique name in quiz (exclude current question)
                    var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x =>
                        x.QuizId == quizId &&
                        x.Id != updateDto.Id &&
                        x.Name != null &&
                        x.Name.ToLower() == normalizedName.ToLower());

                    if (nameExists)
                        throw new ValidationException($"A question with the same name already exists in this quiz (Question ID: {updateDto.Id}).");

                    entity.Name = normalizedName;
                }

                // Update Description if provided
                if (updateDto.Description != null)
                {
                    var trimmedDescription = updateDto.Description.Trim();
                    if (trimmedDescription.Length > 2000)
                        throw new ValidationException($"Description must be at most 2000 characters for question ID {updateDto.Id}.");

                    entity.Description = string.IsNullOrWhiteSpace(trimmedDescription) ? null : trimmedDescription;
                }

                // Update ImageUrl if provided
                if (updateDto.ImageUrl != null)
                {
                    var trimmedImageUrl = updateDto.ImageUrl.Trim();
                    if (trimmedImageUrl.Length > 500)
                        throw new ValidationException($"ImageUrl must be at most 500 characters for question ID {updateDto.Id}.");

                    entity.ImageUrl = string.IsNullOrWhiteSpace(trimmedImageUrl) ? null : trimmedImageUrl;
                }

                // Update QuestionScore if provided
                if (updateDto.QuestionScore.HasValue)
                {
                    if (updateDto.QuestionScore.Value < 0m)
                        throw new ValidationException($"QuestionScore must be greater than or equal to 0 for question ID {updateDto.Id}.");

                    var score = Math.Round(updateDto.QuestionScore.Value, 2, MidpointRounding.AwayFromZero);

                    if (entity.QuestionScore != score)
                    {
                        scoreChanged = true;
                        newScore = score;
                    }

                    entity.QuestionScore = score;
                }

                // Update IsMultipleAnswers if provided
                if (updateDto.IsMultipleAnswers.HasValue)
                {
                    entity.IsMultipleAnswers = updateDto.IsMultipleAnswers.Value;
                }

                await _uow.QuizQuestionRepository.UpdateAsync(entity);

                // Redistribute scores to options if score changed
                if (scoreChanged && newScore.HasValue && newScore.Value > 0)
                {
                    var correctOptions = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                        .Where(o => o.QuizQuestionId == entity.Id && o.IsCorrect)
                        .ToListAsync();

                    if (correctOptions.Any())
                    {
                        if (entity.IsMultipleAnswers)
                        {
                            // Multiple choice: divide score evenly
                            var scorePerOption = Math.Round(newScore.Value / correctOptions.Count, 2, MidpointRounding.AwayFromZero);
                            
                            foreach (var option in correctOptions)
                            {
                                option.OptionScore = scorePerOption;
                                await _uow.QuizQuestionOptionRepository.UpdateAsync(option);
                            }
                        }
                        else
                        {
                            // Single choice: full score to the correct option
                            var correctOption = correctOptions.First();
                            correctOption.OptionScore = newScore.Value;
                            await _uow.QuizQuestionOptionRepository.UpdateAsync(correctOption);
                        }
                    }
                }
            }

            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
