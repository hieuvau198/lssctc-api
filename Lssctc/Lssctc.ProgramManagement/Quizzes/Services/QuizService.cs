using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
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

          

            var usedInActivity = await _uow.ActivityQuizRepository.ExistsAsync(aq => aq.QuizId == id);
            if (usedInActivity)
                throw new ValidationException("Cannot delete quiz that is used in an activity.");

            var hasAttempts = await _uow.QuizAttemptRepository.ExistsAsync(a => a.QuizId == id);
            if (hasAttempts)
                throw new ValidationException("Cannot delete quiz that has attempts recorded.");

            var questions = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(qq => qq.QuizId == id)
                .ToListAsync();

            foreach (var question in questions)
            {
                var options = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Where(o => o.QuizQuestionId == question.Id)
                    .ToListAsync();

                foreach (var option in options)
                {
                   await _uow.QuizQuestionOptionRepository.DeleteAsync(option);
                }


            }

            //delte all questions
            foreach (var question in questions)
            {
                await _uow.QuizQuestionRepository.DeleteAsync(question);
            }

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

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTraineeByActivityIdAsync(int activityId, CancellationToken ct = default)
        {
            // Find the ActivityQuiz link to get the QuizId
            var activityQuiz = await _uow.ActivityQuizRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(aq => aq.ActivityId == activityId, ct);

            if (activityQuiz == null)
            {
                // This activity is not a quiz or has no quiz linked
                return null;
            }

            // Now call the existing method with the found QuizId
            return await GetQuizDetailForTrainee(activityQuiz.QuizId, ct);
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

            // Validate ImageUrl if provided
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && dto.ImageUrl.Length > 500)
                throw new ValidationException("ImageUrl must be at most 500 characters.");

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
                ImageUrl = dto.ImageUrl,
                QuestionScore = questionScore
            };

            await _uow.QuizQuestionRepository.CreateAsync(question);
            await _uow.SaveChangesAsync();

            // Create options - auto-generate DisplayOrder
            // Get max displayOrder from database
            int maxDisplay = 0;
            if (await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().AnyAsync())
            {
                maxDisplay = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Select(x => x.DisplayOrder ?? 0)
                    .MaxAsync();
            }

            int nextDisplayOrder = maxDisplay + 1;

            foreach (var opt in dto.Options)
            {
                var optionEntity = new QuizQuestionOption
                {
                    QuizQuestionId = question.Id,
                    Name = opt.Name,
                    Description = opt.Description,
                    IsCorrect = opt.IsCorrect,
                    DisplayOrder = nextDisplayOrder++,
                    OptionScore = opt.OptionScore,
                    Explanation = opt.Explanation
                };

                await _uow.QuizQuestionOptionRepository.CreateAsync(optionEntity);
            }

            await _uow.SaveChangesAsync();

            return question.Id;
        }

        // Tạo Quiz kèm Questions và Options cùng một lúc
        public async Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            try
            {
                // Validate questions list
                if (dto.Questions == null || dto.Questions.Count == 0)
                    throw new ValidationException("At least one question is required.");

                if (dto.Questions.Count > 100)
                    throw new ValidationException("A quiz cannot contain more than 100 questions.");

                // Validate all questions BEFORE creating quiz
                var totalScore = 0m;
                int questionIndex = 0;
                
                foreach (var questionDto in dto.Questions)
                {
                    questionIndex++;
                    
                    // Validate question name
                    if (string.IsNullOrWhiteSpace(questionDto.Name))
                        throw new ValidationException($"Question #{questionIndex}: Name cannot be empty.");

                    // Normalize FIRST, then validate length
                    var normalizedName = string.Join(' ', questionDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                    
                    if (normalizedName.Length > 100)
                    {
                        throw new ValidationException(
                            $"Question #{questionIndex}: Name exceeds 100 characters " +
                            $"(actual: {normalizedName.Length} chars after removing extra spaces). " +
                            $"Please shorten the question text.");
                    }

                    // Validate options count
                    if (questionDto.Options == null || questionDto.Options.Count == 0)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least one option.");

                    if (questionDto.Options.Count < 2)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least 2 options.");

                    if (questionDto.Options.Count > 20)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Cannot have more than 20 answer options.");

                    // At least one correct option
                    var correctCount = questionDto.Options.Count(o => o.IsCorrect);
                    if (correctCount == 0)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least one correct option.");

                    // Single choice question: chỉ được có 1 option đúng
                    if (!questionDto.IsMultipleAnswers && correctCount > 1)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Single choice question cannot have more than one correct option.");

                    // Validate each option
                    int optionIndex = 0;
                    foreach (var opt in questionDto.Options)
                    {
                        optionIndex++;
                        if (string.IsNullOrWhiteSpace(opt.Name))
                            throw new ValidationException($"Question #{questionIndex}, Option #{optionIndex}: Name cannot be empty.");
                    }

                    // ImageUrl validation - only check length if provided and not empty
                    // ImageUrl can be null or empty (optional field)
                    if (!string.IsNullOrWhiteSpace(questionDto.ImageUrl) && questionDto.ImageUrl.Length > 500)
                    {
                        throw new ValidationException($"Question #{questionIndex}: ImageUrl exceeds 500 characters.");
                    }

                    // Validate question score - MUST be provided
                    if (!questionDto.QuestionScore.HasValue)
                    {
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): QuestionScore is required.");
                    }

                    decimal questionScore = Math.Round(questionDto.QuestionScore.Value, 2, MidpointRounding.AwayFromZero);
                    
                    // QuestionScore must be > 0 and < 10
                    if (questionScore <= 0m || questionScore >= 10m)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): QuestionScore must be greater than 0 and less than 10 (received: {questionScore}).");

                    // Check total doesn't exceed 10
                    totalScore += questionScore;
                }

                // Validate total score equals exactly 10
                if (Math.Abs(totalScore - 10m) > 0.0001m)
                {
                    if (totalScore < 10m)
                        throw new ValidationException($"Total questions scores ({totalScore:F2}) must equal 10. Currently under by {(10m - totalScore):F2}.");
                    else
                        throw new ValidationException($"Total questions scores ({totalScore:F2}) must equal 10. Currently over by {(totalScore - 10m):F2}.");
                }

                // Step 1: Create Quiz (only after all validations pass)
                var quiz = await CreateQuiz(new CreateQuizDto
                {
                    Name = dto.Name,
                    PassScoreCriteria = dto.PassScoreCriteria,
                    TimelimitMinute = dto.TimelimitMinute,
                    Description = dto.Description
                });

                // Step 2: Create Questions with Options
                // Get initial max displayOrder from database
                int globalDisplayOrder = 0;
                if (await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().AnyAsync())
                {
                    globalDisplayOrder = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                        .Select(x => x.DisplayOrder ?? 0)
                        .MaxAsync();
                }
                globalDisplayOrder++; // Start from next available number

                questionIndex = 0;
                foreach (var questionDto in dto.Questions)
                {
                    questionIndex++;
                    try
                    {
                        // Get question score (already validated above)
                        var questionScore = Math.Round(questionDto.QuestionScore!.Value, 2, MidpointRounding.AwayFromZero);

                        // Normalize question name (already validated above)
                        var normalizedName = string.Join(' ', questionDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                        // Create question
                        var question = new QuizQuestion
                        {
                            QuizId = quiz,
                            Name = normalizedName,
                            Description = questionDto.Description,
                            IsMultipleAnswers = questionDto.IsMultipleAnswers,
                            ImageUrl = questionDto.ImageUrl,
                            QuestionScore = questionScore
                        };

                        await _uow.QuizQuestionRepository.CreateAsync(question);
                        await _uow.SaveChangesAsync();

                        // Calculate option score based on question type
                        var correctOptions = questionDto.Options.Where(o => o.IsCorrect).ToList();
                        decimal optionScore = 0m;

                        if (questionDto.IsMultipleAnswers)
                        {
                            // Multiple choice: divide score evenly among correct options
                            optionScore = Math.Round(questionScore / correctOptions.Count, 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            // Single choice: correct option gets full score
                            optionScore = questionScore;
                        }

                        // Create options for this question - auto-generate DisplayOrder
                        foreach (var opt in questionDto.Options)
                        {
                            try
                            {
                                // Calculate option score using helper method
                                var finalOptionScore = CalculateOptionScore(questionScore, questionDto.IsMultipleAnswers, opt.IsCorrect, correctOptions.Count);

                                var optionEntity = new QuizQuestionOption
                                {
                                    QuizQuestionId = question.Id,
                                    Name = opt.Name.Trim(),
                                    Description = opt.Description?.Trim(),
                                    IsCorrect = opt.IsCorrect,
                                    DisplayOrder = globalDisplayOrder++,
                                    OptionScore = finalOptionScore,
                                    Explanation = opt.Explanation?.Trim()
                                };

                                await _uow.QuizQuestionOptionRepository.CreateAsync(optionEntity);
                            }
                            catch (Exception ex)
                            {
                                throw new ValidationException($"Question #{questionIndex}, failed to create option '{opt.Name}': {ex.Message}", ex);
                            }
                        }

                        await _uow.SaveChangesAsync();
                    }
                    catch (Exception ex) when (ex is not ValidationException)
                    {
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(questionDto.Name, 50)}'): {ex.Message}", ex);
                    }
                }

                return quiz;
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new ValidationException($"Database error while creating quiz: {innerMessage}. Please check that all field mappings are correct.", dbEx);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Unexpected error while creating quiz: {ex.Message}", ex);
            }
        }

        public async Task<int> AddQuizToActivity(CreateActivityQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            // Validate quiz exists
            var quiz = await _uow.QuizRepository.GetByIdAsync(dto.QuizId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz with ID {dto.QuizId} not found.");

            // Validate activity exists and is not deleted
            var activity = await _uow.ActivityRepository.GetByIdAsync(dto.ActivityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {dto.ActivityId} not found.");

            // Validate activity type is Quiz (type = 2)
            if (activity.ActivityType != 2)
                throw new ValidationException("Activity type must be Quiz to add a quiz.");

            // Check if quiz is already assigned to this activity
            var alreadyExists = await _uow.ActivityQuizRepository
                .ExistsAsync(aq => aq.QuizId == dto.QuizId && aq.ActivityId == dto.ActivityId);

            if (alreadyExists)
                throw new ValidationException("This quiz is already assigned to this activity.");

            // Create ActivityQuiz using Quiz's name and description
            var activityQuiz = new ActivityQuiz
            {
                QuizId = dto.QuizId,
                ActivityId = dto.ActivityId,
                Name = quiz.Name ?? "Quiz",
                Description = quiz.Description
            };

            await _uow.ActivityQuizRepository.CreateAsync(activityQuiz);
            await _uow.SaveChangesAsync();

            return activityQuiz.Id;
        }

        public async Task<List<QuizOnlyDto>> GetQuizzesByActivityId(int activityId)
        {
            // Validate activity exists
            var activity = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

            // Get all quizzes assigned to this activity
            var quizzes = await _uow.ActivityQuizRepository
                .GetAllAsQueryable()
                .Where(aq => aq.ActivityId == activityId)
                .Select(aq => new QuizOnlyDto
                {
                    Id = aq.Quiz.Id,
                    Name = aq.Quiz.Name,
                    PassScoreCriteria = aq.Quiz.PassScoreCriteria,
                    TimelimitMinute = aq.Quiz.TimelimitMinute,
                    TotalScore = aq.Quiz.TotalScore,
                    Description = aq.Quiz.Description
                })
                .ToListAsync();

            return quizzes;
        }


        // Helper method to truncate text for error messages
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Calculate option score based on question type.
        /// For single choice: option score = question score (correct option gets full score)
        /// For multiple choice: option score = question score / number of correct options (divided equally)
        /// Incorrect options always get 0 score.
        /// </summary>
        private decimal CalculateOptionScore(decimal questionScore, bool isMultipleAnswers, bool isCorrectOption, int correctOptionsCount)
        {
            if (!isCorrectOption)
                return 0m;

            if (isMultipleAnswers)
            {
                // Multiple choice: divide score evenly among correct options
                return Math.Round(questionScore / correctOptionsCount, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                // Single choice: correct option gets full score
                return questionScore;
            }
        }
        public async Task<bool> RemoveQuizFromActivityAsync(int activityId, int quizId)
        {
            // 1. Check the business rule: Is this activity already in use?
            await CheckActivityUsageAsync(activityId);

            // 2. Find the link
            var link = await _uow.ActivityQuizRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(aq => aq.ActivityId == activityId && aq.QuizId == quizId);

            if (link == null)
            {
                throw new KeyNotFoundException($"Quiz {quizId} is not assigned to Activity {activityId}.");
            }

            // 3. Delete the link
            await _uow.ActivityQuizRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateQuizInActivityAsync(int activityId, UpdateActivityQuizDto dto)
        {
            // 1. Validate Activity exists and is a Quiz activity
            var activity = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

            if (activity.ActivityType != (int)ActivityType.Quiz)
                throw new InvalidOperationException("This activity is not a Quiz activity. Its type cannot be changed here.");

            // 2. Check the business rule: Is this activity already in use?
            await CheckActivityUsageAsync(activityId);

            // 3. Validate the new Quiz exists
            var newQuiz = await _uow.QuizRepository.GetByIdAsync(dto.NewQuizId);
            if (newQuiz == null)
            {
                throw new KeyNotFoundException($"The new Quiz with ID {dto.NewQuizId} was not found.");
            }

            // 4. Find the existing link. An activity can only have one quiz,
            // so we find by ActivityId.
            var existingLink = await _uow.ActivityQuizRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(aq => aq.ActivityId == activityId);

            if (existingLink == null)
            {
                throw new KeyNotFoundException($"No quiz is currently assigned to Activity {activityId}. Use the POST method to add one.");
            }

            // 5. Check if it's actually a change
            if (existingLink.QuizId == dto.NewQuizId)
            {
                return true; // No change needed
            }

            // 6. Update the link to point to the new quiz
            existingLink.QuizId = dto.NewQuizId;
            existingLink.Name = newQuiz.Name ?? "Quiz";
            existingLink.Description = newQuiz.Description;

            await _uow.ActivityQuizRepository.UpdateAsync(existingLink);
            await _uow.SaveChangesAsync();

            return true;
        }

        private async Task CheckActivityUsageAsync(int activityId)
        {
            // Find all SectionIDs this Activity is linked to
            var linkedSectionIds = _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.ActivityId == activityId)
                .Select(sa => sa.SectionId);

            // Check if any SectionRecord exists that points to one of those SectionIDs
            var isActivityInUse = await _uow.SectionRecordRepository
                .GetAllAsQueryable()
                .AnyAsync(sr => linkedSectionIds.Contains(sr.SectionId ?? -1));

            if (isActivityInUse)
            {
                throw new InvalidOperationException("This activity is already in use by trainees (has section records) and its quiz cannot be changed or removed.");

            }
        }
    }
}
