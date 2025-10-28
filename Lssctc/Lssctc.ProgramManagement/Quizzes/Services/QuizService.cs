using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;
using Lssctc.ProgramManagement.QuizQuestions.DTOs;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;

        public QuizService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Mapping Helper Methods

        private static QuizDto MapToQuizDto(Quiz entity)
        {
            return new QuizDto
            {
                Id = entity.Id,
                Name = entity.Name,
                PassScoreCriteria = entity.PassScoreCriteria,
                TimelimitMinute = entity.TimelimitMinute,
                TotalScore = entity.TotalScore,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Questions = entity.QuizQuestions?.OrderBy(q => q.Id)
                    .Select(MapToQuizQuestionDto).ToList() ?? new List<QuizQuestionDto>()
            };
        }

        private static QuizQuestionDto MapToQuizQuestionDto(QuizQuestion entity)
        {
            return new QuizQuestionDto
            {
                Id = entity.Id,
                QuizId = entity.QuizId,
                Name = entity.Name,
                QuestionScore = entity.QuestionScore,
                Description = entity.Description,
                IsMultipleAnswers = entity.IsMultipleAnswers,
                Options = entity.QuizQuestionOptions?.OrderBy(o => o.DisplayOrder)
                    .Select(MapToQuizQuestionOptionDto).ToList() ?? new List<QuizQuestionOptionDto>()
            };
        }

        private static QuizQuestionOptionDto MapToQuizQuestionOptionDto(QuizQuestionOption entity)
        {
            return new QuizQuestionOptionDto
            {
                Id = entity.Id,
                QuizQuestionId = entity.QuizQuestionId,
                Name = entity.Name,
                Description = entity.Description,
                IsCorrect = entity.IsCorrect,
                DisplayOrder = entity.DisplayOrder,
                OptionScore = entity.OptionScore,
                Explanation = entity.Explanation
            };
        }

        private static QuizOnlyDto MapToQuizSummaryDto(Quiz entity)
        {
            return new QuizOnlyDto
            {
                Id = entity.Id,
                Name = entity.Name,
                PassScoreCriteria = entity.PassScoreCriteria,
                TimelimitMinute = entity.TimelimitMinute,
                TotalScore = entity.TotalScore,
                Description = entity.Description,
               
            };
        }

        private static QuizQuestionNoOptionsDto MapToQuizQuestionNoOptionsDto(QuizQuestion entity)
        {
            return new QuizQuestionNoOptionsDto
            {
                Id = entity.Id,
                QuizId = entity.QuizId,
                Name = entity.Name,
                QuestionScore = entity.QuestionScore,
                Description = entity.Description,
                IsMultipleAnswers = entity.IsMultipleAnswers
            };
        }

        private static QuizDetailDto MapToQuizDetailDto(Quiz entity)
        {
            return new QuizDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                PassScoreCriteria = entity.PassScoreCriteria,
                TimelimitMinute = entity.TimelimitMinute,
                TotalScore = entity.TotalScore,
                Description = entity.Description,
                Questions = entity.QuizQuestions?.OrderBy(q => q.Id)
                    .Select(MapToQuizDetailQuestionDto).ToList() ?? new List<QuizDetailQuestionDto>()
            };
        }

        private static QuizDetailQuestionDto MapToQuizDetailQuestionDto(QuizQuestion entity)
        {
            return new QuizDetailQuestionDto
            {
                Id = entity.Id,
                QuizId = entity.QuizId,
                Name = entity.Name,
                QuestionScore = entity.QuestionScore,
                Description = entity.Description,
                IsMultipleAnswers = entity.IsMultipleAnswers,
                Options = entity.QuizQuestionOptions?.OrderBy(o => o.DisplayOrder)
                    .Select(MapToQuizDetailQuestionOptionDto).ToList() ?? new List<QuizDetailQuestionOptionDto>()
            };
        }

        private static QuizDetailQuestionOptionDto MapToQuizDetailQuestionOptionDto(QuizQuestionOption entity)
        {
            return new QuizDetailQuestionOptionDto
            {
                Id = entity.Id,
                QuizQuestionId = entity.QuizQuestionId,
                Name = entity.Name,
                Description = entity.Description,
                IsCorrect = entity.IsCorrect,
                DisplayOrder = entity.DisplayOrder,
                OptionScore = entity.OptionScore
            };
        }

        private static QuizTraineeDetailDto MapToQuizTraineeDetailDto(Quiz entity)
        {
            return new QuizTraineeDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                PassScoreCriteria = entity.PassScoreCriteria,
                TimelimitMinute = entity.TimelimitMinute,
                TotalScore = entity.TotalScore,
                Description = entity.Description,
                Questions = entity.QuizQuestions?.OrderBy(q => q.Id)
                    .Select(MapToQuizTraineeQuestionDto).ToList() ?? new List<QuizTraineeQuestionDto>()
            };
        }

        private static QuizTraineeQuestionDto MapToQuizTraineeQuestionDto(QuizQuestion entity)
        {
            return new QuizTraineeQuestionDto
            {
                Id = entity.Id,
                QuizId = entity.QuizId,
                Name = entity.Name,
                QuestionScore = entity.QuestionScore,
                Description = entity.Description,
                IsMultipleAnswers = entity.IsMultipleAnswers,
                Options = entity.QuizQuestionOptions?.OrderBy(o => o.DisplayOrder)
                    .Select(MapToQuizTraineeQuestionOptionDto).ToList() ?? new List<QuizTraineeQuestionOptionDto>()
            };
        }

        private static QuizTraineeQuestionOptionDto MapToQuizTraineeQuestionOptionDto(QuizQuestionOption entity)
        {
            return new QuizTraineeQuestionOptionDto
            {
                Id = entity.Id,
                QuizQuestionId = entity.QuizQuestionId,
                Name = entity.Name,
                Description = entity.Description,
                DisplayOrder = entity.DisplayOrder,
                OptionScore = entity.OptionScore
                // Note: IsCorrect is intentionally excluded for trainee DTOs
            };
        }

        private static Quiz MapToQuizEntity(CreateQuizDto dto)
        {
            return new Quiz
            {
                Name = dto.Name,
                PassScoreCriteria = dto.PassScoreCriteria,
                TimelimitMinute = dto.TimelimitMinute,
                TotalScore = 10m, // fixed default, user cannot change
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static void UpdateQuizEntity(Quiz entity, UpdateQuizDto dto)
        {
            entity.Name = dto.Name;
            entity.PassScoreCriteria = dto.PassScoreCriteria;
            entity.TimelimitMinute = dto.TimelimitMinute;
            entity.TotalScore = 10m; // keep default 10 and do not allow change
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        public async Task<QuizDto?> GetQuizById(int id)
        {
            var entity = await _uow.QuizRepository.GetAllAsQueryable()
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            return entity == null ? null : MapToQuizDto(entity);
        }

        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, CancellationToken ct = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var query = _uow.QuizRepository.GetAllAsQueryable()
                .Include(q => q.QuizQuestions);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            var mappedItems = items.Select(MapToQuizSummaryDto).ToList();

            return new PagedResult<QuizOnlyDto>
            {
                Items = mappedItems,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<QuizDetailDto>> GetDetailQuizzes(int pageIndex, int pageSize, CancellationToken ct = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var query = _uow.QuizRepository.GetAllAsQueryable()
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            var mappedItems = items.Select(MapToQuizDetailDto).ToList();

            return new PagedResult<QuizDetailDto>
            {
                Items = mappedItems,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default)
        {
            var quizDetail = await _uow.QuizRepository.GetAllAsQueryable()
                .Where(q => q.Id == quizId)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (quizDetail == null)
            {
                return null;
            }

            return MapToQuizDetailDto(quizDetail);
        }

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            var quizDetail = await _uow.QuizRepository.GetAllAsQueryable()
                .Where(q => q.Id == quizId)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (quizDetail == null)
            {
                return null;
            }

            return MapToQuizTraineeDetailDto(quizDetail);
        }

        public async Task<int> CreateQuiz(CreateQuizDto dto)
        {
            // Validate PassScoreCriteria vs fixed TotalScore (10)
            if (dto.PassScoreCriteria.HasValue && dto.PassScoreCriteria.Value > 10m)
                throw new ValidationException("PassScoreCriteria must be less than TotalScore (10).");

            var entity = MapToQuizEntity(dto);
            await _uow.QuizRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateQuizById(int id, UpdateQuizDto dto)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // Validate PassScoreCriteria vs fixed TotalScore (10)
            if (dto.PassScoreCriteria.HasValue && dto.PassScoreCriteria.Value > 10m)
                throw new ValidationException("PassScoreCriteria must be less than TotalScore (10).");

            UpdateQuizEntity(entity, dto);
            await _uow.QuizRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizById(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.QuizRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        //==== section quiz ======
        public async Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(
            int sectionQuizId, CancellationToken ct = default)
        {
            var quiz = await _uow.SectionQuizRepository.GetAllAsQueryable()
                .Where(sq => sq.Id == sectionQuizId)
                .Select(sq => sq.Quiz)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuizQuestionOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            return quiz == null ? null : MapToQuizTraineeDetailDto(quiz);
        }

        //==== create question and options by quiz id
        public async Task<int> CreateQuestionWithOptionsByQuizId(
            int quizId, CreateQuizQuestionWithOptionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            // --- Validate Quiz + Question ---
            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");
            var qName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (qName.Length > 500)
                throw new ValidationException("Name must be at most 500 characters.");

            var dupName = await _uow.QuizQuestionRepository.ExistsAsync(q =>
                q.QuizId == quizId && q.Name != null && q.Name.ToLower() == qName.ToLower());
            if (dupName)
                throw new ValidationException("A question with the same name already exists in this quiz.");

            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            decimal? questionScore = dto.QuestionScore;
            if (questionScore.HasValue)
            {
                if (questionScore.Value < 0m)
                    throw new ValidationException("QuestionScore must be >= 0.");
                questionScore = Math.Round(questionScore.Value, 2, MidpointRounding.AwayFromZero);

                if (quiz.TotalScore.HasValue)
                {
                    var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                        .Where(q => q.QuizId == quizId && q.QuestionScore != null)
                        .SumAsync(q => q.QuestionScore) ?? 0m;

                    var willBe = used + questionScore.Value;
                    if (willBe > quiz.TotalScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
                }
            }

            // --- Validate Options ---
            if (dto.Options == null || dto.Options.Count < 2)
                throw new ValidationException("At least 2 options are required.");

            var correctCount = dto.Options.Count(o => o.IsCorrect);
            if (correctCount == 0)
                throw new ValidationException("At least one correct option is required.");
            if (!dto.IsMultipleAnswers && correctCount != 1)
                throw new ValidationException("Exactly 1 correct option is required when IsMultipleAnswers = false.");

            decimal totalOptionScore = 0m;
            var normalizedOptions = new List<(string Name, string? Desc, bool IsCorrect, decimal? Score)>();
            for (int i = 0; i < dto.Options.Count; i++)
            {
                var o = dto.Options[i];
                var rawOptName = (o.Name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(rawOptName))
                    throw new ValidationException($"Option[{i}].Name is required.");
                var optName = string.Join(" ", rawOptName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (optName.Length > 500)
                    throw new ValidationException($"Option[{i}].Name must be at most 500 characters.");
                if (o.Description != null && o.Description.Length > 2000)
                    throw new ValidationException($"Option[{i}].Description must be at most 2000 characters.");

                if (o.OptionScore.HasValue)
                {
                    var v = o.OptionScore.Value;
                    if (v < 0m || v > 999.99m)
                        throw new ValidationException($"Option[{i}].OptionScore must be between 0 and 999.99.");
                    totalOptionScore += Math.Round(v, 2, MidpointRounding.AwayFromZero);
                }

                normalizedOptions.Add((optName, o.Description, o.IsCorrect, o.OptionScore));
            }

            if (questionScore.HasValue && totalOptionScore > questionScore.Value + 0.0001m)
                throw new ValidationException(
                    $"Sum of option scores ({totalOptionScore}) exceeds QuestionScore ({questionScore.Value}).");

            // --- Create Question tr??c ?? có Id ---
            var question = new QuizQuestion
            {
                QuizId = quizId,
                Name = qName,
                Description = dto.Description,
                QuestionScore = questionScore,
                IsMultipleAnswers = dto.IsMultipleAnswers
            };
            await _uow.QuizQuestionRepository.CreateAsync(question);
            await _uow.SaveChangesAsync(); // c?n question.Id

            // --- Chu?n b? Option entities (ch?a set DisplayOrder) ---
            var optionEntities = normalizedOptions.Select(o => new QuizQuestionOption
            {
                QuizQuestionId = question.Id,
                Name = o.Name,
                Description = o.Desc,
                IsCorrect = o.IsCorrect,
                OptionScore = o.Score
                // DisplayOrder s? set ngay tr??c khi Save
            }).ToList();

            // --- Optimistic retry: per-question tr??c, fail thì fallback to global ---
            const int maxRetries = 2;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                // 1) Tính MAX theo câu h?i
                var maxPerQuestion = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Where(o => o.QuizQuestionId == question.Id)
                    .Select(o => o.DisplayOrder)
                    .MaxAsync() ?? 0;

                // 2) Gán order 1..n d?a trên per-question
                int next = maxPerQuestion + 1;
                foreach (var e in optionEntities)
                    e.DisplayOrder = next++;

                try
                {
                    // Add (n?u ch?a add) — n?u ?ã add ? vòng tr??c, EF v?n gi? state Added; c? Save là ??
                    foreach (var e in optionEntities)
                        if (e.Id == 0 && e.DisplayOrder > 0) // heuristic nh?
                            await _uow.QuizQuestionOptionRepository.CreateAsync(e);

                    await _uow.SaveChangesAsync();
                    return question.Id; // OK
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex) && attempt < maxRetries)
                {
                    // Fallback: DB có UNIQUE display_order toàn b?ng ? gán theo GLOBAL MAX và th? l?i
                    var maxGlobal = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                        .Select(o => o.DisplayOrder)
                        .MaxAsync() ?? 0;

                    int gnext = maxGlobal + 1;
                    foreach (var e in optionEntities)
                        e.DisplayOrder = gnext++;

                    try
                    {
                        await _uow.SaveChangesAsync();
                        return question.Id;
                    }
                    catch (DbUpdateException ex2) when (IsUniqueViolation(ex2) && attempt < maxRetries)
                    {
                        // Có th? có ti?n trình khác v?a chen; l?p l?i vòng for ?? tính l?i MAX m?i nh?t
                        continue;
                    }
                }
            }

            throw new ValidationException("Could not assign DisplayOrder due to concurrent inserts. Please retry.");

            static bool IsUniqueViolation(DbUpdateException ex)
                => ex.InnerException is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
        }
    }
}