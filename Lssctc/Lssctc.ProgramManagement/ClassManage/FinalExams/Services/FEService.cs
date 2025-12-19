using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FEService : IFEService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFETemplateService _feTemplateService;

        public FEService(IUnitOfWork uow, IFETemplateService feTemplateService)
        {
            _uow = uow;
            _feTemplateService = feTemplateService;
        }

        public async Task AutoCreateFinalExamsForClassAsync(int classId)
        {
            await _feTemplateService.ResetFinalExamAsync(classId);
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

        public async Task<string> GenerateExamCodeAsync(int fePartialId)
        {
            var entity = await _uow.FinalExamPartialRepository.GetByIdAsync(fePartialId);
            if (entity == null) throw new KeyNotFoundException("Final Exam Partial not found.");

            // Fetch existing codes to ensure uniqueness
            var existingCodes = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Select(p => p.ExamCode)
                .Where(code => code != null)
                .ToListAsync();

            string code = FEHelper.GenerateExamCode(existingCodes!);
            entity.ExamCode = code;

            await _uow.FinalExamPartialRepository.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return code;
        }

        public async Task FinishFinalExamAsync(int classId)
        {
            // 1. Update Final Exam Template Status to Completed
            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (template != null)
            {
                template.Status = (int)FinalExamStatusEnum.Completed;
                await _uow.FinalExamTemplateRepository.UpdateAsync(template);
            }

            // 2. Conclude and Calculate Student Exams
            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();

            foreach (var exam in exams)
            {
                // Calculate scores based on current partials
                RecalculateFinalExamScoreInternal(exam);

                // Force status to Completed as the class exam is being finished by instructor
                exam.Status = (int)FinalExamStatusEnum.Completed;

                if (!exam.CompleteTime.HasValue)
                {
                    exam.CompleteTime = DateTime.UtcNow;
                }

                await _uow.FinalExamRepository.UpdateAsync(exam);
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
                RecalculateFinalExamScoreInternal(finalExam);
                await _uow.FinalExamRepository.UpdateAsync(finalExam);
                await _uow.SaveChangesAsync();
            }
        }

        private void RecalculateFinalExamScoreInternal(FinalExam finalExam)
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

                ExamCode = isInstructor ? p.ExamCode : null, // [ADDED]

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