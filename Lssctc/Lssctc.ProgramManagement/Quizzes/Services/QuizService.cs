using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
        public QuizService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<int> CreateQuiz(CreateQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 100)
                throw new ValidationException("Name must be at most 100 characters.");

            // Normalize whitespace
            var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            // TotalScore fixed to 10 (BR-37)
            var totalScore = 10m;

            // Validate PassScoreCriteria: must be >0 and <= totalScore
            if (dto.PassScoreCriteria.HasValue)
            {
                var pass = Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero);
                if (pass <= 0m || pass > totalScore)
                    throw new ValidationException($"PassScoreCriteria must be greater than 0 and less than or equal to {totalScore}.");
            }

            // Create quiz
            var entity = new Quiz
            {
                Name = normalizedName,
                PassScoreCriteria = dto.PassScoreCriteria.HasValue ? Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero) : null,
                TimelimitMinute = dto.TimelimitMinute,
                Description = dto.Description,
                TotalScore = totalScore,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.QuizRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateQuizById(int id, UpdateQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (!string.IsNullOrEmpty(dto.Name))
            {
                var rawName = dto.Name.Trim();
                if (string.IsNullOrWhiteSpace(rawName))
                    throw new ValidationException("Name cannot be empty.");
                if (rawName.Length > 100)
                    throw new ValidationException("Name must be at most 100 characters.");

                entity.Name = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            if (dto.PassScoreCriteria.HasValue)
            {
                var total = entity.TotalScore ?? 10m;
                var pass = Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero);
                if (pass <= 0m || pass > total)
                    throw new ValidationException($"PassScoreCriteria must be greater than 0 and less than or equal to {total}.");

                entity.PassScoreCriteria = pass;
            }

            if (dto.TimelimitMinute.HasValue)
            {
                if (dto.TimelimitMinute < 1 || dto.TimelimitMinute > 600)
                    throw new ValidationException("TimelimitMinute must be between 1 and 600 minutes.");
                entity.TimelimitMinute = dto.TimelimitMinute;
            }

            if (dto.Description != null)
            {
                if (dto.Description.Length > 2000)
                    throw new ValidationException("Description must be at most 2000 characters.");
                entity.Description = dto.Description;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _uow.QuizRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizById(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Prevent deletion if quiz has questions or is used in activities or attempts
            var hasQuestions = await _uow.QuizQuestionRepository.ExistsAsync(q => q.QuizId == id);
            if (hasQuestions)
                throw new ValidationException("Cannot delete quiz that has questions. Please delete questions first.");

            var usedInActivity = await _uow.ActivityQuizRepository.ExistsAsync(aq => aq.QuizId == id);
            if (usedInActivity)
                throw new ValidationException("Cannot delete quiz that is used in an activity.");

            var hasAttempts = await _uow.QuizAttemptRepository.ExistsAsync(a => a.QuizId == id);
            if (hasAttempts)
                throw new ValidationException("Cannot delete quiz that has attempts recorded.");

            await _uow.QuizRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, CancellationToken ct = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.QuizRepository.GetAllAsQueryable();
            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(q => q.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuizOnlyDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    PassScoreCriteria = q.PassScoreCriteria,
                    TimelimitMinute = q.TimelimitMinute,
                    TotalScore = q.TotalScore,
                    Description = q.Description
                })
                .ToListAsync(ct);

            return new PagedResult<QuizOnlyDto>
            {
                Items = items,
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<PagedResult<QuizDetailDto>> GetDetailQuizzes(int pageIndex, int pageSize, CancellationToken ct = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.QuizRepository.GetAllAsQueryable();
            var total = await query.CountAsync(ct);

            var entities = await query
                .OrderBy(q => q.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions)
                .AsNoTracking()
                .ToListAsync(ct);

            var items = entities.Select(q => new QuizDetailDto
            {
                Id = q.Id,
                Name = q.Name,
                PassScoreCriteria = q.PassScoreCriteria,
                TimelimitMinute = q.TimelimitMinute,
                TotalScore = q.TotalScore,
                Description = q.Description,
                Questions = q.QuizQuestions.Select(qq => new QuizDetailQuestionDto
                {
                    Id = qq.Id,
                    QuizId = qq.QuizId,
                    Name = qq.Name,
                    QuestionScore = qq.QuestionScore,
                    Description = qq.Description,
                    IsMultipleAnswers = qq.IsMultipleAnswers,
                    Options = qq.QuizQuestionOptions
                        .OrderBy(o => o.DisplayOrder ?? int.MaxValue)
                        .Select(o => new QuizDetailQuestionOptionDto
                        {
                            Id = o.Id,
                            QuizQuestionId = o.QuizQuestionId,
                            Name = o.Name,
                            Description = o.Description,
                            IsCorrect = o.IsCorrect,
                            DisplayOrder = o.DisplayOrder,
                            OptionScore = o.OptionScore
                        }).ToList()
                }).ToList()
            }).ToList();

            return new PagedResult<QuizDetailDto>
            {
                Items = items,
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<QuizDto?> GetQuizById(int id)
        {
            var q = await _uow.QuizRepository.GetAllAsQueryable()
                .Where(x => x.Id == id)
                .Select(x => new QuizDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PassScoreCriteria = x.PassScoreCriteria,
                    TimelimitMinute = x.TimelimitMinute,
                    TotalScore = x.TotalScore,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Questions = x.QuizQuestions.Select(qq => new QuizQuestionDto
                    {
                        Id = qq.Id,
                        QuizId = qq.QuizId,
                        Name = qq.Name,
                        QuestionScore = qq.QuestionScore,
                        Description = qq.Description,
                        IsMultipleAnswers = qq.IsMultipleAnswers,
                        Options = qq.QuizQuestionOptions.Select(o => new QuizQuestionOptionDto
                        {
                            Id = o.Id,
                            QuizQuestionId = o.QuizQuestionId,
                            Name = o.Name,
                            Description = o.Description,
                            IsCorrect = o.IsCorrect,
                            Explanation = o.Explanation,
                            DisplayOrder = o.DisplayOrder,
                            OptionScore = o.OptionScore
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return q;
        }

        public async Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default)
        {
            var q = await _uow.QuizRepository.GetAllAsQueryable()
                .Where(x => x.Id == quizId)
                .Select(x => new QuizDetailDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PassScoreCriteria = x.PassScoreCriteria,
                    TimelimitMinute = x.TimelimitMinute,
                    TotalScore = x.TotalScore,
                    Description = x.Description,
                    Questions = x.QuizQuestions.Select(qq => new QuizDetailQuestionDto
                    {
                        Id = qq.Id,
                        QuizId = qq.QuizId,
                        Name = qq.Name,
                        QuestionScore = qq.QuestionScore,
                        Description = qq.Description,
                        IsMultipleAnswers = qq.IsMultipleAnswers,
                        Options = qq.QuizQuestionOptions.Select(o => new QuizDetailQuestionOptionDto
                        {
                            Id = o.Id,
                            QuizQuestionId = o.QuizQuestionId,
                            Name = o.Name,
                            Description = o.Description,
                            IsCorrect = o.IsCorrect,
                            DisplayOrder = o.DisplayOrder,
                            OptionScore = o.OptionScore
                        }).OrderBy(o => o.DisplayOrder ?? int.MaxValue).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            return q;
        }

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default)
        {
            // return quiz detail for trainee (no IsCorrect on options)
            var q = await _uow.QuizRepository.GetAllAsQueryable()
                .Where(x => x.Id == quizId)
                .Select(x => new QuizTraineeDetailDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    PassScoreCriteria = x.PassScoreCriteria,
                    TimelimitMinute = x.TimelimitMinute,
                    TotalScore = x.TotalScore,
                    Description = x.Description,
                    Questions = x.QuizQuestions.Select(qq => new QuizTraineeQuestionDto
                    {
                        Id = qq.Id,
                        QuizId = qq.QuizId,
                        Name = qq.Name,
                        QuestionScore = qq.QuestionScore,
                        Description = qq.Description,
                        IsMultipleAnswers = qq.IsMultipleAnswers,
                        Options = qq.QuizQuestionOptions.Select(o => new QuizTraineeQuestionOptionDto
                        {
                            Id = o.Id,
                            QuizQuestionId = o.QuizQuestionId,
                            Name = o.Name,
                            Description = o.Description,
                            DisplayOrder = o.DisplayOrder,
                            OptionScore = o.OptionScore
                        }).OrderBy(o => o.DisplayOrder ?? int.MaxValue).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            return q;
        }

        public async Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(int sectionQuizId, CancellationToken ct = default)
        {
            // Find ActivityQuiz -> SectionQuiz mapping
            var activityQuiz = await _uow.ActivityQuizRepository.GetByIdAsync(sectionQuizId);
            if (activityQuiz == null) return null;

            return await GetQuizDetailForTrainee(activityQuiz.QuizId, ct);
        }

        public async Task<int> CreateQuestionWithOptionsByQuizId(int quizId, CreateQuizQuestionWithOptionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            // validate question name
            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");
            if (rawName.Length > 500)
                throw new ValidationException("Name must be at most 500 characters.");
            var normalizedName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x => x.QuizId == quizId && x.Name != null && x.Name.ToLower() == normalizedName.ToLower());
            if (nameExists) throw new ValidationException("A question with the same name already exists in this quiz.");

            // check max questions (BR-35)
            var questionCount = await _uow.QuizQuestionRepository.GetAllAsQueryable().CountAsync(q => q.QuizId == quizId);
            if (questionCount >= 100) throw new ValidationException("A quiz cannot contain more than 100 questions.");

            // Validate options
            if (dto.Options == null || dto.Options.Count == 0)
                throw new ValidationException("Options are required.");

            if (dto.Options.Count > 20) throw new ValidationException("A quiz question cannot have more than 20 answer options.");

            // At least one correct option
            if (!dto.Options.Any(o => o.IsCorrect))
                throw new ValidationException("At least one option must be marked as correct.");

            // Validate QuestionScore: must be >0 and <10 (BR-38)
            // For this DTO there is no QuestionScore field; derive question score from sum of option scores if provided, or default to 0 - not allowed
            decimal? questionScore = null;
            var optionsWithScore = dto.Options.Where(o => o.OptionScore.HasValue).ToList();
            if (optionsWithScore.Count > 0)
            {
                questionScore = Math.Round(optionsWithScore.Sum(o => o.OptionScore!.Value), 2, MidpointRounding.AwayFromZero);
            }

            if (!questionScore.HasValue)
            {
                throw new ValidationException("QuestionScore must be provided via option OptionScore values.");
            }

            if (questionScore <= 0m || questionScore >= 10m)
                throw new ValidationException("The score of any single question must be greater than 0 and less than 10.");

            // Check that adding this question won't make total question scores exceed quiz.TotalScore (BR-39/37)
            var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(q => q.QuizId == quizId && q.QuestionScore != null)
                .SumAsync(q => q.QuestionScore) ?? 0m;

            var willBe = used + questionScore.Value;
            var quizTotal = quiz.TotalScore ?? 10m;
            if (willBe > quizTotal + 0.0001m)
                throw new ValidationException($"Total question scores ({willBe}) would exceed quiz.TotalScore ({quizTotal}).");

            // Create question
            var question = new QuizQuestion
            {
                QuizId = quizId,
                Name = normalizedName,
                Description = dto.Description,
                IsMultipleAnswers = dto.IsMultipleAnswers,
                QuestionScore = questionScore
            };

            await _uow.QuizQuestionRepository.CreateAsync(question);
            await _uow.SaveChangesAsync();

            // Create options
            // compute next global displayOrder if needed
            var maxDisplay = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().Select(x => x.DisplayOrder).MaxAsync();
            var nextDisplayBase = (maxDisplay ?? 0) + 1;
            int idx = 0;
            foreach (var opt in dto.Options)
            {
                var displayOrder = opt.DisplayOrder ?? (nextDisplayBase + idx);
                // ensure displayOrder not duplicate
                var dup = await _uow.QuizQuestionOptionRepository.ExistsAsync(x => x.DisplayOrder == displayOrder);
                if (dup)
                {
                    displayOrder = (await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().Select(x => x.DisplayOrder).MaxAsync()) ?? 0;
                    displayOrder++;
                }

                var optionEntity = new QuizQuestionOption
                {
                    QuizQuestionId = question.Id,
                    Name = opt.Name,
                    Description = opt.Description,
                    IsCorrect = opt.IsCorrect,
                    DisplayOrder = displayOrder,
                    OptionScore = opt.OptionScore
                };

                await _uow.QuizQuestionOptionRepository.CreateAsync(optionEntity);
                idx++;
            }

            // Update quiz.TotalScore if needed (keep as 10)
            // If after adding questions the used sum equals quiz.TotalScore, good. We don't enforce equality here to allow incremental additions.
            await _uow.SaveChangesAsync();

            return question.Id;
        }
    }
}
