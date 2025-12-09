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
            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var exists = await _uow.FinalExamRepository.ExistsAsync(fe => fe.EnrollmentId == enrollment.Id);
                if (!exists)
                {
                    var finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        IsPass = null,
                        TotalMarks = 0,
                        CompleteTime = null
                    };
                    await _uow.FinalExamRepository.CreateAsync(finalExam);
                }
            }
            await _uow.SaveChangesAsync();
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
            var entities = await GetFinalExamQuery()
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();
            return entities.Select(e => MapToDto(e, isInstructor: true));
        }

        public async Task<FinalExamDto?> GetMyFinalExamByClassAsync(int classId, int userId)
        {
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

        public async Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId)
        {
            // 1. Get ANY one final exam from this class (assuming all are synced)
            var exampleExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories).ThenInclude(t => t.Quiz)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .FirstOrDefaultAsync(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true);

            var configDto = new ClassExamConfigDto { ClassId = classId };

            if (exampleExam != null)
            {
                foreach (var p in exampleExam.FinalExamPartials)
                {
                    var theory = p.FeTheories.FirstOrDefault();
                    var sim = p.FeSimulations.FirstOrDefault();

                    // Parse Checklist if PE
                    List<PeChecklistItemDto>? checklist = null;
                    if (p.Type == 3 && !string.IsNullOrEmpty(p.Description))
                    {
                        try { checklist = JsonConvert.DeserializeObject<List<PeChecklistItemDto>>(p.Description); } catch { }
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
                throw new ArgumentException("PE checklist configuration (ChecklistConfig) is required when updating Practical Exam type.");
            }


            // --- Data Retrieval ---
            var partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Where(p => p.FinalExam.Enrollment.ClassId == dto.ClassId && p.Type == typeId)
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .ToListAsync();

            if (!partials.Any())
                throw new KeyNotFoundException($"No Final Exam Partials of type '{dto.Type}' found for Class ID {dto.ClassId}.");

            // --- PE Configuration ---
            string? jsonChecklist = null;
            if (typeId == 3 && dto.ChecklistConfig != null)
            {
                jsonChecklist = JsonConvert.SerializeObject(dto.ChecklistConfig);
            }

            // --- Bulk Update ---
            foreach (var p in partials)
            {
                // 1. Update common fields
                if (dto.ExamWeight.HasValue) p.ExamWeight = dto.ExamWeight;
                if (dto.Duration.HasValue) p.Duration = dto.Duration;
                if (dto.StartTime.HasValue) p.StartTime = dto.StartTime; 
                if (dto.EndTime.HasValue) p.EndTime = dto.EndTime;
                // 2. Update Type-Specific Links and Description
                if (typeId == 1 && dto.QuizId.HasValue) // Theory Exam (TE) Link update
                {
                    var theory = p.FeTheories.FirstOrDefault();
                    if (theory != null) { theory.QuizId = dto.QuizId.Value; await _uow.FeTheoryRepository.UpdateAsync(theory); }
                    else { await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = p.Id, QuizId = dto.QuizId.Value }); }
                }
                else if (typeId == 2 && dto.PracticeId.HasValue) // Simulation Exam (SE) Link update
                {
                    var sim = p.FeSimulations.FirstOrDefault();
                    if (sim != null) { sim.PracticeId = dto.PracticeId.Value; await _uow.FeSimulationRepository.UpdateAsync(sim); }
                    else { await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = p.Id, PracticeId = dto.PracticeId.Value }); }
                }
                else if (typeId == 3) // Practical Exam (PE) Description/Checklist update
                {
                    if (jsonChecklist != null)
                    {
                        p.Description = jsonChecklist;
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

        public async Task<FinalExamPartialDto> GetFinalExamPartialByIdForTraineeAsync(int partialId, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            return MapToPartialDto(partial, isInstructor: false);
        }

        public async Task<List<PeChecklistItemDto>> GetPeSubmissionChecklistForTraineeAsync(int partialId, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);
            if (partial.Type != 3) throw new ArgumentException("This ID is not a Practical Exam (PE).");
            if (string.IsNullOrEmpty(partial.Description))
                throw new InvalidOperationException("Practical Exam has not been submitted or graded yet.");

            try
            {
                return JsonConvert.DeserializeObject<List<PeChecklistItemDto>>(partial.Description) ?? new List<PeChecklistItemDto>();
            }
            catch
            {
                throw new InvalidOperationException("Failed to load submission details.");
            }
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

        public async Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial not found.");
            if (partial.Type != 3) throw new ArgumentException("Not a Practical Exam.");

            partial.Description = JsonConvert.SerializeObject(dto.Checklist);

            decimal totalScore = dto.Checklist.Sum(x => x.Score);
            decimal totalMax = dto.Checklist.Sum(x => x.MaxScore);

            partial.Marks = totalMax > 0 ? (totalScore / totalMax) * 10 : 0;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = dto.IsOverallPass;
            partial.Status = (int)FinalExamPartialStatus.Approved;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await RecalculateFinalExamScore(partial.FinalExamId);

            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial not found.");

            // Placeholder for QuizService Grading Logic
            decimal score = 0;

            partial.Marks = score;
            partial.Status = (int)FinalExamPartialStatus.Submitted;
            partial.CompleteTime = DateTime.UtcNow;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await RecalculateFinalExamScore(partial.FinalExamId);

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

        #region Mappers

        private IQueryable<FinalExam> GetFinalExamQuery()
        {
            // Loại bỏ .ThenInclude(t => t.Quiz) vì Quiz content sẽ được lấy qua IQuizService.
            return _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice);
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

            return new FinalExamPartialDto
            {
                Id = p.Id,
                Type = GetTypeName(p.Type ?? 0),
                Marks = p.Marks,
                ExamWeight = p.ExamWeight,
                Description = p.Description,
                Duration = p.Duration,
                StartTime = p.StartTime,        
                EndTime = p.EndTime,          
                CompleteTime = p.CompleteTime,
                Status = GetFinalExamPartialStatusName(statusId),

                QuizId = isInstructor ? theory?.QuizId : null,
                QuizName = isInstructor ? theory?.Quiz?.Name : null,
                PracticeId = sim?.PracticeId,
                PracticeName = sim?.Practice?.PracticeName
            };
        }
        #endregion
    }
}