using Lssctc.LearningManagement.TraineePractices.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.LearningManagement.TraineePractices.Services
{
    public class TraineePracticeService : ITraineePracticeService
    {
        private readonly IUnitOfWork _uow;
        public TraineePracticeService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<TraineePracticeDto?> GetTraineePracticeByIdA(int practiceId, int traineeId)
        {
            var sp = await _uow.SectionPracticeRepository.
                GetAllAsQueryable()
                .Where(x => x.PracticeId == practiceId && x.IsDeleted != true)
                .Include(x => x.SectionPartition)
                    .ThenInclude(sp => sp.LearningRecordPartitions)
                .Include(x => x.Practice)
                .FirstOrDefaultAsync();
            if (sp == null)
                throw new KeyNotFoundException($"No SectionPractice found for Practice ID {practiceId}.");
            if(sp.SectionPartition == null)
                throw new InvalidOperationException($"SectionPractice with ID {sp.Id} has no associated SectionPartition.");
            if(sp.Practice == null)
                throw new InvalidOperationException($"SectionPractice with ID {sp.Id} has no associated Practice.");


            if (sp.SectionPartition.LearningRecordPartitions == null || sp.SectionPartition.LearningRecordPartitions.Count == 0)
            {
                var lr = await _uow.LearningRecordRepository
                    .GetAllAsQueryable()
                    .Where(lr => lr.TrainingProgress.CourseMember.TraineeId == traineeId)
                    .FirstOrDefaultAsync();
                if (lr == null)
                    throw new KeyNotFoundException($"No LearningRecord found for Trainee ID {traineeId}.");
                var newLrp = new LearningRecordPartition
                {
                    SectionPartitionId = sp.SectionPartitionId,
                    Name = sp.SectionPartition.Name + " Record",
                    LearningRecordId = lr.Id,
                    Description = sp.SectionPartition.Description,
                    IsComplete = false,
                    RecordPartitionOrder = 1,
                };
                await _uow.LearningRecordPartitionRepository.CreateAsync(newLrp);
                await _uow.SaveChangesAsync();
            }
            var lrp = sp.SectionPartition.LearningRecordPartitions?
                .FirstOrDefault();

            if (lrp == null)
                throw new InvalidOperationException($"No LearningRecordPartition found for SectionPartition ID {sp.SectionPartitionId}.");

            var dto = new TraineePracticeDto
            {
                SectionPracticeId = sp.Id,
                PartitionId = lrp.SectionPartitionId,
                PracticeId = sp.PracticeId,
                CustomDeadline = sp.CustomDeadline,
                Status = lrp.IsComplete ? "Completed" : "Incompleted",
                IsCompleted = lrp.IsComplete,
                PracticeName = sp.Practice.PracticeName ?? "Practice Name",
                PracticeDescription = sp.Practice.PracticeDescription ?? "Practice Description",
                EstimatedDurationMinutes = sp.Practice.EstimatedDurationMinutes,
                DifficultyLevel = sp.Practice.DifficultyLevel,
            };
            return dto;
        }

        public async Task<List<TraineePracticeDto>> GetTraineePracticesByTraineeIdAndClassId(int traineeId, int classId)
        {
            var spList = await _uow.SectionPracticeRepository
                .GetAllAsQueryable()
                .Where(x => x.SectionPartition.Section.ClassesId == classId && x.IsDeleted != true)
                .Include(x => x.SectionPartition)
                    .ThenInclude(sp => sp.LearningRecordPartitions)
                        .ThenInclude(lrp => lrp.LearningRecord)
                            .ThenInclude(lr => lr.TrainingProgress)
                                .ThenInclude(tp => tp.CourseMember)
                .Include(x => x.Practice)
                .ToListAsync();

            if (spList == null || spList.Count == 0)
                throw new KeyNotFoundException($"No SectionPractices found for Class ID {classId}.");

            var result = new List<TraineePracticeDto>();

            foreach (var sp in spList)
            {
                if (sp.SectionPartition == null || sp.Practice == null)
                    continue;

                var lrPartition = sp.SectionPartition.LearningRecordPartitions?
                    .FirstOrDefault(lrp =>
                        lrp.LearningRecord != null &&
                        lrp.LearningRecord.TrainingProgress.CourseMember.TraineeId == traineeId);

                // If trainee has no LearningRecordPartition for this section, optionally skip or create
                if (lrPartition == null)
                {
                    // try to find an existing learning record for trainee
                    var lr = await _uow.LearningRecordRepository
                        .GetAllAsQueryable()
                        .Where(lr => lr.TrainingProgress.CourseMember.TraineeId == traineeId)
                        .FirstOrDefaultAsync();

                    if (lr == null)
                        continue;

                    var newLrp = new LearningRecordPartition
                    {
                        SectionPartitionId = sp.SectionPartitionId,
                        Name = sp.SectionPartition.Name + " Record",
                        LearningRecordId = lr.Id,
                        Description = sp.SectionPartition.Description,
                        IsComplete = false,
                        RecordPartitionOrder = 1,
                    };
                    await _uow.LearningRecordPartitionRepository.CreateAsync(newLrp);
                    await _uow.SaveChangesAsync();
                    lrPartition = newLrp;
                }

                var dto = new TraineePracticeDto
                {
                    SectionPracticeId = sp.Id,
                    PartitionId = lrPartition.SectionPartitionId,
                    PracticeId = sp.PracticeId,
                    CustomDeadline = sp.CustomDeadline,
                    Status = lrPartition.IsComplete ? "Completed" : "Incompleted",
                    IsCompleted = lrPartition.IsComplete,
                    PracticeName = sp.Practice.PracticeName ?? "Practice Name",
                    PracticeDescription = sp.Practice.PracticeDescription ?? "Practice Description",
                    EstimatedDurationMinutes = sp.Practice.EstimatedDurationMinutes,
                    DifficultyLevel = sp.Practice.DifficultyLevel,
                };

                result.Add(dto);
            }

            return result;
        }

    }
}
