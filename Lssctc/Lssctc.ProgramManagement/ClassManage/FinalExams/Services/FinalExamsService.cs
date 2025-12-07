using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FinalExamsService : IFinalExamsService
    {
        private readonly IUnitOfWork _uow;

        public FinalExamsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Auto Creation
        public async Task AutoCreateFinalExamsForClassAsync(int classId)
        {
            // Get all active enrollments for the class
            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                // Prevent duplicate creation
                var exists = await _uow.FinalExamRepository.ExistsAsync(fe => fe.EnrollmentId == enrollment.Id);
                if (!exists)
                {
                    var finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        IsPass = null, // Pending
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

            var entity = new FinalExam { EnrollmentId = dto.EnrollmentId, TotalMarks = 0 };
            await _uow.FinalExamRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return await GetFinalExamByIdAsync(entity.Id);
        }

        public async Task<FinalExamDto> GetFinalExamByIdAsync(int id)
        {
            var entity = await GetFinalExamQuery().FirstOrDefaultAsync(fe => fe.Id == id);
            if (entity == null) throw new KeyNotFoundException($"Final Exam {id} not found.");
            return MapToDto(entity);
        }

        public async Task<IEnumerable<FinalExamDto>> GetFinalExamsByClassAsync(int classId)
        {
            var entities = await GetFinalExamQuery()
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task<FinalExamDto?> GetFinalExamByEnrollmentAsync(int enrollmentId)
        {
            var entity = await GetFinalExamQuery().FirstOrDefaultAsync(fe => fe.EnrollmentId == enrollmentId);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<FinalExamDto>> GetFinalExamsByTraineeAsync(int traineeId)
        {
            var entities = await GetFinalExamQuery()
                .Where(fe => fe.Enrollment.TraineeId == traineeId)
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task DeleteFinalExamAsync(int id)
        {
            var entity = await _uow.FinalExamRepository.GetByIdAsync(id);
            if (entity != null)
            {
                // Cascade delete implies deleting partials, done by DB or manually here if needed
                await _uow.FinalExamRepository.DeleteAsync(entity);
                await _uow.SaveChangesAsync();
            }
        }
        #endregion

        #region Partials CRUD
        public async Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto)
        {
            // 1. Convert String Type to Int
            int typeId = ParseExamType(dto.Type);

            // 2. Create Entity
            var partial = new FinalExamPartial
            {
                FinalExamId = dto.FinalExamId,
                Type = typeId, // Assigned parsed int value
                ExamWeight = dto.ExamWeight,
                Duration = dto.Duration,
                Marks = 0,
                IsPass = null
            };

            await _uow.FinalExamPartialRepository.CreateAsync(partial);
            await _uow.SaveChangesAsync();

            // 3. Link to Quiz/Practice based on parsed typeId
            if (typeId == 1 && dto.QuizId.HasValue) // 1 = Theory
            {
                await _uow.FeTheoryRepository.CreateAsync(new FeTheory
                {
                    FinalExamPartialId = partial.Id,
                    QuizId = dto.QuizId.Value,
                    Name = "Theory Exam"
                });
            }
            else if (typeId == 2 && dto.PracticeId.HasValue) // 2 = Simulation
            {
                await _uow.FeSimulationRepository.CreateAsync(new FeSimulation
                {
                    FinalExamPartialId = partial.Id,
                    PracticeId = dto.PracticeId.Value,
                    Name = "Simulation Exam"
                });
            }
            // PE (Type 3) doesn't need an external link table init here, handled via description/checklist later

            await _uow.SaveChangesAsync();
            return MapToPartialDto(partial);
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

            // Update associations if provided
            if (dto.QuizId.HasValue && partial.Type == 1)
            {
                var theory = partial.FeTheories.FirstOrDefault();
                if (theory != null)
                {
                    theory.QuizId = dto.QuizId.Value;
                    await _uow.FeTheoryRepository.UpdateAsync(theory);
                }
                else
                {
                    await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = id, QuizId = dto.QuizId.Value });
                }
            }

            if (dto.PracticeId.HasValue && partial.Type == 2)
            {
                var sim = partial.FeSimulations.FirstOrDefault();
                if (sim != null)
                {
                    sim.PracticeId = dto.PracticeId.Value;
                    await _uow.FeSimulationRepository.UpdateAsync(sim);
                }
                else
                {
                    await _uow.FeSimulationRepository.CreateAsync(new FeSimulation { FinalExamPartialId = id, PracticeId = dto.PracticeId.Value });
                }
            }

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // Recalculate total exam score in case weights changed
            await RecalculateFinalExamScore(partial.FinalExamId);

            return MapToPartialDto(partial);
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

        #region Submissions (TE, SE, PE)

        public List<string> GetPeChecklistCriteria()
        {
            return new List<string>
            {
                "Kỹ thuật điều khiển tốc độ",
                "Kỹ thuật nâng hạ",
                "Kỹ thuật đặt vị trí"
            };
        }

        public async Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.Type != 3) throw new ArgumentException("This ID does not correspond to a Practical Exam.");

            // 1. Save Checklist as JSON
            partial.Description = JsonConvert.SerializeObject(dto.Checklist);

            // 2. Calculate Score (Average of items scaled to 10)
            decimal totalScore = 0;
            decimal totalMax = 0;

            if (dto.Checklist != null && dto.Checklist.Any())
            {
                totalScore = dto.Checklist.Sum(x => x.Score);
                totalMax = dto.Checklist.Sum(x => x.MaxScore);
            }

            // Avoid division by zero
            partial.Marks = totalMax > 0 ? (totalScore / totalMax) * 10 : 0;

            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = partial.Marks >= 5; // Assuming 5 is pass threshold

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            // 3. Update parent Exam
            await RecalculateFinalExamScore(partial.FinalExamId);

            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitTeAsync(int partialId, SubmitTeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.Type != 1) throw new ArgumentException("This ID is not a Theory Exam.");

            partial.Marks = dto.Marks;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = partial.Marks >= 5;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            await RecalculateFinalExamScore(partial.FinalExamId);
            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitSeAsync(int partialId, SubmitSeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.Type != 2) throw new ArgumentException("This ID is not a Simulation Exam.");

            partial.Marks = dto.Marks;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = partial.Marks >= 5;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            await RecalculateFinalExamScore(partial.FinalExamId);
            return await GetFinalExamByIdAsync(partial.FinalExamId);
        }

        private async Task RecalculateFinalExamScore(int finalExamId)
        {
            var finalExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .FirstOrDefaultAsync(fe => fe.Id == finalExamId);

            if (finalExam == null) return;

            decimal weightedTotal = 0;
            decimal totalWeight = 0;

            foreach (var p in finalExam.FinalExamPartials)
            {
                decimal weight = p.ExamWeight ?? 0;
                decimal marks = p.Marks ?? 0;

                // Formula: Marks * (Weight/100)
                weightedTotal += marks * (weight / 100m);
                totalWeight += weight;
            }

            finalExam.TotalMarks = weightedTotal;

            // Exam passed if total score >= 5 (and optionally if total weight was 100%)
            finalExam.IsPass = finalExam.TotalMarks >= 5;

            // If all partials are done, mark exam as complete
            if (finalExam.FinalExamPartials.Any() && finalExam.FinalExamPartials.All(p => p.CompleteTime.HasValue))
            {
                finalExam.CompleteTime = DateTime.UtcNow;
            }

            await _uow.FinalExamRepository.UpdateAsync(finalExam);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Helpers
        private IQueryable<FinalExam> GetFinalExamQuery()
        {
            return _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment).ThenInclude(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories).ThenInclude(t => t.Quiz)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice);
        }

        private FinalExamDto MapToDto(FinalExam entity)
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
                Partials = entity.FinalExamPartials?.Select(MapToPartialDto).ToList() ?? new List<FinalExamPartialDto>()
            };
        }

        private FinalExamPartialDto MapToPartialDto(FinalExamPartial p)
        {
            var theory = p.FeTheories.FirstOrDefault();
            var sim = p.FeSimulations.FirstOrDefault();

            return new FinalExamPartialDto
            {
                Id = p.Id,
                Type = p.Type ?? 0,
                Marks = p.Marks,
                ExamWeight = p.ExamWeight,
                Description = p.Description,
                Duration = p.Duration,
                QuizId = theory?.QuizId,
                QuizName = theory?.Quiz?.Name, // Assuming Quiz has Name
                PracticeId = sim?.PracticeId,
                PracticeName = sim?.Practice?.PracticeName // Assuming Practice has Name
            };
        }
        private static int ParseExamType(string type)
        {
            return type.Trim().ToLower() switch
            {
                "theory" => 1,
                "simulation" => 2,
                "practical" => 3,
                _ => throw new ArgumentException($"Invalid exam type '{type}'. Allowed values: 'Theory', 'Simulation', 'Practical'.")
            };
        }
        #endregion
    }
}