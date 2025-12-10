using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivitySessionService _sessionService;
        private readonly IQuizValidator _validator;
        private readonly IQuizExcelProcessor _excelProcessor;

        public QuizService(
            IUnitOfWork uow,
            IActivitySessionService sessionService,
            IQuizValidator validator,
            IQuizExcelProcessor excelProcessor)
        {
            _uow = uow;
            _sessionService = sessionService;
            _validator = validator;
            _excelProcessor = excelProcessor;
        }

        #region DELETE
        public async Task<bool> DeleteQuizById(int id) => await DeleteQuizById(id, null);

        public async Task<bool> DeleteQuizById(int id, int? instructorId)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);
            if (entity == null) return false;

            if (instructorId.HasValue && instructorId > 0)
            {
                if (!await _uow.QuizAuthorRepository.ExistsAsync(qa => qa.QuizId == id && qa.InstructorId == instructorId.Value))
                    return false;
            }

            if (await _uow.ActivityQuizRepository.ExistsAsync(aq => aq.QuizId == id))
                throw new ValidationException("Cannot delete quiz used in an activity.");

            if (await _uow.QuizAttemptRepository.ExistsAsync(a => a.QuizId == id))
                throw new ValidationException("Cannot delete quiz that has attempts.");

            // Cleanup related data
            var questions = await _uow.QuizQuestionRepository.GetAllAsQueryable().Where(q => q.QuizId == id).ToListAsync();
            foreach (var q in questions)
            {
                var options = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().Where(o => o.QuizQuestionId == q.Id).ToListAsync();
                foreach (var o in options) await _uow.QuizQuestionOptionRepository.DeleteAsync(o);
                await _uow.QuizQuestionRepository.DeleteAsync(q);
            }

            var authors = await _uow.QuizAuthorRepository.GetAllAsQueryable().Where(qa => qa.QuizId == id).ToListAsync();
            foreach (var a in authors) await _uow.QuizAuthorRepository.DeleteAsync(a);

            await _uow.QuizRepository.DeleteAsync(entity);
            await _uow.SaveChangesAsync();
            return true;
        }
        #endregion

        #region GET
        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int page, int size, CancellationToken ct = default)
            => await GetQuizzes(page, size, null, null, null, null, ct);

        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int page, int size, int? instructorId, CancellationToken ct = default)
            => await GetQuizzes(page, size, instructorId, null, null, null, ct);

        public async Task<PagedResult<QuizOnlyDto>> GetQuizzes(int page, int size, int? instructorId, string? search, string? sort, string? dir, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var query = _uow.QuizRepository.GetAllAsQueryable();
            if (instructorId > 0)
                query = query.Where(q => q.QuizAuthors.Any(qa => qa.InstructorId == instructorId.Value));

            query = query.ApplySearch(search).ApplySort(sort, dir);

            return await query.Select(q => new QuizOnlyDto
            {
                Id = q.Id,
                Name = q.Name,
                PassScoreCriteria = q.PassScoreCriteria,
                TimelimitMinute = q.TimelimitMinute,
                TotalScore = q.TotalScore,
                Description = q.Description
            }).ToPagedResultAsync(page, size);
        }

        public async Task<QuizDto?> GetQuizById(int id) => await GetQuizById(id, null);

        public async Task<QuizDto?> GetQuizById(int id, int? instructorId)
        {
            var query = _uow.QuizRepository.GetAllAsQueryable();
            if (instructorId > 0)
                query = query.Where(q => q.Id == id && q.QuizAuthors.Any(qa => qa.InstructorId == instructorId.Value));
            else
                query = query.Where(q => q.Id == id);

            return await query.Select(x => new QuizDto
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
            }).FirstOrDefaultAsync();
        }

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default)
        {
            return await _uow.QuizRepository.GetAllAsQueryable().Where(x => x.Id == quizId)
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
                        }).OrderBy(o => o.DisplayOrder).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync(ct);
        }

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTraineeByActivityIdAsync(int activityId, int? traineeId = null, CancellationToken ct = default)
        {
            if (traineeId.HasValue)
            {
                var record = await _uow.ActivityRecordRepository.GetAllAsQueryable().AsNoTracking()
                    .Where(ar => ar.ActivityId == activityId && ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId.Value)
                    .Select(ar => new { ar.SectionRecord.LearningProgress.Enrollment.ClassId }).FirstOrDefaultAsync(ct);
                if (record != null) await _sessionService.CheckActivityAccess(record.ClassId, activityId);
            }

            var aq = await _uow.ActivityQuizRepository.GetAllAsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.ActivityId == activityId, ct);
            return aq == null ? null : await GetQuizDetailForTrainee(aq.QuizId, ct);
        }

        public async Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(int sectionQuizId, CancellationToken ct = default)
        {
            var aq = await _uow.ActivityQuizRepository.GetByIdAsync(sectionQuizId);
            return aq == null ? null : await GetQuizDetailForTrainee(aq.QuizId, ct);
        }

        public async Task<TraineeQuizResponseDto> GetQuizForTraineeByRecordIdAsync(int activityRecordId, CancellationToken ct = default)
        {
            // 1. Lấy thông tin ActivityRecord để tìm ClassId và ActivityId
            var record = await _uow.ActivityRecordRepository.GetAllAsQueryable()
                .AsNoTracking()
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId, ct);

            if (record == null)
                throw new KeyNotFoundException("Activity Record not found.");

            var activityId = record.ActivityId.GetValueOrDefault();
            var classId = record.SectionRecord.LearningProgress.Enrollment.ClassId;

            // 2. Tìm Session của Activity trong Class này
            // FIX: Bỏ điều kiện IsActive == true để lấy được session ngay cả khi nó bị disable
            var session = await _uow.ActivitySessionRepository.GetAllAsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ClassId == classId && s.ActivityId == activityId, ct);

            // 3. Xác định trạng thái Session
            var status = new QuizSessionStatusDto
            {
                IsOpen = true,
                Message = "Available"
            };

            if (session != null)
            {
                status.StartTime = session.StartTime;
                status.EndTime = session.EndTime;
                var now = DateTime.Now;

                // FIX: Kiểm tra IsActive trước. Nếu False thì đóng session.
                // Lưu ý: IsActive có thể là bool? nên cần check IsActive != true hoặc (session.IsActive ?? false) == false
                if (session.IsActive != true)
                {
                    status.IsOpen = false;
                    status.Message = "Session is inactive";
                }
                else if (status.StartTime.HasValue && now < status.StartTime.Value)
                {
                    status.IsOpen = false;
                    status.Message = "Not started yet";
                }
                else if (status.EndTime.HasValue && now > status.EndTime.Value)
                {
                    status.IsOpen = false;
                    status.Message = "Expired";
                }
            }

            // 4. Lấy chi tiết Quiz (Giữ nguyên logic cũ)
            var quizDetail = await GetQuizDetailForTraineeByActivityIdAsync(activityId, null, ct);

            if (quizDetail == null)
                throw new KeyNotFoundException("Quiz content not found for this activity.");

            return new TraineeQuizResponseDto
            {
                Quiz = quizDetail,
                SessionStatus = status
            };
        }

        #endregion

        #region CREATE & UPDATE
        public async Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto) => await CreateQuizWithQuestions(dto, 0);

        public async Task<int> CreateQuizWithQuestions(CreateQuizWithQuestionsDto dto, int instructorId)
        {
            _validator.ValidateCreateQuiz(dto);

            var normName = string.Join(' ', dto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var quiz = new Quiz
            {
                Name = normName,
                PassScoreCriteria = dto.PassScoreCriteria.HasValue ? Math.Round(dto.PassScoreCriteria.Value, 2) : null,
                TimelimitMinute = dto.TimelimitMinute,
                Description = dto.Description,
                TotalScore = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.QuizRepository.CreateAsync(quiz);
            await _uow.SaveChangesAsync();

            if (instructorId > 0)
            {
                if (await _uow.InstructorRepository.GetByIdAsync(instructorId) == null)
                    throw new ValidationException($"Instructor {instructorId} not found.");
                await _uow.QuizAuthorRepository.CreateAsync(new QuizAuthor { InstructorId = instructorId, QuizId = quiz.Id });
                await _uow.SaveChangesAsync();
            }

            await SaveQuestions(quiz.Id, dto.Questions);
            return quiz.Id;
        }

        public async Task<int> CreateQuizFromExcel(ImportQuizExcelDto dto, int instructorId)
        {
            var createDto = _excelProcessor.ParseExcel(dto);
            return await CreateQuizWithQuestions(createDto, instructorId);
        }

        public async Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto)
            => await UpdateQuizWithQuestionsAsync(quizId, dto, null);

        public async Task<bool> UpdateQuizWithQuestionsAsync(int quizId, UpdateQuizWithQuestionsDto dto, int? instructorId)
        {
            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            if (instructorId > 0 && !await _uow.QuizAuthorRepository.ExistsAsync(qa => qa.QuizId == quizId && qa.InstructorId == instructorId.Value))
                throw new KeyNotFoundException($"Quiz {quizId} not found or access denied.");

            if (await _uow.QuizAttemptRepository.ExistsAsync(a => a.QuizId == quizId))
                throw new ValidationException("Cannot update used quiz.");

            _validator.ValidateUpdateQuiz(dto);

            // Update Quiz Info
            if (!string.IsNullOrEmpty(dto.Name)) quiz.Name = string.Join(' ', dto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (dto.PassScoreCriteria.HasValue) quiz.PassScoreCriteria = Math.Round(dto.PassScoreCriteria.Value, 2);
            if (dto.TimelimitMinute.HasValue) quiz.TimelimitMinute = dto.TimelimitMinute;
            if (dto.Description != null) quiz.Description = dto.Description;
            quiz.UpdatedAt = DateTime.UtcNow;

            await _uow.QuizRepository.UpdateAsync(quiz);

            // Clear old questions
            var oldQs = await _uow.QuizQuestionRepository.GetAllAsQueryable().Where(q => q.QuizId == quizId).ToListAsync();
            foreach (var q in oldQs)
            {
                var oldOpts = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().Where(o => o.QuizQuestionId == q.Id).ToListAsync();
                foreach (var o in oldOpts) await _uow.QuizQuestionOptionRepository.DeleteAsync(o);
                await _uow.QuizQuestionRepository.DeleteAsync(q);
            }
            await _uow.SaveChangesAsync();

            // Convert UpdateDto to CreateDto format to reuse saving logic if possible, or just copy logic
            // Since DTOs are different types, we duplicate the save loop or map it. 
            // Mapping is cleaner.
            var questionDtos = dto.Questions.Select(q => new CreateQuizQuestionWithOptionsDto
            {
                Name = q.Name,
                Description = q.Description,
                ImageUrl = q.ImageUrl,
                IsMultipleAnswers = q.IsMultipleAnswers,
                QuestionScore = q.QuestionScore,
                Options = q.Options.Select(o => new CreateQuizQuestionOptionDto { Name = o.Name, Description = o.Description, IsCorrect = o.IsCorrect, Explanation = o.Explanation }).ToList()
            }).ToList();

            await SaveQuestions(quizId, questionDtos);
            return true;
        }
        #endregion

        #region ACTIVITY
        public async Task<int> AddQuizToActivity(CreateActivityQuizDto dto)
        {
            if (dto == null) throw new ValidationException("Body required.");
            var quiz = await _uow.QuizRepository.GetByIdAsync(dto.QuizId) ?? throw new KeyNotFoundException($"Quiz {dto.QuizId} not found.");
            var act = await _uow.ActivityRepository.GetByIdAsync(dto.ActivityId);

            if (act == null || act.IsDeleted == true) throw new KeyNotFoundException($"Activity {dto.ActivityId} not found.");
            if (act.ActivityType != 2) throw new ValidationException("Activity must be Quiz type.");
            if (await _uow.ActivityQuizRepository.ExistsAsync(aq => aq.QuizId == dto.QuizId && aq.ActivityId == dto.ActivityId))
                throw new ValidationException("Already assigned.");

            var link = new ActivityQuiz { QuizId = dto.QuizId, ActivityId = dto.ActivityId, Name = quiz.Name ?? "Quiz", Description = quiz.Description };
            await _uow.ActivityQuizRepository.CreateAsync(link);
            await _uow.SaveChangesAsync();
            return link.Id;
        }

        public async Task<List<QuizOnlyDto>> GetQuizzesByActivityId(int activityId)
        {
            var act = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (act == null || act.IsDeleted == true) throw new KeyNotFoundException($"Activity {activityId} not found.");

            return await _uow.ActivityQuizRepository.GetAllAsQueryable().Where(aq => aq.ActivityId == activityId)
                .Select(aq => new QuizOnlyDto
                {
                    Id = aq.Quiz.Id,
                    Name = aq.Quiz.Name,
                    PassScoreCriteria = aq.Quiz.PassScoreCriteria,
                    TimelimitMinute = aq.Quiz.TimelimitMinute,
                    TotalScore = aq.Quiz.TotalScore,
                    Description = aq.Quiz.Description
                }).ToListAsync();
        }

        public async Task<bool> RemoveQuizFromActivityAsync(int activityId, int quizId)
        {
            await CheckActivityUsage(activityId);
            var link = await _uow.ActivityQuizRepository.GetAllAsQueryable().FirstOrDefaultAsync(aq => aq.ActivityId == activityId && aq.QuizId == quizId);
            if (link == null) throw new KeyNotFoundException("Link not found.");

            await _uow.ActivityQuizRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuizInActivityAsync(int activityId, UpdateActivityQuizDto dto)
        {
            var act = await _uow.ActivityRepository.GetByIdAsync(activityId);
            if (act == null || act.IsDeleted == true) throw new KeyNotFoundException($"Activity {activityId} not found.");
            if (act.ActivityType != (int)ActivityType.Quiz) throw new InvalidOperationException("Not a Quiz activity.");

            await CheckActivityUsage(activityId);
            var newQuiz = await _uow.QuizRepository.GetByIdAsync(dto.NewQuizId) ?? throw new KeyNotFoundException($"New Quiz {dto.NewQuizId} not found.");
            var link = await _uow.ActivityQuizRepository.GetAllAsQueryable().FirstOrDefaultAsync(aq => aq.ActivityId == activityId)
                       ?? throw new KeyNotFoundException($"No quiz currently assigned to Activity {activityId}.");

            if (link.QuizId == dto.NewQuizId) return true;

            link.QuizId = dto.NewQuizId;
            link.Name = newQuiz.Name ?? "Quiz";
            link.Description = newQuiz.Description;
            await _uow.ActivityQuizRepository.UpdateAsync(link);
            await _uow.SaveChangesAsync();
            return true;
        }
        #endregion

        #region PRIVATE HELPERS
        private async Task SaveQuestions(int quizId, List<CreateQuizQuestionWithOptionsDto> questions)
        {
            int globalOrder = 1;
            if (await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().AnyAsync())
                globalOrder = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable().MaxAsync(x => x.DisplayOrder ?? 0) + 1;

            foreach (var qDto in questions)
            {
                var qScore = Math.Round(qDto.QuestionScore!.Value, 2);
                var normName = string.Join(' ', qDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                var question = new QuizQuestion
                {
                    QuizId = quizId,
                    Name = normName,
                    Description = qDto.Description,
                    IsMultipleAnswers = qDto.IsMultipleAnswers,
                    ImageUrl = qDto.ImageUrl,
                    QuestionScore = qScore
                };
                await _uow.QuizQuestionRepository.CreateAsync(question);
                await _uow.SaveChangesAsync();

                var correctCount = qDto.Options.Count(o => o.IsCorrect);
                var optScore = qDto.IsMultipleAnswers ? Math.Round(qScore / correctCount, 2) : qScore;

                foreach (var opt in qDto.Options)
                {
                    await _uow.QuizQuestionOptionRepository.CreateAsync(new QuizQuestionOption
                    {
                        QuizQuestionId = question.Id,
                        Name = opt.Name,
                        Description = opt.Description,
                        IsCorrect = opt.IsCorrect,
                        DisplayOrder = globalOrder++,
                        Explanation = opt.Explanation,
                        OptionScore = opt.IsCorrect ? optScore : 0
                    });
                }
                await _uow.SaveChangesAsync();
            }
        }

        private async Task CheckActivityUsage(int activityId)
        {
            var sectionIds = _uow.SectionActivityRepository.GetAllAsQueryable().Where(sa => sa.ActivityId == activityId).Select(sa => sa.SectionId);
            if (await _uow.SectionRecordRepository.GetAllAsQueryable().AnyAsync(sr => sectionIds.Contains(sr.SectionId ?? -1)))
                throw new InvalidOperationException("Activity is in use by trainees.");
        }
        #endregion
    }
}