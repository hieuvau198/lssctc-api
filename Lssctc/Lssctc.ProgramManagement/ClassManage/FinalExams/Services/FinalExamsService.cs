using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FinalExamsService : IFinalExamsService
    {
        private readonly IUnitOfWork _uow;

        public FinalExamsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task AutoCreateFinalExamsForClassAsync(int classId)
        {
            var classInfo = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classInfo == null) return;

            var defaultTime = classInfo.EndDate ?? classInfo.StartDate.AddMonths(1);

            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var finalExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                    .FirstOrDefaultAsync(fe => fe.EnrollmentId == enrollment.Id);

                if (finalExam == null)
                {
                    finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        IsPass = null,
                        TotalMarks = 0,
                        CompleteTime = null,
                        Status = (int)FinalExamStatusEnum.NotYet
                    };
                    await _uow.FinalExamRepository.CreateAsync(finalExam);
                    await _uow.SaveChangesAsync();
                }

                await EnsurePartialExists(finalExam.Id, 1, defaultTime); // Theory
                await EnsurePartialExists(finalExam.Id, 2, defaultTime); // Simulation
                await EnsurePartialExists(finalExam.Id, 3, defaultTime); // Practical
            }
            await _uow.SaveChangesAsync();
        }

        private async Task EnsurePartialExists(int finalExamId, int type, DateTime defaultTime)
        {
            var exists = await _uow.FinalExamPartialRepository.ExistsAsync(p => p.FinalExamId == finalExamId && p.Type == type);
            if (!exists)
            {
                // [UPDATE] Default weights: TE=30%, SE=20%, PE=50%
                decimal defaultWeight = 0;
                switch (type)
                {
                    case 1: defaultWeight = 30; break; // Theory
                    case 2: defaultWeight = 20; break; // Simulation
                    case 3: defaultWeight = 50; break; // Practical
                }

                var partial = new FinalExamPartial
                {
                    FinalExamId = finalExamId,
                    Type = type,
                    Marks = 0,
                    ExamWeight = defaultWeight,
                    Duration = 60,
                    StartTime = defaultTime,
                    EndTime = defaultTime.AddMinutes(60),
                    Status = (int)FinalExamPartialStatus.NotYet
                };
                await _uow.FinalExamPartialRepository.CreateAsync(partial);
            }
        }

        public async Task<FinalExamDto> CreateFinalExamAsync(CreateFinalExamDto dto)
        {
            var exists = await _uow.FinalExamRepository.ExistsAsync(fe => fe.EnrollmentId == dto.EnrollmentId);
            if (exists) throw new InvalidOperationException("Final Exam already exists for this enrollment.");

            var entity = new FinalExam
            {
                EnrollmentId = dto.EnrollmentId,
                TotalMarks = 0,
                Status = (int)FinalExamStatusEnum.NotYet
            };
            await _uow.FinalExamRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return await GetFinalExamByIdAsync(entity.Id);
        }

        public async Task<FinalExamDto> UpdateFinalExamAsync(int id, UpdateFinalExamDto dto)
        {
            var entity = await _uow.FinalExamRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Final Exam not found.");

            if (dto.IsPass.HasValue) entity.IsPass = dto.IsPass;
            if (dto.TotalMarks.HasValue) entity.TotalMarks = dto.TotalMarks;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;

            await _uow.FinalExamRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return await GetFinalExamByIdAsync(id);
        }

        public async Task<FinalExamDto> GetFinalExamByIdAsync(int id)
        {
            var entity = await GetFinalExamQuery().FirstOrDefaultAsync(fe => fe.Id == id);
            if (entity == null) throw new KeyNotFoundException($"Final Exam {id} not found.");
            return MapToDto(entity, isInstructor: true);
        }

        public async Task<IEnumerable<FinalExamDto>> GetFinalExamsByClassAsync(int classId)
        {
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .AnyAsync(fe => fe.Enrollment.ClassId == classId);

            if (!hasExams)
            {
                await AutoCreateFinalExamsForClassAsync(classId);
            }

            var entities = await GetFinalExamQuery()
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();
            return entities.Select(e => MapToDto(e, isInstructor: true));
        }

        public async Task<FinalExamDto?> GetMyFinalExamByClassAsync(int classId, int userId)
        {
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .AnyAsync(fe => fe.Enrollment.ClassId == classId);

            if (!hasExams)
            {
                await AutoCreateFinalExamsForClassAsync(classId);
            }

            var entity = await GetFinalExamQuery()
                .FirstOrDefaultAsync(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.TraineeId == userId);
            return entity == null ? null : MapToDto(entity, isInstructor: false);
        }

        public async Task<IEnumerable<FinalExamDto>> GetFinalExamsByTraineeAsync(int traineeId)
        {
            var entities = await GetFinalExamQuery()
                .Where(fe => fe.Enrollment.TraineeId == traineeId)
                .ToListAsync();
            return entities.Select(e => MapToDto(e, isInstructor: false));
        }

        public async Task DeleteFinalExamAsync(int id)
        {
            var entity = await _uow.FinalExamRepository.GetByIdAsync(id);
            if (entity != null)
            {
                await _uow.FinalExamRepository.DeleteAsync(entity);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateExamCodeAsync(int finalExamId)
        {
            var entity = await _uow.FinalExamRepository.GetByIdAsync(finalExamId);
            if (entity == null) throw new KeyNotFoundException("Final Exam not found.");

            string code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            entity.ExamCode = code;

            await _uow.FinalExamRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return code;
        }

        public async Task FinishFinalExamAsync(int classId)
        {
            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();

            foreach (var exam in exams)
            {
                await RecalculateFinalExamScoreInternal(exam);
            }
            await _uow.SaveChangesAsync();
        }

        public async Task RecalculateFinalExamScore(int finalExamId)
        {
            _uow.GetDbContext().ChangeTracker.Clear();
            var finalExam = await _uow.FinalExamRepository.GetAllAsQueryable()
               .Include(fe => fe.FinalExamPartials)
               .FirstOrDefaultAsync(fe => fe.Id == finalExamId);

            if (finalExam != null)
            {
                await RecalculateFinalExamScoreInternal(finalExam);
                await _uow.FinalExamRepository.UpdateAsync(finalExam);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task UpdateClassExamWeightsAsync(int classId, UpdateClassWeightsDto dto)
        {
            // Validate Total Sum (using a small epsilon for floating point comparison safety)
            if (Math.Abs((dto.TheoryWeight + dto.SimulationWeight + dto.PracticalWeight) - 1.0m) > 0.0001m)
            {
                throw new InvalidOperationException("The sum of weights must be exactly 1.0 (100%).");
            }

            // Get all final exams for the class with their partials
            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();

            if (!exams.Any())
            {
                // Optionally auto-create exams if none exist, or just return
                await AutoCreateFinalExamsForClassAsync(classId);
                // Re-fetch
                exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                    .Include(fe => fe.FinalExamPartials)
                    .Where(fe => fe.Enrollment.ClassId == classId)
                    .ToListAsync();
            }

            foreach (var exam in exams)
            {
                foreach (var partial in exam.FinalExamPartials)
                {
                    // Map Type ID to Weight: 1=Theory, 2=Simulation, 3=Practical
                    // Convert 0-1 range to 0-100 range for storage
                    switch (partial.Type)
                    {
                        case 1:
                            partial.ExamWeight = dto.TheoryWeight * 100;
                            break;
                        case 2:
                            partial.ExamWeight = dto.SimulationWeight * 100;
                            break;
                        case 3:
                            partial.ExamWeight = dto.PracticalWeight * 100;
                            break;
                    }
                }

                // Recalculate scores with new weights
                await RecalculateFinalExamScoreInternal(exam);
            }

            await _uow.SaveChangesAsync();
        }

        private async Task RecalculateFinalExamScoreInternal(FinalExam finalExam)
        {
            decimal total = 0;
            foreach (var p in finalExam.FinalExamPartials)
            {
                total += (p.Marks ?? 0) * ((p.ExamWeight ?? 0) / 100m);
            }
            finalExam.TotalMarks = total;
            finalExam.IsPass = finalExam.TotalMarks >= 5;

            if (finalExam.FinalExamPartials.Any() &&
                finalExam.FinalExamPartials.All(p => p.Status == (int)FinalExamPartialStatus.Approved || p.Status == (int)FinalExamPartialStatus.Submitted))
            {
                finalExam.CompleteTime = DateTime.UtcNow;
                finalExam.Status = (int)FinalExamStatusEnum.Completed;
            }
        }

        private IQueryable<FinalExam> GetFinalExamQuery()
        {
            return _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists);
        }

        private FinalExamDto MapToDto(FinalExam entity, bool isInstructor)
        {
            return new FinalExamDto
            {
                Id = entity.Id,
                EnrollmentId = entity.EnrollmentId,
                TraineeName = entity.Enrollment?.Trainee?.IdNavigation?.Fullname,
                TraineeCode = entity.Enrollment?.Trainee?.TraineeCode,
                IsPass = entity.IsPass,
                TotalMarks = entity.TotalMarks,
                CompleteTime = entity.CompleteTime,
                ExamCode = isInstructor ? entity.ExamCode : null,
                Status = GetFinalExamStatusName(entity.Status),
                Partials = entity.FinalExamPartials?.Select(p => MapToPartialDto(p, isInstructor)).ToList() ?? new List<FinalExamPartialDto>()
            };
        }

        private FinalExamPartialDto MapToPartialDto(FinalExamPartial p, bool isInstructor)
        {
            var theory = p.FeTheories.FirstOrDefault();
            var sim = p.FeSimulations.FirstOrDefault();
            int statusId = p.Status ?? 0;
            List<SeTaskDto>? tasks = null;
            if (p.Type == 2 && sim != null)
            {
                if (sim.SeTasks != null)
                {
                    tasks = sim.SeTasks.Select(t => new SeTaskDto
                    {
                        Id = t.Id,
                        FeSimulationId = t.FeSimulationId,
                        TaskCode = t.SimTask?.TaskCode,
                        Name = t.Name ?? t.SimTask?.TaskName,
                        Description = t.Description,
                        IsPass = t.IsPass,
                        CompleteTime = t.CompleteTime,
                        DurationSecond = t.DurationSecond
                    }).ToList();
                }
            }
            return new FinalExamPartialDto
            {
                Id = p.Id,
                Type = GetTypeName(p.Type ?? 0),
                Marks = p.Marks,
                ExamWeight = p.ExamWeight,
                Duration = p.Duration,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                CompleteTime = p.CompleteTime,
                Status = GetFinalExamPartialStatusName(statusId),
                IsPass = p.IsPass,
                QuizId = isInstructor ? theory?.QuizId : null,
                QuizName = isInstructor ? theory?.Quiz?.Name : null,
                PracticeId = sim?.PracticeId,
                PracticeName = sim?.Practice?.PracticeName,
                Checklists = p.PeChecklists?.Select(c => new PeChecklistItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsPass = c.IsPass
                }).ToList(),
                Tasks = tasks
            };
        }

        private string GetFinalExamStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "NotYet",
                2 => "Submitted",
                3 => "Completed",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }

        private string GetFinalExamPartialStatusName(int statusId)
        {
            return statusId switch { 0 => "NotYet", 1 => "Submitted", 2 => "Approved", _ => "Unknown" };
        }

        private string GetTypeName(int typeId)
        {
            return typeId switch { 1 => "Theory", 2 => "Simulation", 3 => "Practical", _ => "Unknown" };
        }
    }
}