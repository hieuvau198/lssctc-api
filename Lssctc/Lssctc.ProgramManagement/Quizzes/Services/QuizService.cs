using ExcelDataReader;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
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

        public async Task<bool> DeleteQuizById(int id)
        {
            return await DeleteQuizById(id, instructorId: null);
        }

        public async Task<bool> DeleteQuizById(int id, int? instructorId)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            // If instructorId is provided and > 0, check if instructor is the author
            if (instructorId.HasValue && instructorId.Value > 0)
            {
                // Check if this instructor is the author of this quiz
                var isAuthor = await _uow.QuizAuthorRepository.ExistsAsync(qa => qa.QuizId == id && qa.InstructorId == instructorId.Value);
                if (!isAuthor)
                    return false; // Not the author, return false (simulating not found)
            }

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

            //delete all questions
            foreach (var question in questions)
            {
                await _uow.QuizQuestionRepository.DeleteAsync(question);
            }

            // Delete QuizAuthor records
            var quizAuthors = await _uow.QuizAuthorRepository.GetAllAsQueryable()
                .Where(qa => qa.QuizId == id)
                .ToListAsync();

            foreach (var quizAuthor in quizAuthors)
            {
                await _uow.QuizAuthorRepository.DeleteAsync(quizAuthor);
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
                .OrderByDescending(q => q.CreatedAt)
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

        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int pageIndex, int pageSize, int? instructorId, CancellationToken ct = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            // If instructorId is provided and > 0, filter quizzes by instructor
            IQueryable<Quiz> query = _uow.QuizRepository.GetAllAsQueryable();
            
            if (instructorId.HasValue && instructorId.Value > 0)
            {
                // Only get quizzes where this instructor is the author
                query = query.Where(q => q.QuizAuthors.Any(qa => qa.InstructorId == instructorId.Value));
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(q => q.CreatedAt)
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

        public async Task<QuizDto?> GetQuizById(int id, int? instructorId)
        {
            // If instructorId is provided and > 0, check if instructor is the author
            IQueryable<Quiz> query = _uow.QuizRepository.GetAllAsQueryable();
            
            if (instructorId.HasValue && instructorId.Value > 0)
            {
                // Only allow instructor to see quiz they created
                query = query.Where(q => q.Id == id && q.QuizAuthors.Any(qa => qa.InstructorId == instructorId.Value));
            }
            else
            {
                // If no instructor specified, just get by ID (for Admin)
                query = query.Where(q => q.Id == id);
            }

            var q = await query
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

        public async Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto)
        {
            return await CreateQuizWithQuestions(dto, instructorId: 0);
        }

        public async Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto, int instructorId)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            try
            {
                if (dto.Questions == null || dto.Questions.Count == 0)
                    throw new ValidationException("At least one question is required.");

                if (dto.Questions.Count > 100)
                    throw new ValidationException("A quiz cannot contain more than 100 questions.");

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

                    // Single choice question: ch? ???c có 1 option ??ng
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

                // Step 1: Create Quiz directly (instead of calling CreateQuiz method)
                var rawName = (dto.Name ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(rawName))
                    throw new ValidationException("Quiz name is required.");

                if (rawName.Length > 100)
                    throw new ValidationException("Quiz name must be at most 100 characters.");

                var normalizedQuizName = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                // TotalScore fixed to 10 (BR-37)
                var totalQuizScore = 10m;

                // Validate PassScoreCriteria: must be >0 and <= totalScore
                if (dto.PassScoreCriteria.HasValue)
                {
                    var pass = Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero);
                    if (pass <= 0m || pass > totalQuizScore)
                        throw new ValidationException($"PassScoreCriteria must be greater than 0 and less than or equal to {totalQuizScore}.");
                }

                var quiz = new Quiz
                {
                    Name = normalizedQuizName,
                    PassScoreCriteria = dto.PassScoreCriteria.HasValue ? Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero) : null,
                    TimelimitMinute = dto.TimelimitMinute,
                    Description = dto.Description,
                    TotalScore = totalQuizScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _uow.QuizRepository.CreateAsync(quiz);
                await _uow.SaveChangesAsync();
                var quizId = quiz.Id;

                // Step 1b: Save QuizAuthor if instructorId is provided (> 0)
                if (instructorId > 0)
                {
                    // Verify instructor exists
                    var instructor = await _uow.InstructorRepository.GetByIdAsync(instructorId);
                    if (instructor == null)
                        throw new ValidationException($"Instructor with ID {instructorId} not found.");

                    var quizAuthor = new QuizAuthor
                    {
                        InstructorId = instructorId,
                        QuizId = quizId
                    };

                    await _uow.QuizAuthorRepository.CreateAsync(quizAuthor);
                    await _uow.SaveChangesAsync();
                }

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
                            QuizId = quizId,
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
                                    Name = opt.Name,
                                    Description = opt.Description,
                                    IsCorrect = opt.IsCorrect,
                                    DisplayOrder = globalDisplayOrder++,
                                    OptionScore = finalOptionScore,
                                    Explanation = opt.Explanation
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

                return quizId;
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

        public async Task<int> CreateQuizFromExcel(ImportQuizExcelDto dto, int instructorId)
        {
            // 1. Validate file extension
            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                throw new ValidationException("Only Excel files (.xlsx, .xls) are allowed.");

            var createQuizDto = new CreateQuizWithQuestionsDto
            {
                Name = dto.Name,
                PassScoreCriteria = dto.PassScoreCriteria,
                TimelimitMinute = dto.TimelimitMinute,
                Description = dto.Description,
                Questions = new List<CreateQuizQuestionWithOptionsDto>()
            };

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = dto.File.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                if (result.Tables.Count == 0)
                    throw new ValidationException("The Excel file is empty.");

                var dataTable = result.Tables[0];

                // [FIX 1] Preliminary check for column count
                if (dataTable.Columns.Count < 7)
                    throw new ValidationException("The Excel file is missing columns. Please ensure there are at least 7 columns according to the template.");

                var questionMap = new Dictionary<string, CreateQuizQuestionWithOptionsDto>();

                int rowIndex = 1; // Used to report which row is invalid
                foreach (DataRow row in dataTable.Rows)
                {
                    rowIndex++;
                    var qName = row[0]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(qName)) continue;

                    // [FIX 2] More robust Score handling (Accepts both 2.5 and 2,5)
                    var scoreRaw = row[1]?.ToString()?.Trim();
                    decimal qScore = 0;

                    if (!string.IsNullOrEmpty(scoreRaw))
                    {
                        // Replace comma with dot to normalize to InvariantCulture
                        var normalizedScore = scoreRaw.Replace(",", ".");
                        if (!decimal.TryParse(normalizedScore, NumberStyles.Any, CultureInfo.InvariantCulture, out qScore))
                        {
                            // If parsing fails, qScore will be 0 -> Will be blocked at the final total score validation step
                            // Or you can throw an error right here if you want to be strict:
                            // throw new ValidationException($"Row {rowIndex}: Score '{scoreRaw}' is invalid.");
                        }
                    }

                    var isMultiRaw = row[2]?.ToString()?.ToLower();
                    bool isMulti = isMultiRaw == "true" || isMultiRaw == "1" || isMultiRaw == "yes";

                    var qDesc = row[3]?.ToString();

                    var optName = row[4]?.ToString()?.Trim();
                    // If there is a question name but no option name -> Skip
                    if (string.IsNullOrEmpty(optName)) continue;

                    var isCorrectRaw = row[5]?.ToString()?.ToLower();
                    bool isCorrect = isCorrectRaw == "true" || isCorrectRaw == "1" || isCorrectRaw == "yes";

                    var optExplain = row[6]?.ToString();

                    if (!questionMap.ContainsKey(qName))
                    {
                        var newQuestion = new CreateQuizQuestionWithOptionsDto
                        {
                            Name = qName,
                            QuestionScore = qScore,
                            IsMultipleAnswers = isMulti,
                            Description = qDesc,
                            Options = new List<CreateQuizQuestionOptionDto>()
                        };
                        questionMap.Add(qName, newQuestion);
                        createQuizDto.Questions.Add(newQuestion);
                    }

                    // Additional logic: If subsequent rows of the same question have different scores, 
                    // current code will keep the score of the first row. (Safe)

                    questionMap[qName].Options.Add(new CreateQuizQuestionOptionDto
                    {
                        Name = optName,
                        IsCorrect = isCorrect,
                        Explanation = optExplain
                    });
                }
            }

            if (createQuizDto.Questions.Count == 0)
                throw new ValidationException("No valid data found in the Excel file.");

            var totalScore = createQuizDto.Questions.Sum(q => q.QuestionScore ?? 0);

            // [FIX 3] Log the actual total score for easier debugging if errors persist
            if (Math.Abs(totalScore - 10m) > 0.0001m)
            {
                throw new ValidationException($"Calculated total score is {totalScore} (Required = 10). Please check the Score column (Column B).");
            }

            return await CreateQuizWithQuestions(createQuizDto, instructorId);
        }

        public async Task<int> AddQuizToActivity(CreateActivityQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var quiz = await _uow.QuizRepository.GetByIdAsync(dto.QuizId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz with ID {dto.QuizId} not found.");

            var activity = await _uow.ActivityRepository.GetByIdAsync(dto.ActivityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {dto.ActivityId} not found.");

            if (activity.ActivityType != 2)
                throw new ValidationException("Activity type must be Quiz to add a quiz.");

            var alreadyExists = await _uow.ActivityQuizRepository
                .ExistsAsync(aq => aq.QuizId == dto.QuizId && aq.ActivityId == dto.ActivityId);

            if (alreadyExists)
                throw new ValidationException("This quiz is already assigned to this activity.");

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
            var activity = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted == true)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

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

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        private decimal CalculateOptionScore(decimal questionScore, bool isMultipleAnswers, bool isCorrectOption, int correctOptionsCount)
        {
            if (!isCorrectOption)
                return 0m;

            if (isMultipleAnswers)
            {
                return Math.Round(questionScore / correctOptionsCount, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                return questionScore;
            }
        }

        public async Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto)
        {
            return await UpdateQuizWithQuestionsAsync(quizId, dto, instructorId: null);
        }

        public async Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto, int? instructorId)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            try
            {
                var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
                if (quiz == null)
                    throw new KeyNotFoundException($"Quiz with ID {quizId} not found.");

                // If instructorId is provided and > 0, check if instructor is the author
                if (instructorId.HasValue && instructorId.Value > 0)
                {
                    // Check if this instructor is the author of this quiz
                    var isAuthor = await _uow.QuizAuthorRepository.ExistsAsync(qa => qa.QuizId == quizId && qa.InstructorId == instructorId.Value);
                    if (!isAuthor)
                        throw new KeyNotFoundException($"Quiz with ID {quizId} not found.");
                }

                await CheckQuizUsageAsync(quizId);

                if (dto.Questions == null || dto.Questions.Count == 0)
                    throw new ValidationException("At least one question is required.");

                if (dto.Questions.Count > 100)
                    throw new ValidationException("A quiz cannot contain more than 100 questions.");

                var totalScore = 0m;
                int questionIndex = 0;

                foreach (var questionDto in dto.Questions)
                {
                    questionIndex++;

                    if (string.IsNullOrWhiteSpace(questionDto.Name))
                        throw new ValidationException($"Question #{questionIndex}: Name cannot be empty.");

                    var normalizedName = string.Join(' ', questionDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

                    if (normalizedName.Length > 500)
                    {
                        throw new ValidationException(
                            $"Question #{questionIndex}: Name exceeds 500 characters " +
                            $"(actual: {normalizedName.Length} chars after removing extra spaces). " +
                            $"Please shorten the question text.");
                    }

                    if (questionDto.Options == null || questionDto.Options.Count == 0)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least one option.");

                    if (questionDto.Options.Count < 2)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least 2 options.");

                    if (questionDto.Options.Count > 20)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Cannot have more than 20 answer options.");

                    var correctCount = questionDto.Options.Count(o => o.IsCorrect);
                    if (correctCount == 0)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Must have at least one correct option.");

                    if (!questionDto.IsMultipleAnswers && correctCount > 1)
                        throw new ValidationException($"Question #{questionIndex} ('{TruncateText(normalizedName, 50)}'): Single choice question cannot have more than one correct option.");

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

                // Step 1: Update quiz properties if provided
                if (!string.IsNullOrEmpty(dto.Name))
                {
                    var rawName = dto.Name.Trim();
                    if (string.IsNullOrWhiteSpace(rawName))
                        throw new ValidationException("Quiz name cannot be empty.");
                    if (rawName.Length > 100)
                        throw new ValidationException("Quiz name must be at most 100 characters.");

                    quiz.Name = string.Join(' ', rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }

                if (dto.PassScoreCriteria.HasValue)
                {
                    var pass = Math.Round(dto.PassScoreCriteria.Value, 2, MidpointRounding.AwayFromZero);
                    if (pass <= 0m || pass > 10m)
                        throw new ValidationException("PassScoreCriteria must be greater than 0 and less than or equal to 10.");
                    quiz.PassScoreCriteria = pass;
                }

                if (dto.TimelimitMinute.HasValue)
                {
                    if (dto.TimelimitMinute < 1 || dto.TimelimitMinute > 600)
                        throw new ValidationException("TimelimitMinute must be between 1 and 600 minutes.");
                    quiz.TimelimitMinute = dto.TimelimitMinute;
                }

                if (dto.Description != null)
                {
                    if (dto.Description.Length > 2000)
                        throw new ValidationException("Description must be at most 2000 characters.");
                    quiz.Description = dto.Description;
                }

                quiz.UpdatedAt = DateTime.UtcNow;
                await _uow.QuizRepository.UpdateAsync(quiz);
                await _uow.SaveChangesAsync();

                // Step 2: Delete all existing questions and options for this quiz
                var existingQuestions = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                    .Where(q => q.QuizId == quizId)
                    .ToListAsync();

                foreach (var question in existingQuestions)
                {
                    var options = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                        .Where(o => o.QuizQuestionId == question.Id)
                        .ToListAsync();

                    foreach (var option in options)
                    {
                        await _uow.QuizQuestionOptionRepository.DeleteAsync(option);
                    }

                    await _uow.QuizQuestionRepository.DeleteAsync(question);
                }

                await _uow.SaveChangesAsync();

                // Step 3: Create new Questions with Options
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
                            QuizId = quizId,
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
                                    Name = opt.Name,
                                    Description = opt.Description,
                                    IsCorrect = opt.IsCorrect,
                                    DisplayOrder = globalDisplayOrder++,
                                    OptionScore = finalOptionScore,
                                    Explanation = opt.Explanation
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

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new ValidationException($"Database error while updating quiz: {innerMessage}. Please check that all field mappings are correct.", dbEx);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Unexpected error while updating quiz: {ex.Message}", ex);
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

        private async Task CheckQuizUsageAsync(int quizId)
        {
            // Check if quiz has any attempts recorded
            var hasAttempts = await _uow.QuizAttemptRepository.ExistsAsync(a => a.QuizId == quizId);
            if (hasAttempts)
                throw new ValidationException("Cannot update quiz that has attempts recorded. Quiz is already in use by trainees.");
        }
    }
}
