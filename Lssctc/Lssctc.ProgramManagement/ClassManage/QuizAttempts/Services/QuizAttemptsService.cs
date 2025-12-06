using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.QuizAttempts.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services
{
    public class QuizAttemptsService : IQuizAttemptsService
    {
        private readonly IUnitOfWork _uow;
        private readonly ProgressHelper _progressHelper;
        private readonly IActivitySessionService _sessionService; 

        public QuizAttemptsService(IUnitOfWork uow, IActivitySessionService sessionService) // ADDED injection
        {
            _uow = uow;
            _progressHelper = new ProgressHelper(uow);
            _sessionService = sessionService; 
        }

        #region Gets
        public async Task<IEnumerable<QuizAttemptDto>> GetQuizAttemptsAsync(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to view these attempts.");

            var attempts = await _uow.QuizAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(qa => qa.QuizAttemptQuestions)
                    .ThenInclude(qaq => qaq.QuizAttemptAnswers)
                .Where(qa => qa.ActivityRecordId == activityRecordId)
                .OrderByDescending(qa => qa.AttemptOrder)
                .ToListAsync();

            return attempts.Select(MapToDto);
        }

        public async Task<QuizAttemptDto?> GetLatestQuizAttemptAsync(int traineeId, int activityRecordId)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");

            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to view this attempt.");

            var attempt = await _uow.QuizAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(qa => qa.QuizAttemptQuestions)
                    .ThenInclude(qaq => qaq.QuizAttemptAnswers)
                .Where(qa => qa.ActivityRecordId == activityRecordId && qa.IsCurrent)
                .FirstOrDefaultAsync();

            return attempt == null ? null : MapToDto(attempt);
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetLatestAttemptsForActivityAsync(int activityId)
        {
            var activityRecordIds = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Where(ar => ar.ActivityId == activityId)
                .Select(ar => ar.Id)
                .ToListAsync();

            if (!activityRecordIds.Any())
                return new List<QuizAttemptDto>();

            var attempts = await _uow.QuizAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(qa => qa.QuizAttemptQuestions)
                    .ThenInclude(qaq => qaq.QuizAttemptAnswers)
                .Where(qa => activityRecordIds.Contains(qa.ActivityRecordId) && qa.IsCurrent)
                .ToListAsync();

            return attempts.Select(MapToDto);
        }

        #endregion

        #region Submits
        public async Task<QuizAttemptDto> SubmitQuizAttemptAsync(int traineeId, SubmitQuizDto dto)
        {
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Include(ar => ar.SectionRecord.LearningProgress)
                .FirstOrDefaultAsync(ar => ar.Id == dto.ActivityRecordId);

            if (activityRecord == null)
                throw new KeyNotFoundException("Activity record not found.");
            if (activityRecord.SectionRecord.LearningProgress.Enrollment.TraineeId != traineeId)
                throw new UnauthorizedAccessException("You are not authorized to submit this attempt.");
            if (activityRecord.ActivityType != (int)ActivityType.Quiz)
                throw new InvalidOperationException("This activity is not a quiz.");
            await _sessionService.CheckActivityAccess(
                activityRecord.SectionRecord.LearningProgress.Enrollment.ClassId,
                activityRecord.ActivityId.Value);

            var activityQuiz = await _uow.ActivityQuizRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(aq => aq.Quiz)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.QuizQuestionOptions)
                .FirstOrDefaultAsync(aq => aq.ActivityId == activityRecord.ActivityId);

            if (activityQuiz == null || activityQuiz.Quiz == null)
                throw new InvalidOperationException("Could not find the quiz associated with this activity.");

            var quizTemplate = activityQuiz.Quiz;
            var questionTemplateMap = quizTemplate.QuizQuestions.ToDictionary(q => q.Id);
            var optionsTemplateMap = quizTemplate.QuizQuestions
                .SelectMany(q => q.QuizQuestionOptions)
                .ToDictionary(o => o.Id);

            var oldAttempts = await _uow.QuizAttemptRepository
                .GetAllAsQueryable()
                .Where(qa => qa.ActivityRecordId == dto.ActivityRecordId)
                .ToListAsync();

            int attemptOrder = 1;
            if (oldAttempts.Any())
            {
                attemptOrder = oldAttempts.Max(qa => qa.AttemptOrder ?? 0) + 1;
                foreach (var oldAttempt in oldAttempts)
                {
                    oldAttempt.IsCurrent = false;
                    await _uow.QuizAttemptRepository.UpdateAsync(oldAttempt);
                }
            }

            var newAttempt = new QuizAttempt
            {
                ActivityRecordId = dto.ActivityRecordId,
                QuizId = quizTemplate.Id,
                Name = $"{quizTemplate.Name} - Attempt {attemptOrder}",
                QuizAttemptDate = DateTime.UtcNow,
                Status = (int)QuizAttemptStatusEnum.Submitted,
                AttemptOrder = attemptOrder,
                IsCurrent = true,
                QuizAttemptQuestions = new List<QuizAttemptQuestion>()
            };

            decimal totalScore = 0;
            decimal maxScore = 0;

            foreach (var submittedQuestion in dto.Answers)
            {
                if (!questionTemplateMap.TryGetValue(submittedQuestion.QuestionId, out var questionTemplate))
                    continue;

                maxScore += questionTemplate.QuestionScore ?? 0;
                bool isQuestionCorrect = true;

                var correctOptionIds = questionTemplate.QuizQuestionOptions
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToHashSet();

                var newAttemptQuestion = new QuizAttemptQuestion
                {
                    // QuizAttempt = newAttempt, // <--- REMOVE THIS LINE
                    QuestionId = questionTemplate.Id,
                    QuestionScore = questionTemplate.QuestionScore,
                    IsMultipleAnswers = questionTemplate.IsMultipleAnswers,
                    Name = questionTemplate.Name,
                    Description = questionTemplate.Description,
                    QuizAttemptAnswers = new List<QuizAttemptAnswer>()
                };

                if (submittedQuestion.SelectedOptionIds.Count != correctOptionIds.Count)
                {
                    isQuestionCorrect = false;
                }

                foreach (var selectedOptionId in submittedQuestion.SelectedOptionIds)
                {
                    if (!optionsTemplateMap.TryGetValue(selectedOptionId, out var optionTemplate))
                        continue;

                    bool isThisAnswerCorrect = correctOptionIds.Contains(selectedOptionId);
                    if (!isThisAnswerCorrect)
                    {
                        isQuestionCorrect = false;
                    }

                    newAttemptQuestion.QuizAttemptAnswers.Add(new QuizAttemptAnswer
                    {
                        // QuizAttemptQuestion = newAttemptQuestion, // <--- REMOVE THIS LINE
                        QuizOptionId = selectedOptionId,
                        IsCorrect = isThisAnswerCorrect,
                        Name = optionTemplate.Name,
                        Description = optionTemplate.Description
                    });
                }

                if (isQuestionCorrect)
                {
                    newAttemptQuestion.IsCorrect = true;
                    newAttemptQuestion.AttemptScore = newAttemptQuestion.QuestionScore;
                    totalScore += newAttemptQuestion.AttemptScore ?? 0;
                }
                else
                {
                    newAttemptQuestion.IsCorrect = false;
                    newAttemptQuestion.AttemptScore = 0;
                }

                newAttempt.QuizAttemptQuestions.Add(newAttemptQuestion);
            }

            newAttempt.AttemptScore = totalScore;
            newAttempt.MaxScore = maxScore;
            newAttempt.IsPass = maxScore > 0 ? (totalScore / maxScore) >= 0.5m : true;

            await _uow.QuizAttemptRepository.CreateAsync(newAttempt);
            await _uow.SaveChangesAsync();

            var activityRecordToUpdate = activityRecord;
            var sectionRecordToUpdate = activityRecord.SectionRecord;
            var learningProgressToUpdate = activityRecord.SectionRecord.LearningProgress;

            await _progressHelper.UpdateActivityRecordProgressAsync(traineeId, activityRecordToUpdate.Id);
            await _progressHelper.UpdateSectionRecordProgressAsync(traineeId, sectionRecordToUpdate.Id);
            await _progressHelper.UpdateLearningProgressProgressAsync(traineeId, learningProgressToUpdate.Id);

            return MapToDto(newAttempt);
        }

        #endregion

        #region Mapping Methods
        private static QuizAttemptDto MapToDto(QuizAttempt qa)
        {
            return new QuizAttemptDto
            {
                Id = qa.Id,
                ActivityRecordId = qa.ActivityRecordId,
                QuizId = qa.QuizId,
                Name = qa.Name,
                AttemptScore = qa.AttemptScore,
                MaxScore = qa.MaxScore,
                QuizAttemptDate = qa.QuizAttemptDate,
                Status = Enum.GetName(typeof(QuizAttemptStatusEnum), qa.Status) ?? "Unknown",
                AttemptOrder = qa.AttemptOrder,
                IsPass = qa.IsPass,
                IsCurrent = qa.IsCurrent,
                QuizAttemptQuestions = qa.QuizAttemptQuestions.Select(MapToQuestionDto).ToList()
            };
        }

        private static QuizAttemptQuestionDto MapToQuestionDto(QuizAttemptQuestion qaq)
        {
            return new QuizAttemptQuestionDto
            {
                Id = qaq.Id,
                QuestionId = qaq.QuestionId,
                AttemptScore = qaq.AttemptScore,
                QuestionScore = qaq.QuestionScore,
                IsCorrect = qaq.IsCorrect,
                IsMultipleAnswers = qaq.IsMultipleAnswers,
                Name = qaq.Name,
                QuizAttemptAnswers = qaq.QuizAttemptAnswers.Select(MapToAnswerDto).ToList()
            };
        }

        private static QuizAttemptAnswerDto MapToAnswerDto(QuizAttemptAnswer qaa)
        {
            return new QuizAttemptAnswerDto
            {
                Id = qaa.Id,
                QuizOptionId = qaa.QuizOptionId,
                IsCorrect = qaa.IsCorrect,
                Name = qaa.Name
            };
        }

        #endregion
    }
}
