using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.PracticeAttempts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.SimulationManagement.PracticeAttempts.Services
{
    public class PracticeAttemptService : IPracticeAttemptService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PracticeAttemptService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<PracticeAttemptDto> CreatePracticeAttempt(int sectionPracticeId, int traineeId)
        {
            // Validate input
            if (sectionPracticeId < 0 || traineeId < 0)
                throw new ArgumentException("Invalid practice ID and trainee ID.");

            // Check if the section practice exists, with specific trainee
            var sp = await _unitOfWork.SectionPracticeRepository
                .GetAllAsQueryable()
                .Where(sp => 
                    sp.Id == sectionPracticeId 
                    && sp.IsDeleted != true
                    && sp.SectionPartition.Section.Classes.ClassMembers.Any(cm => cm.TraineeId == traineeId)
                    )
                .Include(sp => sp.SectionPartition)
                    .ThenInclude(s => s.LearningRecordPartitions)
                        .ThenInclude(lrp => lrp.LearningRecord)
                .FirstOrDefaultAsync();
            if (sp == null)
                throw new KeyNotFoundException($"No found for SectionPractice ID {sectionPracticeId}, for trainee {traineeId}.");

            // find lrp or create new one
            var lrp = sp.SectionPartition.LearningRecordPartitions?
                .FirstOrDefault();
            if (lrp == null)
            {
                var lr = await _unitOfWork.LearningRecordRepository
                    .GetAllAsQueryable()
                    .Where(lr => 
                        lr.SectionId == sp.SectionPartition.SectionId
                        && lr.TrainingProgress.CourseMember.TraineeId == traineeId
                        )
                    .FirstOrDefaultAsync();
                if (lr == null)
                    throw new InvalidOperationException($"No LearningRecord found for Section ID {sp.SectionPartition.SectionId} and Trainee ID {traineeId}.");
                lrp = new LearningRecordPartition
                {
                    SectionPartitionId = sp.SectionPartitionId,
                    Name = sp.Practice.PracticeName ?? "LearningRecordPartition Name",
                    LearningRecordId = lr.Id,
                    Description = $"Record for {sp.SectionPartition.Name}",
                    StartedAt = DateTime.UtcNow,
                    RecordPartitionOrder = 1,
                    IsComplete = false
                };
                await _unitOfWork.LearningRecordPartitionRepository.CreateAsync(lrp);
                await _unitOfWork.SaveChangesAsync();
            }

            // create new attempt
            var newAttempt = new SectionPracticeAttempt
            {
                SectionPracticeId = sectionPracticeId,
                LearningRecordPartitionId = lrp.Id, // to be updated
                Score = 0,
                AttemptDate = DateTime.UtcNow,
                AttemptStatus = 1,
                Description = "Initial attempt",
                IsPass = false,
                IsDeleted = false,
            };
            
            await _unitOfWork.SectionPracticeAttemptRepository.CreateAsync(newAttempt);
            await _unitOfWork.SaveChangesAsync();

            // Map to DTO
            var dto = new PracticeAttemptDto
            {
                PracticeAttemptId = newAttempt.Id,
                SectionPracticeId = newAttempt.SectionPracticeId,
                LearningRecordPartitionId = newAttempt.LearningRecordPartitionId,
                Score = newAttempt.Score,
                AttemptDate = newAttempt.AttemptDate,
                AttemptStatus = newAttempt.AttemptStatus,
                Description = newAttempt.Description,
                IsPass = newAttempt.IsPass,
            };
            return dto;
        }

        public Task<bool> DeletePracticeAttempt(int attemptId)
        {
            throw new NotImplementedException();
        }

        public Task<PracticeAttemptDto?> GetPracticeAttemptById(int attemptId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PracticeAttemptDto>> GetPracticeAttemptsByPracticeIdAndTraineeId(int sectionPracticeId, int traineeId)
        {
            throw new NotImplementedException();
        }
    }
}
