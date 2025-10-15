using Lssctc.ProgramManagement.Learnings.LearningsPartitions.Services;
using Lssctc.ProgramManagement.Learnings.LearningsQuizzes.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Learnings.LearningsQuizzes.Services
{
    public class LearningsSectionQuizService : ILearningsSectionQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILearningsSectionPartitionService _lspService;
        public LearningsSectionQuizService(IUnitOfWork unitOfWork, ILearningsSectionPartitionService lspService)
        {
            _unitOfWork = unitOfWork;
            _lspService = lspService;
        }
        public async Task<LearningsSectionQuizDto> GetSectionQuizByPartitionIdAndTraineeId(int partitionId, int traineeId)
        {
            if (partitionId <= 0 || traineeId <= 0)
            {
                throw new ArgumentException("Invalid partitionId or traineeId");
            }
            var lrp = await GetOrCreateLrp(partitionId, traineeId);

            var sq = await _unitOfWork.SectionQuizRepository.GetAllAsQueryable()
                .Where(sq => sq.SectionPartitionId == partitionId)
                .Include(sq => sq.Quiz)
                .FirstOrDefaultAsync();

            if (sq == null || sq.Quiz == null)
            {
                throw new KeyNotFoundException("SectionQuiz or associated Quiz not found");
            }

            //var traineeCert = await _unitOfWork.TraineeCertificateRepository.GetAllAsQueryable()
            //    .Where(tc => tc.TraineeId == traineeId && tc.CertificateId == sq.Quiz.CertificateId)
            //    .FirstOrDefaultAsync();

            //if (traineeCert != null)
            //{
            //    throw new InvalidOperationException("Trainee has already obtained the certificate associated with this quiz.");
            //}

            var sqa = lrp.SectionQuizAttempts
                .OrderByDescending(sqa => sqa.QuizAttemptDate)
                .FirstOrDefault(sqa => sqa.SectionQuizId == sq.Id);
            

            var lsqDto = new LearningsSectionQuizDto
            {
                SectionQuizId = sq.Id,
                QuizId = sq.QuizId,
                LearningRecordPartitionId = lrp.Id,
                SectionQuizAttemptId = sqa?.Id,
                QuizName = sq.Quiz.Name,
                PassScoreCriteria = sq.Quiz.PassScoreCriteria,
                TimelimitMinute = sq.Quiz.TimelimitMinute,
                TotalScore = sq.Quiz.TotalScore,
                Description = sq.Quiz.Description,
                IsCompleted = lrp.IsComplete,
                AttemptScore = sqa?.AttemptScore,
                LastAttemptIsPass = sqa?.IsPass,
                LastAttemptDate = sqa?.QuizAttemptDate,
            };

            return lsqDto;
        }

        

        public async Task<LearningsSectionQuizDto> SubmitSectionQuizAttempt(int partitionId, int traineeId, CreateLearningsSectionQuizAttemptDto attempt)
        {
            if (partitionId <= 0 || traineeId <= 0)
            {
                throw new ArgumentException("Invalid partitionId or traineeId");
            }
            if (attempt == null 
                || attempt.Answers == null 
                || !attempt.Answers.Any()
                || attempt.QuizId <= 0 
                || attempt.SectionQuizId <= 0)
            {
                throw new ArgumentException("Invalid attempt data");
            }
            var lrp = await GetOrCreateLrp(partitionId, traineeId);

            var sq = await _unitOfWork.SectionQuizRepository.GetAllAsQueryable()
                .Where(sq => sq.Id == attempt.SectionQuizId)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(q => q.QuizQuestionOptions)
                .FirstOrDefaultAsync();

            if (sq == null || sq.Quiz == null)
            {
                throw new KeyNotFoundException("SectionQuiz or associated Quiz not found");
            }

            var sqa = new SectionQuizAttempt
            {
                SectionQuizId = attempt.SectionQuizId,
                Name = sq.Quiz.Name ?? "Name",
                AttemptScore = 0, // to be calculated
                LearningRecordPartitionId = lrp.Id,
                MaxScore = sq.Quiz.TotalScore,
                QuizAttemptDate = DateTime.UtcNow,
                Status = 1, // assuming 1 means completed
                AttemptOrder = (lrp.SectionQuizAttempts?.Count ?? 0) + 1,
                IsPass = false // to be determined
            };
            decimal totalScore = 0;
            foreach (var ans in attempt.Answers)
            {
                var question = sq.Quiz.QuizQuestions
                    .FirstOrDefault(q => q.Id == ans.QuestionId);
                if (question == null)
                {
                    throw new KeyNotFoundException($"Question with ID {ans.QuestionId} not found in Quiz");
                }
                var correctOptionIds = question.QuizQuestionOptions
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToList();
                var isCorrect = correctOptionIds.Count == ans.SelectedOptionIds.Intersect(correctOptionIds).Count()
                    && correctOptionIds.Count == ans.SelectedOptionIds.Count;
                var questionScore = isCorrect ? question.QuestionScore ?? 0 : 0;
                totalScore += questionScore;
                var sqaQuestion = new SectionQuizAttemptQuestion
                {
                    SectionQuizAttemptId = sqa.Id,
                    AttemptScore = questionScore,
                    QuestionScore = question.QuestionScore,
                    IsCorrect = isCorrect,
                    IsMultipleAnswers = question.IsMultipleAnswers,
                    Name = question.Name,
                    Description = question.Description,
                };

                foreach (var selectedOptionId in ans.SelectedOptionIds)
                {
                    var selectedOption = question.QuizQuestionOptions.FirstOrDefault(o => o.Id == selectedOptionId);
                    if (selectedOption == null)
                        continue; // skip if invalid option id

                    var isOptionCorrect = selectedOption.IsCorrect; // true or false from DB
                    var optionScore = isOptionCorrect ? selectedOption.OptionScore ?? 0 : 0;

                    var sqaAnswer = new SectionQuizAttemptAnswer
                    {
                        SectionQuizAttemptQuestion = sqaQuestion,
                        AttemptScore = optionScore,
                        IsCorrect = isOptionCorrect,
                        Description = selectedOption.Description,
                        Name = selectedOption.Name
                    };

                    sqaQuestion.SectionQuizAttemptAnswers.Add(sqaAnswer);
                }

                sqa.SectionQuizAttemptQuestions.Add(sqaQuestion);
            }

            sqa.AttemptScore = totalScore;
            sqa.IsPass = totalScore >= (sq.Quiz.PassScoreCriteria ?? 0);

            await _unitOfWork.SectionQuizAttemptRepository.CreateAsync(sqa);
            await _unitOfWork.SaveChangesAsync();

            lrp.IsComplete = sqa.IsPass ?? false;
            lrp.CompletedAt = sqa.IsPass == true ? sqa.QuizAttemptDate : lrp.CompletedAt;
            lrp.UpdatedAt = sqa.QuizAttemptDate;
            await _unitOfWork.LearningRecordPartitionRepository.UpdateAsync(lrp);
            await _unitOfWork.SaveChangesAsync();

            var lsqDto = new LearningsSectionQuizDto
            {
                SectionQuizId = sq.Id,
                QuizId = sq.QuizId,
                LearningRecordPartitionId = lrp.Id,
                SectionQuizAttemptId = sqa.Id,
                QuizName = sq.Quiz.Name,
                PassScoreCriteria = sq.Quiz.PassScoreCriteria,
                TimelimitMinute = sq.Quiz.TimelimitMinute,
                TotalScore = sq.Quiz.TotalScore,
                Description = sq.Quiz.Description,
                IsCompleted = lrp.IsComplete,
                AttemptScore = sqa.AttemptScore,
                LastAttemptIsPass = sqa.IsPass,
                LastAttemptDate = sqa.QuizAttemptDate,
            };

            return lsqDto;
        }

        private async Task<LearningRecordPartition> GetOrCreateLrp(int partitionId, int traineeId)
        {
            var lrp = await _unitOfWork.LearningRecordPartitionRepository.GetAllAsQueryable()
                .Where(p => p.SectionPartitionId == partitionId && p.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId)
                .Include(p => p.SectionQuizAttempts)
                .FirstOrDefaultAsync();
            if (lrp == null)
            {
                await _lspService.GetSectionPartitionByPartitionIdAndTraineeId(partitionId, traineeId);
                lrp = await _unitOfWork.LearningRecordPartitionRepository.GetAllAsQueryable()
                    .Where(p => p.SectionPartitionId == partitionId && p.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId)
                    .FirstOrDefaultAsync();
                if (lrp == null)
                {
                    throw new Exception("Failed to create LearningRecordPartition");
                }
            }
            return lrp;
        }
    }
}
