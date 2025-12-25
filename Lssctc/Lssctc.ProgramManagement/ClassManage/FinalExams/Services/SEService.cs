using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class SEService : ISEService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFEService _finalExamsService;

        public SEService(IUnitOfWork uow, IFEService finalExamsService)
        {
            _uow = uow;
            _finalExamsService = finalExamsService;
        }

        public async Task EnsureSeTasksForSimulationAsync(int feSimulationId, int practiceId)
        {
            var exists = await _uow.SeTaskRepository.ExistsAsync(t => t.FeSimulationId == feSimulationId);
            if (exists) return;

            var practiceTasks = await _uow.PracticeTaskRepository.GetAllAsQueryable()
                .Include(pt => pt.Task)
                .Where(pt => pt.PracticeId == practiceId)
                .ToListAsync();

            if (!practiceTasks.Any()) return;

            foreach (var pt in practiceTasks)
            {
                var seTask = new SeTask
                {
                    FeSimulationId = feSimulationId,
                    SimTaskId = pt.TaskId,
                    Name = pt.Task?.TaskName ?? "Unknown Task",
                    Description = pt.Task?.TaskDescription,
                    Status = 0,
                    IsPass = null,
                    DurationSecond = 0,
                    AttemptTime = null
                };
                await _uow.SeTaskRepository.CreateAsync(seTask);
            }
            await _uow.SaveChangesAsync();
        }

        public async Task<IEnumerable<SePracticeListDto>> GetMySimulationExamPartialsByClassAsync(int classId, int userId)
        {
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .AnyAsync(fe => fe.Enrollment.ClassId == classId);

            if (!hasExams)
            {
                await _finalExamsService.AutoCreateFinalExamsForClassAsync(classId);
            }

            var partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Where(p => p.FinalExam.Enrollment.ClassId == classId &&
                            p.FinalExam.Enrollment.TraineeId == userId &&
                            p.Type == 2)
                .ToListAsync();

            var practiceDtos = new List<SePracticeListDto>();

            foreach (var partial in partials)
            {
                var feSim = partial.FeSimulations.FirstOrDefault();
                var practice = feSim?.Practice;

                if (practice == null) continue;

                var dto = new SePracticeListDto
                {
                    id = practice.Id,
                    practiceName = practice.PracticeName,
                    practiceCode = practice.PracticeCode,
                    practiceDescription = practice.PracticeDescription ?? string.Empty,
                    estimatedDurationMinutes = partial.Duration ?? 0,
                    difficultyLevel = practice.DifficultyLevel ?? string.Empty,
                    maxAttempts = practice.MaxAttempts ?? 0,
                    createdDate = practice.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                    isActive = practice.IsActive ?? false,
                    isCompleted = partial.CompleteTime.HasValue,
                    FinalExamPartialId = partial.Id,
                    FinalExamPartialStatus = GetFinalExamPartialStatusName(partial.Status ?? 0),
                    StartTime = partial.StartTime,
                    EndTime = partial.EndTime
                };
                practiceDtos.Add(dto);
            }

            return practiceDtos;
        }

        public async Task<FinalExamPartialDto> ValidateSeCodeAndStartSimulationExamAsync(int partialId, string examCode, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");

            // --- UPDATED VALIDATION: Check Final Exam Status instead of strictly checking CompleteTime ---
            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot start exam. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }
            // -----------------------------------------------------------------------------------------

            if (string.IsNullOrEmpty(partial.ExamCode) ||
                !partial.ExamCode.Equals(examCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid Exam Code.");
            }

            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
                await _uow.FinalExamPartialRepository.UpdateAsync(partial);
                await _uow.SaveChangesAsync();
            }

            return MapToPartialDto(partial, isInstructor: false);
        }

        public async Task<FinalExamPartialDto> StartSimulationExamAsync(int partialId, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");

            // --- UPDATED VALIDATION: Check Final Exam Status ---
            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot start exam. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }
            // -------------------------------------------------

            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
                await _uow.FinalExamPartialRepository.UpdateAsync(partial);
                await _uow.SaveChangesAsync();
            }

            return MapToPartialDto(partial, isInstructor: false);
        }

        public async Task<SeTaskDto> SubmitSeTaskByCodeAsync(int partialId, string taskCode, int userId, SubmitSeTaskDto dto)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");

            var seTask = await _uow.GetDbContext().Set<SeTask>()
                .Include(t => t.SimTask)
                .Include(t => t.FeSimulation)
                .FirstOrDefaultAsync(t => t.FeSimulation.FinalExamPartialId == partialId
                                       && t.SimTask.TaskCode == taskCode);

            if (seTask == null) throw new KeyNotFoundException($"Task with code '{taskCode}' not found in this exam.");
            var nowUtc7 = DateTime.UtcNow;

            seTask.IsPass = dto.IsPass;
            seTask.DurationSecond = dto.DurationSecond;
            seTask.CompleteTime = DateTime.UtcNow.AddHours(7);
            if (dto.DurationSecond.HasValue)
            {
                seTask.AttemptTime = nowUtc7.AddSeconds(-dto.DurationSecond.Value);
            }
            else
            {
                seTask.AttemptTime = nowUtc7;
            }
            seTask.Status = 1;

            _uow.GetDbContext().Set<SeTask>().Update(seTask);
            await _uow.SaveChangesAsync();

            return new SeTaskDto
            {
                Id = seTask.Id,
                FeSimulationId = seTask.FeSimulationId,
                TaskCode = seTask.SimTask?.TaskCode,
                Name = seTask.Name ?? seTask.SimTask?.TaskName,
                Description = seTask.Description,
                IsPass = seTask.IsPass,
                CompleteTime = seTask.CompleteTime,
                DurationSecond = seTask.DurationSecond
            };
        }

        public async Task<FinalExamPartialDto> SubmitSeFinalAsync(int partialId, int userId, SubmitSeFinalDto dto)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

            // Validation: Check Final Exam Status
            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot submit result. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }

            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");
            var nowUtc7 = DateTime.UtcNow;

            // 1. Update individual Task statuses (Always save task results regardless of pass/fail)
            if (dto.Tasks != null && dto.Tasks.Any())
            {
                // Note: Removed .Include(t => t.FeSimulation) to avoid tracking conflicts with 'partial'
                var seTasks = await _uow.GetDbContext().Set<SeTask>()
                    .Include(t => t.SimTask)
                    .Where(t => t.FeSimulation.FinalExamPartialId == partialId)
                    .ToListAsync();

                foreach (var taskDto in dto.Tasks)
                {
                    var taskEntity = seTasks.FirstOrDefault(t => t.SimTask.TaskCode == taskDto.TaskCode);
                    if (taskEntity != null)
                    {
                        taskEntity.IsPass = taskDto.IsPass;
                        taskEntity.DurationSecond = taskDto.DurationSecond;
                        taskEntity.CompleteTime = nowUtc7;
                        taskEntity.Status = 1; // Mark as attempted/submitted

                        // Calculate attempt time based on duration
                        if (taskDto.DurationSecond.HasValue)
                        {
                            taskEntity.AttemptTime = nowUtc7.AddSeconds(-taskDto.DurationSecond.Value);
                        }
                        else
                        {
                            taskEntity.AttemptTime = nowUtc7;
                        }
                    }
                }
                await _uow.SaveChangesAsync();
            }

            // 2. Calculate Overall Score based on New Logic
            decimal calculatedMarks = 0;

            if (dto.IsPass)
            {
                // Default score is 10. Minus 0.5 for every mistake.
                // Formula: 10 - (TotalMistake * 0.5)
                decimal deduction = dto.TotalMistake * 0.5m;
                calculatedMarks = 10m - deduction;

                // Ensure marks do not go below 0
                if (calculatedMarks < 0) calculatedMarks = 0;
            }
            else
            {
                // If SE status is Fail, Total Score is 0
                calculatedMarks = 0;
            }

            // 3. Update Partial Exam Result
            partial.Marks = calculatedMarks;
            partial.IsPass = dto.IsPass;
            partial.Description = dto.Description;
            partial.StartTime = dto.StartTime;
            partial.CompleteTime = dto.CompleteTime;
            partial.Status = (int)FinalExamPartialStatus.Submitted;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // Auto generate new code after submission
            await _finalExamsService.GenerateExamCodeAsync(partialId);

            // Recalculate the Final Exam total score
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);

            return MapToPartialDto(partial, false);
        }

        public async Task<SimulationExamDetailDto> GetSimulationExamDetailAsync(int partialId)
        {
            var checkSim = await _uow.FeSimulationRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(s => s.FinalExamPartialId == partialId);

            if (checkSim != null && checkSim.PracticeId > 0)
            {
                await EnsureSeTasksForSimulationAsync(checkSim.Id, checkSim.PracticeId);
            }

            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(p => p.FeSimulations).ThenInclude(s => s.SeTasks).ThenInclude(t => t.SimTask)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.Type != 2) throw new ArgumentException("This is not a Simulation Exam.");

            return MapToSimulationDetailDto(partial);
        }

        public async Task<IEnumerable<ClassSimulationResultDto>> GetClassSimulationResultsAsync(int classId)
        {
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .AnyAsync(fe => fe.Enrollment.ClassId == classId);
            if (!hasExams) await _finalExamsService.AutoCreateFinalExamsForClassAsync(classId);

            var simPartials = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Where(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true)
                .SelectMany(fe => fe.FinalExamPartials)
                .Where(p => p.Type == 2)
                .Select(p => p.FeSimulations.FirstOrDefault())
                .Where(s => s != null && s.PracticeId > 0)
                .Select(s => new { s.Id, s.PracticeId })
                .ToListAsync();

            foreach (var sim in simPartials)
            {
                await EnsureSeTasksForSimulationAsync(sim.Id, sim.PracticeId);
            }

            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(fe => fe.FinalExamPartials)
                    .ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(fe => fe.FinalExamPartials)
                    .ThenInclude(p => p.FeSimulations).ThenInclude(s => s.SeTasks).ThenInclude(t => t.SimTask)
                .Where(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true)
                .ToListAsync();

            var results = new List<ClassSimulationResultDto>();

            foreach (var exam in exams)
            {
                var simPartial = exam.FinalExamPartials.FirstOrDefault(p => p.Type == 2);
                var trainee = exam.Enrollment.Trainee;

                var dto = new ClassSimulationResultDto
                {
                    TraineeId = trainee.Id,
                    TraineeCode = trainee.TraineeCode,
                    TraineeName = trainee.IdNavigation?.Fullname,
                    AvatarUrl = trainee.IdNavigation?.AvatarUrl,
                    SimulationResult = simPartial != null ? MapToSimulationDetailDto(simPartial) : null
                };
                results.Add(dto);
            }

            return results;
        }

        private async Task<FinalExamPartial> GetPartialWithSecurityCheckAsync(int partialId, int userId)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.FinalExam.Enrollment.TraineeId != userId) throw new UnauthorizedAccessException("This exam does not belong to the current user.");

            var learningProgress = await _uow.LearningProgressRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == partial.FinalExam.EnrollmentId);

            if (learningProgress == null)
            {
                throw new InvalidOperationException($"Learning progress not found for EnrollmentId: {partial.FinalExam.EnrollmentId} (PartialId: {partialId}, UserId: {userId})");
            }

            if (learningProgress.Status != (int)LearningProgressStatusEnum.Completed)
            {
                throw new InvalidOperationException($"Learning progress must be completed before taking the Simulation Exam. Current Status: {learningProgress.Status} (LP_ID: {learningProgress.Id}, EnrollmentId: {partial.FinalExam.EnrollmentId})");
            }

            return partial;
        }

        private SimulationExamDetailDto MapToSimulationDetailDto(FinalExamPartial p)
        {
            var sim = p.FeSimulations.FirstOrDefault();
            var baseDto = MapToPartialDto(p, true);

            var detailDto = new SimulationExamDetailDto
            {
                Id = baseDto.Id,
                Type = baseDto.Type,
                Marks = baseDto.Marks,
                ExamWeight = baseDto.ExamWeight,
                Duration = baseDto.Duration,
                StartTime = baseDto.StartTime,
                EndTime = baseDto.EndTime,
                CompleteTime = baseDto.CompleteTime,
                Status = baseDto.Status,
                IsPass = baseDto.IsPass,
                QuizId = baseDto.QuizId,
                QuizName = baseDto.QuizName,
                PracticeId = baseDto.PracticeId,
                PracticeName = baseDto.PracticeName,
                Tasks = baseDto.Tasks,

                PracticeInfo = sim?.Practice != null ? new PracticeInfoDetailDto
                {
                    Id = sim.Practice.Id,
                    PracticeName = sim.Practice.PracticeName,
                    PracticeCode = sim.Practice.PracticeCode,
                    Description = sim.Practice.PracticeDescription,
                    DifficultyLevel = sim.Practice.DifficultyLevel
                } : null
            };

            return detailDto;
        }

        private string GetFinalExamPartialStatusName(int statusId)
        {
            return statusId switch { 0 => "NotYet", 1 => "Submitted", 2 => "Approved", _ => "Unknown" };
        }

        private string GetTypeName(int typeId)
        {
            return typeId switch { 1 => "Theory", 2 => "Simulation", 3 => "Practical", _ => "Unknown" };
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
    }
}