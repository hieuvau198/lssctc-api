using Lssctc.LearningManagement.QuizQuestions.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.QuizQuestions.Services
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
            //  Validate question input data
            if (dto == null) throw new ValidationException("Body is required.");

            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 500)
                throw new ValidationException("Name must be at most 500 characters.");

            // Chuẩn hoá khoảng trắng (gộp nhiều khoảng trắng thành 1)
            var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            //  Tên câu hỏi phải duy nhất trong 1 quiz (so sánh không phân biệt hoa thường)
            var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x =>
                x.QuizId == quizId &&
                x.Name != null &&
                x.Name.ToLower() == normalizedName.ToLower());

            if (nameExists)
                throw new ValidationException("A question with the same name already exists in this quiz.");

            //  Validate Description 
            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            //  Validate QuestionScore (>= 0, làm tròn 2 chữ số)
            decimal? score = dto.QuestionScore;
            if (score.HasValue)
            {
                if (score.Value < 0m)
                    throw new ValidationException("QuestionScore must be greater than or equal to 0.");

                score = Math.Round(score.Value, 2, MidpointRounding.AwayFromZero);

                //  Không vượt tổng điểm quiz (nếu quiz.TotalScore có giá trị)
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

            // Create question manually without AutoMapper
            var entity = new QuizQuestion
            {
                QuizId = quizId,
                Name = normalizedName,
                QuestionScore = score,
                Description = dto.Description,
                IsMultipleAnswers = dto.IsMultipleAnswers // Map từ DTO thay vì default false
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
                    IsMultipleAnswers = q.IsMultipleAnswers
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
                IsMultipleAnswers = question.IsMultipleAnswers
            };
        }

        public async Task<bool> UpdateQuestionById(int questionId, UpdateQuizQuestionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var entity = await _uow.QuizQuestionRepository.GetByIdAsync(questionId);
            if (entity == null) return false;

            // Update Name if provided
            if (!string.IsNullOrEmpty(dto.Name))
            {
                var rawName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(rawName))
                    throw new ValidationException("Name cannot be empty.");

                if (rawName.Length > 500)
                    throw new ValidationException("Name must be at most 500 characters.");

                // Normalize whitespace
                var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // Check unique name in quiz (exclude current question)
                var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x =>
                    x.QuizId == entity.QuizId &&
                    x.Id != questionId &&
                    x.Name != null &&
                    x.Name.ToLower() == normalizedName.ToLower());

                if (nameExists)
                    throw new ValidationException("A question with the same name already exists in this quiz.");

                entity.Name = normalizedName;
            }

            // Update Description if provided (allow empty string to clear description)
            if (dto.Description != null)
            {
                if (dto.Description.Length > 2000)
                    throw new ValidationException("Description must be at most 2000 characters.");

                entity.Description = dto.Description;
            }

            // Update QuestionScore if provided
            if (dto.QuestionScore.HasValue)
            {
                if (dto.QuestionScore.Value < 0m)
                    throw new ValidationException("QuestionScore must be greater than or equal to 0.");

                var score = Math.Round(dto.QuestionScore.Value, 2, MidpointRounding.AwayFromZero);

                // Check if updating score would exceed quiz total score
                var quiz = await _uow.QuizRepository.GetByIdAsync(entity.QuizId);
                if (quiz?.TotalScore.HasValue == true)
                {
                    var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                        .Where(q => q.QuizId == entity.QuizId && q.Id != questionId && q.QuestionScore != null)
                        .SumAsync(q => q.QuestionScore) ?? 0m;

                    var willBe = used + score;
                    if (willBe > quiz.TotalScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
                }

                entity.QuestionScore = score;
            }

            // Update IsMultipleAnswers if provided
            if (dto.IsMultipleAnswers.HasValue)
            {
                entity.IsMultipleAnswers = dto.IsMultipleAnswers.Value;
            }

            await _uow.QuizQuestionRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionById(int questionId)
        {
            var entity = await _uow.QuizQuestionRepository.GetByIdAsync(questionId);
            if (entity == null) return false;

            // Check if question has associated options
            var hasOptions = await _uow.QuizQuestionOptionRepository.ExistsAsync(o => o.QuizQuestionId == questionId);
            if (hasOptions)
            {
                throw new ValidationException("Cannot delete question that has associated options. Please delete all options first.");
            }

            await _uow.QuizQuestionRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
