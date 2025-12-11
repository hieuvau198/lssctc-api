using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.Quizzes.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FinalExamsService : IFinalExamsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IQuizService _quizService;


        public FinalExamsService(IUnitOfWork uow, IQuizService quizService)
        {
            _uow = uow;
            _quizService = quizService;
        }

        #region Helpers and Security

        // Helper để lấy Partial Entity và kiểm tra quyền sở hữu
        private async Task<FinalExamPartial> GetPartialWithSecurityCheckAsync(int partialId, int userId)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.FinalExam.Enrollment.TraineeId != userId) throw new UnauthorizedAccessException("This exam does not belong to the current user.");
            return partial;
        }

        private static int ParseExamType(string type)
        {
            return type.Trim().ToLower() switch
            {
                "theory" => 1,
                "simulation" => 2,
                "practical" => 3,
                _ => throw new ArgumentException($"Invalid exam type '{type}'. Allowed: 'Theory', 'Simulation', 'Practical'.")
            };
        }

        private string GetTypeName(int typeId)
        {
            return typeId switch { 1 => "Theory", 2 => "Simulation", 3 => "Practical", _ => "Unknown" };
        }

        private string GetFinalExamPartialStatusName(int statusId)
        {
            return statusId switch { 0 => "NotYet", 1 => "Submitted", 2 => "Approved", _ => "Unknown" };
        }

        #endregion

        #region Auto Creation
        public async Task AutoCreateFinalExamsForClassAsync(int classId)
        {
            // 1. Get Class Info to determine default dates
            var classInfo = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classInfo == null) return;

            // Default time is Class EndDate. If null, fallback to StartDate + 1 month or DateTime.Now
            var defaultTime = classInfo.EndDate ?? classInfo.StartDate.AddMonths(1);

            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                // 2. Check or Create Final Exam
                var finalExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                    .FirstOrDefaultAsync(fe => fe.EnrollmentId == enrollment.Id);

                if (finalExam == null)
                {
                    finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        IsPass = null,
                        TotalMarks = 0,
                        CompleteTime = null
                    };
                    await _uow.FinalExamRepository.CreateAsync(finalExam);
                    await _uow.SaveChangesAsync(); // Save to generate Id
                }

                // 3. Create Default Partials (TE, SE, PE)
                // Type 1: Theory
                await EnsurePartialExists(finalExam.Id, 1, defaultTime);
                // Type 2: Simulation
                await EnsurePartialExists(finalExam.Id, 2, defaultTime);
                // Type 3: Practical
                await EnsurePartialExists(finalExam.Id, 3, defaultTime);
            }
            await _uow.SaveChangesAsync();
        }

        private async Task EnsurePartialExists(int finalExamId, int type, DateTime defaultTime)
        {
            var exists = await _uow.FinalExamPartialRepository.ExistsAsync(p => p.FinalExamId == finalExamId && p.Type == type);
            if (!exists)
            {
                var partial = new FinalExamPartial
                {
                    FinalExamId = finalExamId,
                    Type = type,
                    Marks = 0,
                    ExamWeight = 0, // Default weight
                    Duration = 60,  // Default duration
                    StartTime = defaultTime,
                    EndTime = defaultTime.AddMinutes(60),
                    Status = (int)FinalExamPartialStatus.NotYet
                };
                await _uow.FinalExamPartialRepository.CreateAsync(partial);
            }
        }
        #endregion

        #region FinalExam CRUD

        public async Task<FinalExamDto> CreateFinalExamAsync(CreateFinalExamDto dto)
        {
            var exists = await _uow.FinalExamRepository.ExistsAsync(fe => fe.EnrollmentId == dto.EnrollmentId);
            if (exists) throw new InvalidOperationException("Final Exam already exists for this enrollment.");

            var entity = new FinalExam
            {
                EnrollmentId = dto.EnrollmentId,
                TotalMarks = 0
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
            // [ADDED] Logic to auto-create exams if they don't exist
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
            // [ADDED] Logic to auto-create exams if they don't exist (ensures trainee sees the exam structure)
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

        public async Task<FinalExamPartialDto> GetFinalExamPartialByIdAsync(int id)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
               .Include(p => p.FeTheories).ThenInclude(t => t.Quiz)
               .Include(p => p.FeSimulations).ThenInclude(s => s.Practice)
               .Include(p => p.FeSimulations).ThenInclude(s => s.SeTasks).ThenInclude(t => t.SimTask)
               .Include(p => p.PeChecklists) // [ADDED]
               .FirstOrDefaultAsync(p => p.Id == id);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            return MapToPartialDto(partial, true);
        }

        public async Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId)
        {
            // Ensure exams exist
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable().AnyAsync(fe => fe.Enrollment.ClassId == classId);
            if (!hasExams) await AutoCreateFinalExamsForClassAsync(classId);

            var exampleExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories).ThenInclude(t => t.Quiz)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists) // Include Entities
                .FirstOrDefaultAsync(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true);

            var configDto = new ClassExamConfigDto { ClassId = classId };

            if (exampleExam != null)
            {
                foreach (var p in exampleExam.FinalExamPartials)
                {
                    var theory = p.FeTheories.FirstOrDefault();
                    var sim = p.FeSimulations.FirstOrDefault();

                    // [UPDATE] Map Checklist from Entities
                    List<PeChecklistItemDto>? checklist = null;
                    if (p.Type == 3 && p.PeChecklists != null)
                    {
                        checklist = p.PeChecklists.Select(c => new PeChecklistItemDto
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                            IsPass = c.IsPass
                        }).ToList();
                    }

                    configDto.PartialConfigs.Add(new FinalExamPartialConfigDto
                    {
                        Type = GetTypeName(p.Type ?? 0),
                        ExamWeight = p.ExamWeight,
                        Duration = p.Duration,
                        StartTime = p.StartTime,
                        EndTime = p.EndTime,
                        QuizId = theory?.QuizId,
                        QuizName = theory?.Quiz?.Name,
                        PracticeId = sim?.PracticeId,
                        PracticeName = sim?.Practice?.PracticeName,
                        Checklist = checklist
                    });
                }
            }
            return configDto;
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

        #endregion

        #region Partials CRUD & Bulk Config

        public async Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto)
        {
            int typeId = ParseExamType(dto.Type);

            var partial = new FinalExamPartial
            {
                FinalExamId = dto.FinalExamId,
                Type = typeId,
                ExamWeight = dto.ExamWeight,
                Duration = dto.Duration,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Marks = 0,
                IsPass = null,
                Status = (int)FinalExamPartialStatus.NotYet
            };

            await _uow.FinalExamPartialRepository.CreateAsync(partial);
            await _uow.SaveChangesAsync();

            if (typeId == 1 && dto.QuizId.HasValue)
            {
                await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = partial.Id, QuizId = dto.QuizId.Value, Name = "Theory Exam" });
            }
            else if (typeId == 2 && dto.PracticeId.HasValue)
            {
                await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = partial.Id, PracticeId = dto.PracticeId.Value, Name = "Simulation Exam" });
            }

            await _uow.SaveChangesAsync();
            return MapToPartialDto(partial, true);
        }

        public async Task<IEnumerable<FinalExamDto>> CreatePartialsForClassAsync(CreateClassPartialDto dto)
        {
            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Where(fe => fe.Enrollment.ClassId == dto.ClassId && fe.Enrollment.IsDeleted != true)
                .ToListAsync();

            if (!exams.Any()) throw new KeyNotFoundException("No final exams found for this class.");

            int typeId = ParseExamType(dto.Type);
            var updatedExams = new List<FinalExamDto>();

            foreach (var exam in exams)
            {
                var exists = await _uow.FinalExamPartialRepository.ExistsAsync(p => p.FinalExamId == exam.Id && p.Type == typeId);
                if (exists) continue;

                var partial = new FinalExamPartial
                {
                    FinalExamId = exam.Id,
                    Type = typeId,
                    ExamWeight = dto.ExamWeight,
                    Duration = dto.Duration,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Marks = 0,
                    IsPass = null,
                    Status = (int)FinalExamPartialStatus.NotYet
                };

                await _uow.FinalExamPartialRepository.CreateAsync(partial);
                await _uow.SaveChangesAsync();

                if (typeId == 1 && dto.QuizId.HasValue)
                    await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = partial.Id, QuizId = dto.QuizId.Value, Name = "Theory Exam" });
                else if (typeId == 2 && dto.PracticeId.HasValue)
                    await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = partial.Id, PracticeId = dto.PracticeId.Value, Name = "Simulation Exam" });

                updatedExams.Add(await GetFinalExamByIdAsync(exam.Id));
            }
            await _uow.SaveChangesAsync();
            return updatedExams;
        }

        public async Task UpdatePartialsConfigForClassAsync(UpdateClassPartialConfigDto dto)
        {
            int typeId = ParseExamType(dto.Type);

            // --- Validation 1: Check Class Existence ---
            var classExists = await _uow.ClassRepository.ExistsAsync(c => c.Id == dto.ClassId);
            if (!classExists) throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            // --- Validation 2: Check TE/SE Links Existence ---
            if (typeId == 1 && dto.QuizId.HasValue)
            {
                var quizExists = await _uow.QuizRepository.ExistsAsync(q => q.Id == dto.QuizId.Value);
                if (!quizExists) throw new KeyNotFoundException($"Quiz with ID {dto.QuizId.Value} not found for Theory Exam configuration.");
            }
            else if (typeId == 2 && dto.PracticeId.HasValue)
            {
                var practiceExists = await _uow.PracticeRepository.ExistsAsync(p => p.Id == dto.PracticeId.Value);
                if (!practiceExists) throw new KeyNotFoundException($"Practice/Simulation with ID {dto.PracticeId.Value} not found for Simulation Exam configuration.");
            }

            // --- Validation 3: Check PE Checklist Configuration ---
            if (typeId == 3 && (dto.ChecklistConfig == null || !dto.ChecklistConfig.Any()))
            {
                throw new ArgumentException("PE checklist configuration is required when updating Practical Exam type.");
            }

            var partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Where(p => p.FinalExam.Enrollment.ClassId == dto.ClassId && p.Type == typeId)
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .Include(p => p.PeChecklists) // Include current checklists to remove them
                .ToListAsync();

            if (!partials.Any())
                throw new KeyNotFoundException($"No Final Exam Partials of type '{dto.Type}' found for Class ID {dto.ClassId}.");

            foreach (var p in partials)
            {
                // Update basic config
                if (dto.ExamWeight.HasValue) p.ExamWeight = dto.ExamWeight;
                if (dto.Duration.HasValue) p.Duration = dto.Duration;
                if (dto.StartTime.HasValue) p.StartTime = dto.StartTime;
                if (dto.EndTime.HasValue) p.EndTime = dto.EndTime;

                // Update Links (Theory/Simulation)
                if (typeId == 1 && dto.QuizId.HasValue)
                {
                    var theory = p.FeTheories.FirstOrDefault();
                    if (theory != null) { theory.QuizId = dto.QuizId.Value; await _uow.FeTheoryRepository.UpdateAsync(theory); }
                    else { await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = p.Id, QuizId = dto.QuizId.Value }); }
                }
                else if (typeId == 2 && dto.PracticeId.HasValue)
                {
                    var sim = p.FeSimulations.FirstOrDefault();
                    if (sim != null) { sim.PracticeId = dto.PracticeId.Value; await _uow.FeSimulationRepository.UpdateAsync(sim); }
                    else { await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = p.Id, PracticeId = dto.PracticeId.Value }); }
                }

                // [UPDATE] Practical Exam (PE) Checklist Entity Update
                else if (typeId == 3 && dto.ChecklistConfig != null)
                {
                    // 1. Remove old checklists (Resetting template)
                    // Assuming _uow exposes a repository for PeChecklist or a generic repository method
                    // If generic repo is not directly exposed as property, we iterate and delete.
                    // Assuming _uow.PeChecklistRepository exists based on pattern, or using GenericRepository<PeChecklist>
                    // If your UoW doesn't have specific repo, use: _uow.GetRepository<PeChecklist>().DeleteRangeAsync(p.PeChecklists);

                    // For safety in this snippet, assuming standard specific repo or access via context
                    // Accessing via property assuming it was added to UoW
                    foreach (var oldChecklist in p.PeChecklists.ToList())
                    {
                        // Using a generic way if specific repo not known, but usually:
                        // await _uow.PeChecklistRepository.DeleteAsync(oldChecklist); 
                        // Or context.PeChecklists.Remove(oldChecklist);
                        // Let's assume a Repository exists on UoW or use DbContext directly if UoW exposes it.
                        _uow.GetDbContext().Set<PeChecklist>().Remove(oldChecklist);
                    }

                    // 2. Add new checklists from config
                    foreach (var item in dto.ChecklistConfig)
                    {
                        var newChecklist = new PeChecklist
                        {
                            FinalExamPartialId = p.Id,
                            Name = item.Name,
                            Description = item.Description,
                            IsPass = null // Reset status
                        };
                        _uow.GetDbContext().Set<PeChecklist>().Add(newChecklist);
                    }
                }

                await _uow.FinalExamPartialRepository.UpdateAsync(p);
            }
            await _uow.SaveChangesAsync();
        }

        public async Task<FinalExamPartialDto> UpdateFinalExamPartialAsync(int id, UpdateFinalExamPartialDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");

            if (dto.ExamWeight.HasValue) partial.ExamWeight = dto.ExamWeight;
            if (dto.Duration.HasValue) partial.Duration = dto.Duration;
            if (dto.StartTime.HasValue) partial.StartTime = dto.StartTime;
            if (dto.EndTime.HasValue) partial.EndTime = dto.EndTime;
            if (!string.IsNullOrEmpty(dto.Description)) partial.Description = dto.Description;

            if (dto.QuizId.HasValue && partial.Type == 1)
            {
                var theory = partial.FeTheories.FirstOrDefault();
                if (theory != null) { theory.QuizId = dto.QuizId.Value; await _uow.FeTheoryRepository.UpdateAsync(theory); }
                else { await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = id, QuizId = dto.QuizId.Value }); }
            }

            if (dto.PracticeId.HasValue && partial.Type == 2)
            {
                var sim = partial.FeSimulations.FirstOrDefault();
                if (sim != null) { sim.PracticeId = dto.PracticeId.Value; await _uow.FeSimulationRepository.UpdateAsync(sim); }
                else { await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = id, PracticeId = dto.PracticeId.Value }); }
            }

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await RecalculateFinalExamScore(partial.FinalExamId);

            return MapToPartialDto(partial, true);
        }

        public async Task DeleteFinalExamPartialAsync(int id)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(id);
            if (partial != null)
            {
                int examId = partial.FinalExamId;
                await _uow.FinalExamPartialRepository.DeleteAsync(partial);
                await _uow.SaveChangesAsync();
                await RecalculateFinalExamScore(examId);
            }
        }
        #endregion

        #region Trainee View Specific
        public async Task<IEnumerable<SePracticeListDto>> GetMySimulationExamPartialsByClassAsync(int classId, int userId)
        {
            // Auto-create exams to ensure the trainee always sees the structure
            var hasExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .AnyAsync(fe => fe.Enrollment.ClassId == classId);

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

                // Map Practice entity and FinalExamPartial entity fields to SePracticeListDto
                var dto = new SePracticeListDto
                {
                    // Fields from PracticeDto
                    id = practice.Id,
                    practiceName = practice.PracticeName,
                    practiceCode = practice.PracticeCode,
                    practiceDescription = practice.PracticeDescription ?? string.Empty,
                    estimatedDurationMinutes = partial.Duration ?? 0, // Use partial duration for exam context
                    difficultyLevel = practice.DifficultyLevel ?? string.Empty,
                    maxAttempts = practice.MaxAttempts ?? 0,
                    createdDate = practice.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                    isActive = practice.IsActive ?? false,
                    isCompleted = partial.CompleteTime.HasValue,

                    // Fields from FinalExamPartial
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
            if (partial.CompleteTime.HasValue) throw new InvalidOperationException("Exam is already complete.");

            // 1. Check Exam Code
            if (string.IsNullOrEmpty(partial.FinalExam.ExamCode) ||
                !partial.FinalExam.ExamCode.Equals(examCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid Exam Code.");
            }

            // 2. Set StartTime if not already set
            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
                await _uow.FinalExamPartialRepository.UpdateAsync(partial);
                await _uow.SaveChangesAsync();
            }

            return MapToPartialDto(partial, isInstructor: false);
        }

        public async Task<FinalExamPartialDto> GetFinalExamPartialByIdForTraineeAsync(int partialId, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            return MapToPartialDto(partial, isInstructor: false);
        }

        public async Task<List<PeChecklistItemDto>> GetPeSubmissionChecklistForTraineeAsync(int partialId, int userId)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.PeChecklists) // Include Entities
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.FinalExam.Enrollment.TraineeId != userId) throw new UnauthorizedAccessException("Access denied.");
            if (partial.Type != 3) throw new ArgumentException("Not a Practical Exam.");

            // Return list from entities
            return partial.PeChecklists.Select(c => new PeChecklistItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsPass = c.IsPass
            }).ToList();
        }

        public async Task<FinalExamPartialDto> StartSimulationExamAsync(int partialId, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");
            if (partial.CompleteTime.HasValue) throw new InvalidOperationException("Exam is already complete.");

            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
                await _uow.FinalExamPartialRepository.UpdateAsync(partial);
                await _uow.SaveChangesAsync();
            }

            return MapToPartialDto(partial, isInstructor: false);
        }

        #endregion

        #region Allow Retake

        public async Task<FinalExamDto> AllowPartialRetakeAsync(int partialId, string? note = null)
        {
            // 1. Get partial
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null)
                throw new KeyNotFoundException("Partial not found.");

            // 2. Validate it's a Theory exam
            if (partial.Type != 1)
                throw new ArgumentException("Only Theory partial can be reset.");

            // 3. Reset partial to allow retake
            partial.Status = (int)FinalExamPartialStatus.NotYet;
            partial.StartTime = null;
            partial.CompleteTime = null;
            partial.Marks = 0;
            partial.IsPass = null;

            // 4. Optionally append note to description
            if (!string.IsNullOrWhiteSpace(note))
            {
                if (string.IsNullOrWhiteSpace(partial.Description))
                {
                    partial.Description = $"Reset note: {note}";
                }
                else
                {
                    partial.Description += $"\nReset note: {note}";
                }
            }

            // 5. Update and save
            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // 6. Recalculate final exam score
            await RecalculateFinalExamScore(partial.FinalExamId);

            // 7. Return updated final exam
            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        #endregion

        #region Submissions & Quiz Service Delegation


        public async Task<object> GetTeQuizContentAsync(int partialId, string examCode, int userId)
        {
            // 1. Thực hiện Security Check và lấy QuizId
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

            if (partial.Type != 1) throw new ArgumentException("This ID is not a Theory Exam.");

            // Lấy QuizId từ FeTheory
            var feTheory = partial.FeTheories.FirstOrDefault();
            if (feTheory == null || feTheory.QuizId == 0) throw new KeyNotFoundException("Quiz content not assigned.");

            // 2. Kiểm tra mã đề
            if (string.IsNullOrEmpty(partial.FinalExam.ExamCode) ||
                !partial.FinalExam.ExamCode.Equals(examCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid Exam Code.");
            }

            // 3. Ghi lại StartTime nếu chưa có
            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
                await _uow.FinalExamPartialRepository.UpdateAsync(partial);
                await _uow.SaveChangesAsync();
            }

            // 4. Gọi Quiz Service để lấy nội dung Quiz
            var quizContent = await _quizService.GetQuizDetailForTrainee(feTheory.QuizId, CancellationToken.None);

            if (quizContent == null) throw new KeyNotFoundException("Quiz content not found in Quiz Service.");

            return quizContent;
        }
        public async Task<SeTaskDto> SubmitSeTaskByCodeAsync(int partialId, string taskCode, int userId, SubmitSeTaskDto dto)
        {
            // 1. Security Check
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");

            // 2. Find the SeTask via FeSimulation and SimTask Code
            // Assuming the relationship: FinalExamPartial -> FeSimulations -> SeTasks -> SimTask
            var seTask = await _uow.GetDbContext().Set<SeTask>()
                .Include(t => t.SimTask)
                .Include(t => t.FeSimulation)
                .FirstOrDefaultAsync(t => t.FeSimulation.FinalExamPartialId == partialId
                                       && t.SimTask.TaskCode == taskCode);

            if (seTask == null) throw new KeyNotFoundException($"Task with code '{taskCode}' not found in this exam.");
            var nowUtc7 = DateTime.UtcNow.AddHours(7);
            // 3. Update Task
            seTask.IsPass = dto.IsPass;
            seTask.DurationSecond = dto.DurationSecond;
            seTask.CompleteTime = DateTime.UtcNow;
            if (dto.DurationSecond.HasValue)
            {
                seTask.AttemptTime = nowUtc7.AddSeconds(-dto.DurationSecond.Value);
            }
            else
            {
                seTask.AttemptTime = nowUtc7;
            }
            seTask.Status = 1; // Assuming 1 = Submitted/Completed

            // 4. Save
            // Assuming direct DbSet access or a generic repo available via _uow
            _uow.GetDbContext().Set<SeTask>().Update(seTask);
            await _uow.SaveChangesAsync();

            // 5. Return DTO
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

            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam (SE).");
            var nowUtc7 = DateTime.UtcNow.AddHours(7);
            // 1. Optional: Bulk update tasks if provided in DTO
            if (dto.Tasks != null && dto.Tasks.Any())
            {
                var seTasks = await _uow.GetDbContext().Set<SeTask>()
                    .Include(t => t.SimTask)
                    .Include(t => t.FeSimulation)
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
                        taskEntity.Status = 1;
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

            // 2. Calculate Score based on SeTasks
            // Fetch fresh tasks to ensure we have the latest state (from single submits or bulk above)
            var allTasks = await _uow.GetDbContext().Set<SeTask>()
                .Where(t => t.FeSimulation.FinalExamPartialId == partialId)
                .ToListAsync();

            decimal calculatedMarks = 0;
            if (allTasks.Any())
            {
                int totalTasks = allTasks.Count;
                int passedTasks = allTasks.Count(t => t.IsPass == true);

                // Calculate score on 10-point scale
                calculatedMarks = totalTasks > 0
                    ? Math.Round(((decimal)passedTasks / totalTasks) * 10, 2)
                    : 0;
            }
            else
            {
                
                calculatedMarks = (decimal)dto.Marks;
            }

            // 3. Update FinalExamPartial
            partial.Marks = calculatedMarks;
            partial.IsPass = dto.IsPass;
            partial.Description = dto.Description;
            partial.CompleteTime = nowUtc7;
            partial.Status = (int)FinalExamPartialStatus.Submitted;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // 4. Recalculate Final Exam total score
            await RecalculateFinalExamScore(partial.FinalExamId);

            // 5. Return updated partial DTO (with tasks)
            return await GetFinalExamPartialByIdAsync(partialId);
        }

        public async Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.PeChecklists)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial not found.");
            if (partial.Type != 3) throw new ArgumentException("Not a Practical Exam.");

            int passCount = 0;
            int totalCount = partial.PeChecklists.Count;

            // Update Checklist Entities
            foreach (var itemDto in dto.Checklist)
            {
                // Match by ID if possible, else by Name? Best to rely on ID passed back from FE.
                var entity = partial.PeChecklists.FirstOrDefault(c => c.Id == itemDto.Id);
                if (entity != null)
                {
                    entity.IsPass = itemDto.IsPass;
                    if (itemDto.IsPass == true) passCount++;

                    // Assuming simple EF Core tracking updates it, but explicitly:
                    _uow.GetDbContext().Entry(entity).State = EntityState.Modified;
                }
            }

            // Calculate Marks (based on Pass Count ratio)
            if (totalCount > 0)
            {
                partial.Marks = ((decimal)passCount / totalCount) * 10;
            }
            else
            {
                partial.Marks = 0;
            }

            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = dto.IsOverallPass; // Manual override from Instructor
            partial.Status = (int)FinalExamPartialStatus.Approved;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await RecalculateFinalExamScore(partial.FinalExamId);

            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto)
        {
            // 1. Get partial with security check
            var partial = await _uow.FinalExamPartialRepository
                .GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeTheories)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null)
                throw new KeyNotFoundException("Partial exam not found.");

            if (partial.FinalExam.Enrollment.TraineeId != userId)
                throw new UnauthorizedAccessException("This exam does not belong to the current user.");

            if (partial.Type != 1)
                throw new ArgumentException("This ID is not a Theory Exam.");

            // 2. Get FeTheory and QuizId
            var feTheory = partial.FeTheories.FirstOrDefault();
            if (feTheory == null || feTheory.QuizId == 0)
                throw new KeyNotFoundException("Quiz content not assigned to this exam.");

            // 3. Get quiz with full details including correct answers using GetQuizById (not GetQuizDetailForTrainee which hides IsCorrect)
            var quiz = await _quizService.GetQuizById(feTheory.QuizId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz with ID {feTheory.QuizId} not found.");

            // 4. Prepare lookup structures for grading
            var questionsById = quiz.Questions.ToDictionary(q => q.Id);
            
            // Map questionId -> Set of correct optionIds
            var correctOptionIdsByQuestion = new Dictionary<int, HashSet<int>>();
            
            // Map questionId -> question score
            var questionScoreByQuestion = new Dictionary<int, decimal>();
            
            foreach (var question in quiz.Questions)
            {
                var correctOptionIds = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id)
                    .ToHashSet();
                
                correctOptionIdsByQuestion[question.Id] = correctOptionIds;
                questionScoreByQuestion[question.Id] = question.QuestionScore ?? 0m;
            }

            // Calculate total possible score from quiz (fallback: sum question scores if TotalScore is null)
            decimal totalPossible = quiz.TotalScore ?? quiz.Questions.Sum(q => q.QuestionScore ?? 0m);
            
            if (totalPossible == 0)
            {
                // Edge case: no score defined, treat as 10
                totalPossible = 10m;
            }

            // 5. Grade the answers
            decimal totalObtained = 0m;

            // Handle null or empty answers (student submitted blank exam)
            if (dto.Answers != null && dto.Answers.Any())
            {
                foreach (var answer in dto.Answers)
                {
                    // Skip if question doesn't exist in quiz
                    if (!questionsById.ContainsKey(answer.QuestionId))
                        continue;

                    var question = questionsById[answer.QuestionId];
                    var correctOptions = correctOptionIdsByQuestion[answer.QuestionId];
                    var questionScore = questionScoreByQuestion[answer.QuestionId];

                    // Normalize answer: use OptionIds if provided, otherwise convert OptionId to list
                    HashSet<int> studentAnswerIds;
                    
                    if (answer.OptionIds != null && answer.OptionIds.Any())
                    {
                        // Multiple choice answer provided
                        studentAnswerIds = answer.OptionIds.ToHashSet();
                    }
                    else if (answer.OptionId.HasValue)
                    {
                        // Single choice answer provided (backward compatibility)
                        studentAnswerIds = new HashSet<int> { answer.OptionId.Value };
                    }
                    else
                    {
                        // No answer provided for this question
                        continue;
                    }

                    // Grading logic:
                    // For full credit: student's answer set must exactly match correct answer set
                    // (This implements strict exact-match grading as specified)
                    if (studentAnswerIds.SetEquals(correctOptions))
                    {
                        totalObtained += questionScore;
                    }
                    // Optional: implement partial credit for multiple choice
                    // Currently: no partial credit (0 points if not exact match)
                }
            }

            // 6. Normalize score to 0-10 scale
            decimal marks = totalPossible > 0 
                ? Math.Round((totalObtained / totalPossible) * 10m, 2) 
                : 0m;

            // 7. Determine IsPass based on quiz PassScoreCriteria
            // Assumption: quiz.PassScoreCriteria is stored as a value on 0-10 scale (matching our normalized marks)
            // If PassScoreCriteria is stored as percentage (0-100), adjust accordingly
            bool isPass;
            
            if (quiz.PassScoreCriteria.HasValue)
            {
                // PassScoreCriteria interpretation:
                // If it's >= 10, assume it's percentage (0-100) and convert to 0-10
                // Otherwise, use directly as threshold on 0-10 scale
                decimal passThreshold = quiz.PassScoreCriteria.Value;
                
                if (passThreshold > 10m)
                {
                    // Stored as percentage (0-100), convert to 0-10
                    passThreshold = (passThreshold / 10m);
                }
                
                isPass = marks >= passThreshold;
            }
            else
            {
                // Default: pass if score >= 5 out of 10
                isPass = marks >= 5m;
            }

            // 8. Update partial exam
            partial.Marks = marks;
            partial.Status = (int)FinalExamPartialStatus.Submitted;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = isPass;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // 9. Recalculate final exam total score
            await RecalculateFinalExamScore(partial.FinalExamId);

            // 10. Return updated final exam DTO
            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitSeAsync(int partialId, SubmitSeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");

            partial.Marks = dto.Marks;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = partial.Marks >= 5;
            partial.Status = (int)FinalExamPartialStatus.Submitted;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await RecalculateFinalExamScore(partial.FinalExamId);
            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        private async Task RecalculateFinalExamScore(int finalExamId)
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

        private async Task RecalculateFinalExamScoreInternal(FinalExam finalExam)
        {
            decimal total = 0;
            foreach (var p in finalExam.FinalExamPartials)
            {
                total += (p.Marks ?? 0) * ((p.ExamWeight ?? 0) / 100m);
            }
            finalExam.TotalMarks = total;
            finalExam.IsPass = finalExam.TotalMarks >= 5;

            // Check if all partials are Approved/Submitted to mark the FinalExam as Complete (using CompleteTime field)
            if (finalExam.FinalExamPartials.Any() &&
                finalExam.FinalExamPartials.All(p => p.Status == (int)FinalExamPartialStatus.Approved || p.Status == (int)FinalExamPartialStatus.Submitted))
            {
                finalExam.CompleteTime = DateTime.UtcNow;
            }
        }

        #endregion

        #region Helpers

        private IQueryable<FinalExam> GetFinalExamQuery()
        {
            // Included PeChecklists
            return _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists); // [ADDED]
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
                Partials = entity.FinalExamPartials?.Select(p => MapToPartialDto(p, isInstructor)).ToList() ?? new List<FinalExamPartialDto>()
            };
        }

        private FinalExamPartialDto MapToPartialDto(FinalExamPartial p, bool isInstructor)
        {
            var theory = p.FeTheories.FirstOrDefault();
            var sim = p.FeSimulations.FirstOrDefault();
            int statusId = p.Status ?? 0;
            List<SeTaskDto>? tasks = null;
            if (p.Type == 2 && sim != null) // Simulation Type
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
                // Description field is less relevant for PE now, but kept for compatibility
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

                // [UPDATE] Map Checklists
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
        #endregion
    }
}