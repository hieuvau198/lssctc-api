using Lssctc.ProgramManagement.Learnings.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Learnings.Services
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

            var sqa = lrp.SectionQuizAttempts
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

        

        public Task<LearningsSectionQuizDto> SubmitSectionQuizAttempt(int partitionId, int traineeId, CreateLearningsSectionQuizAttemptDto input)
        {
            throw new NotImplementedException();
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
