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
            if (partial.Type != 2) throw new ArgumentException("Đây không phải bài kiểm tra mô phỏng.");

            // Check Final Exam Status ---
            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Không thể bắt đầu bài làm,  '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }

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

            var seTask = await _uow.SeTaskRepository.GetAllAsQueryable()
                .Include(t => t.SimTask)
                .Include(t => t.FeSimulation)
                .FirstOrDefaultAsync(t => t.FeSimulation.FinalExamPartialId == partialId
                                        && t.SimTask.TaskCode == taskCode);

            if (seTask == null) throw new KeyNotFoundException($"Task with code '{taskCode}' not found in this exam.");

            // Update status
            seTask.IsPass = dto.IsPass;
            seTask.DurationSecond = dto.DurationSecond;
            seTask.Status = 1; // Mark as Attempted/Submitted

            seTask.CompleteTime = null;

            seTask.AttemptTime = null;

            await _uow.SeTaskRepository.UpdateAsync(seTask);
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
            // This method ALREADY loads FeSimulations and SeTasks
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

            // Validation: Check Final Exam Status
            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot submit result. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }

            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");

            // [FIX] Use the tasks ALREADY loaded in the partial graph. Do not fetch a new list.
            var existingTasks = partial.FeSimulations.SelectMany(s => s.SeTasks).ToList();

            // 1. Update individual Task statuses
            if (dto.Tasks != null && dto.Tasks.Any())
            {
                foreach (var taskDto in dto.Tasks)
                {
                    // Find the task inside the loaded graph
                    var taskEntity = existingTasks.FirstOrDefault(t =>
                        t.SimTask != null &&
                        t.SimTask.TaskCode.Equals(taskDto.TaskCode, StringComparison.OrdinalIgnoreCase));

                    if (taskEntity != null)
                    {
                        // Updating properties on the graph object directly
                        taskEntity.IsPass = taskDto.IsPass;
                        taskEntity.DurationSecond = taskDto.DurationSecond;
                        taskEntity.Status = 1; // Mark as attempted

                        var taskCompletionTime = dto.CompleteTime;
                        taskEntity.CompleteTime = taskCompletionTime != null ? taskCompletionTime.Value.AddHours(7) : DateTime.UtcNow.AddHours(7);

                        if (taskDto.DurationSecond.HasValue && taskCompletionTime.HasValue)
                            taskEntity.AttemptTime = taskCompletionTime.Value.AddSeconds(-taskDto.DurationSecond.Value);
                        else
                            taskEntity.AttemptTime = taskCompletionTime;

                        // Note: We do NOT need to call _uow.SeTaskRepository.UpdateAsync(taskEntity) here.
                        // The UpdateAsync(partial) call at the end will handle the whole graph.
                    }
                }
            }

            // 2. Calculate Overall Score
            decimal calculatedMarks = 0;
            if (dto.IsPass)
            {
                int totalTasksCount = existingTasks.Count;

                if (dto.Tasks != null && dto.Tasks.Any() && totalTasksCount > 0)
                {
                    decimal scorePerTask = 10m / totalTasksCount;
                    foreach (var taskDto in dto.Tasks)
                    {
                        if (taskDto.IsPass)
                        {
                            decimal deduction = taskDto.Mistake * 0.5m;
                            decimal taskScore = scorePerTask - deduction;
                            if (taskScore < 0) taskScore = 0;
                            calculatedMarks += taskScore;
                        }
                    }
                }
                else
                {
                    decimal deduction = dto.TotalMistake * 0.5m;
                    calculatedMarks = 10m - deduction;
                }

                if (calculatedMarks < 0) calculatedMarks = 0;
                if (calculatedMarks > 10) calculatedMarks = 10;
            }

            // 3. Update Partial Exam Result
            partial.Marks = calculatedMarks;
            partial.IsPass = dto.IsPass;
            partial.Description = dto.Description;
            partial.StartTime = dto.StartTime != null ? dto.StartTime.Value.AddHours(7) : DateTime.UtcNow.AddHours(7);
            partial.CompleteTime = dto.CompleteTime != null ? dto.CompleteTime.Value.AddHours(7) : DateTime.UtcNow.AddHours(7);
            partial.Status = (int)FinalExamPartialStatus.Submitted;

            // [FIX] This Single Update call saves the partial AND the modified SeTasks in its graph
            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // Auto generate new code after submission
            await _finalExamsService.GenerateExamCodeAsync(partialId);

            // Recalculate the Final Exam total score
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);

            // Reload partial to ensure response includes the latest state from DB
            partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

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
                    .ThenInclude(s => s.SeTasks)      
                    .ThenInclude(t => t.SimTask)      
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