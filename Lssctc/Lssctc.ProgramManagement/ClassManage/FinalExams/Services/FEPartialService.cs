using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.Quizzes.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FEPartialService : IFEPartialService
    {
        private readonly IUnitOfWork _uow;
        private readonly IQuizService _quizService;
        private readonly IFEService _finalExamsService;
        private readonly ISEService _finalExamSeService;
        private static readonly Random _random = new Random(); // Added for code generation

        public FEPartialService(
            IUnitOfWork uow,
            IQuizService quizService,
            IFEService finalExamsService,
            ISEService finalExamSeService)
        {
            _uow = uow;
            _quizService = quizService;
            _finalExamsService = finalExamsService;
            _finalExamSeService = finalExamSeService;
        }

        public async Task<FinalExamPartialDto> GetFinalExamPartialByIdAsync(int id)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
               .Include(p => p.FeTheories).ThenInclude(t => t.Quiz)
               .Include(p => p.FeSimulations).ThenInclude(s => s.Practice)
               .Include(p => p.FeSimulations).ThenInclude(s => s.SeTasks).ThenInclude(t => t.SimTask)
               .Include(p => p.PeChecklists)
               .FirstOrDefaultAsync(p => p.Id == id);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            return MapToPartialDto(partial, true);
        }

        public async Task<FinalExamPartialDto> CreateFinalExamPartialAsync(CreateFinalExamPartialDto dto)
        {
            var type = ParseExamType(dto.Type);
            int typeId = (int)type;

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

            if (type == FinalExamPartialType.Theory && dto.QuizId.HasValue)
            {
                await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = partial.Id, QuizId = dto.QuizId.Value, Name = "Theory Exam" });
            }
            else if (type == FinalExamPartialType.Simulation && dto.PracticeId.HasValue)
            {
                var feSim = new FeSimulation { FinalExamPartialId = partial.Id, PracticeId = dto.PracticeId.Value, Name = "Simulation Exam" };
                await _uow.FeSimulationRepository.CreateAsync(feSim);
                await _uow.SaveChangesAsync();
                await _finalExamSeService.EnsureSeTasksForSimulationAsync(feSim.Id, dto.PracticeId.Value);
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

            var type = ParseExamType(dto.Type);
            int typeId = (int)type;
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

                if (type == FinalExamPartialType.Theory && dto.QuizId.HasValue)
                    await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = partial.Id, QuizId = dto.QuizId.Value, Name = "Theory Exam" });
                else if (type == FinalExamPartialType.Simulation && dto.PracticeId.HasValue)
                {
                    var feSim = new FeSimulation { FinalExamPartialId = partial.Id, PracticeId = dto.PracticeId.Value, Name = "Simulation Exam" };
                    await _uow.FeSimulationRepository.CreateAsync(feSim);
                    await _uow.SaveChangesAsync();
                    await _finalExamSeService.EnsureSeTasksForSimulationAsync(feSim.Id, dto.PracticeId.Value);
                }

                updatedExams.Add(await _finalExamsService.GetFinalExamByIdAsync(exam.Id));
            }
            await _uow.SaveChangesAsync();
            return updatedExams;
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

            if (dto.StartTime.HasValue) partial.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) partial.EndTime = dto.EndTime.Value;

            if (!string.IsNullOrEmpty(dto.Description)) partial.Description = dto.Description;

            if (dto.QuizId.HasValue && partial.Type == (int)FinalExamPartialType.Theory)
            {
                var theory = partial.FeTheories.FirstOrDefault();
                if (theory != null) { theory.QuizId = dto.QuizId.Value; await _uow.FeTheoryRepository.UpdateAsync(theory); }
                else { await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = id, QuizId = dto.QuizId.Value }); }
            }

            if (dto.PracticeId.HasValue && partial.Type == (int)FinalExamPartialType.Simulation)
            {
                var sim = partial.FeSimulations.FirstOrDefault();
                if (sim != null)
                {
                    sim.PracticeId = dto.PracticeId.Value;
                    await _uow.FeSimulationRepository.UpdateAsync(sim);
                    await _finalExamSeService.EnsureSeTasksForSimulationAsync(sim.Id, dto.PracticeId.Value);
                }
                else
                {
                    var newSim = new FeSimulation { FinalExamPartialId = id, PracticeId = dto.PracticeId.Value };
                    await _uow.FeSimulationRepository.CreateAsync(newSim);
                    await _uow.SaveChangesAsync();
                    await _finalExamSeService.EnsureSeTasksForSimulationAsync(newSim.Id, dto.PracticeId.Value);
                }
            }

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);

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
                await _finalExamsService.RecalculateFinalExamScore(examId);
            }
        }

        public async Task<FinalExamDto> AllowPartialRetakeAsync(int partialId, string? note = null)
        {
            var partial = await _uow.FinalExamPartialRepository.GetByIdAsync(partialId);
            if (partial == null)
                throw new KeyNotFoundException("Partial not found.");

            if (partial.Type != (int)FinalExamPartialType.Theory)
                throw new ArgumentException("Only Theory partial can be reset.");

            partial.Status = (int)FinalExamPartialStatus.NotYet;
            partial.StartTime = null;
            partial.CompleteTime = null;
            partial.Marks = 0;
            partial.IsPass = null;

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

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);

            return await _finalExamsService.GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<object> GetTeQuizContentAsync(int partialId, string examCode, int userId)
        {
            var partial = await GetPartialWithSecurityCheckAsync(partialId, userId);

            if (partial.Type != (int)FinalExamPartialType.Theory)
                throw new ArgumentException("This ID is not a Theory Exam.");

            var learningProgress = await _uow.LearningProgressRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == partial.FinalExam.EnrollmentId);

            if (learningProgress == null || learningProgress.Status != (int)LearningProgressStatusEnum.Completed)
            {
                throw new InvalidOperationException("Learning progress must be completed before taking the Theory Exam.");
            }

            var feTheory = partial.FeTheories.FirstOrDefault();
            if (feTheory == null || feTheory.QuizId == 0) throw new KeyNotFoundException("Quiz content not assigned.");

            if (string.IsNullOrEmpty(partial.ExamCode) ||
                !partial.ExamCode.Equals(examCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid Exam Code.");
            }

            // [MODIFIED] Generate new code to prevent reuse
            partial.ExamCode = GenerateRandomCode();

            if (!partial.StartTime.HasValue)
            {
                partial.StartTime = DateTime.UtcNow;
            }

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();

            var quizContent = await _quizService.GetQuizDetailForTrainee(feTheory.QuizId, CancellationToken.None);

            if (quizContent == null) throw new KeyNotFoundException("Quiz content not found in Quiz Service.");

            return quizContent;
        }

        public async Task<FinalExamDto> SubmitTeAsync(int partialId, int userId, SubmitTeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository
                .GetAllAsQueryable()
                .Include(p => p.FinalExam).ThenInclude(fe => fe.Enrollment)
                .Include(p => p.FeTheories)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null)
                throw new KeyNotFoundException("Partial exam not found.");

            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot submit result. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }

            if (partial.FinalExam.Enrollment.TraineeId != userId)
                throw new UnauthorizedAccessException("This exam does not belong to the current user.");

            if (partial.Type != (int)FinalExamPartialType.Theory)
                throw new ArgumentException("This ID is not a Theory Exam.");

            var learningProgress = await _uow.LearningProgressRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == partial.FinalExam.EnrollmentId);

            if (learningProgress == null || learningProgress.Status != (int)LearningProgressStatusEnum.Completed)
            {
                throw new InvalidOperationException("Learning progress must be completed before submitting the Theory Exam.");
            }

            var feTheory = partial.FeTheories.FirstOrDefault();
            if (feTheory == null || feTheory.QuizId == 0)
                throw new KeyNotFoundException("Quiz content not assigned to this exam.");

            var quiz = await _quizService.GetQuizById(feTheory.QuizId);
            if (quiz == null)
                throw new KeyNotFoundException($"Quiz with ID {feTheory.QuizId} not found.");

            var questionsById = quiz.Questions.ToDictionary(q => q.Id);
            var correctOptionIdsByQuestion = new Dictionary<int, HashSet<int>>();
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

            decimal totalPossible = quiz.TotalScore ?? quiz.Questions.Sum(q => q.QuestionScore ?? 0m);
            if (totalPossible == 0) totalPossible = 10m;

            decimal totalObtained = 0m;

            if (dto.Answers != null && dto.Answers.Any())
            {
                foreach (var answer in dto.Answers)
                {
                    if (!questionsById.ContainsKey(answer.QuestionId)) continue;

                    var correctOptions = correctOptionIdsByQuestion[answer.QuestionId];
                    var questionScore = questionScoreByQuestion[answer.QuestionId];

                    HashSet<int> studentAnswerIds;
                    if (answer.OptionIds != null && answer.OptionIds.Any())
                    {
                        studentAnswerIds = answer.OptionIds.ToHashSet();
                    }
                    else if (answer.OptionId.HasValue)
                    {
                        studentAnswerIds = new HashSet<int> { answer.OptionId.Value };
                    }
                    else
                    {
                        continue;
                    }

                    if (studentAnswerIds.SetEquals(correctOptions))
                    {
                        totalObtained += questionScore;
                    }
                }
            }

            decimal marks = totalPossible > 0
                ? Math.Round((totalObtained / totalPossible) * 10m, 2)
                : 0m;

            bool isPass;
            if (quiz.PassScoreCriteria.HasValue)
            {
                decimal passThreshold = quiz.PassScoreCriteria.Value;
                if (passThreshold > 10m) passThreshold = (passThreshold / 10m);
                isPass = marks >= passThreshold;
            }
            else
            {
                isPass = marks >= 5m;
            }

            partial.Marks = marks;
            partial.Status = (int)FinalExamPartialStatus.Submitted;
            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = isPass;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);
            return await _finalExamsService.GetFinalExamByIdAsync(partial.FinalExamId);
        }

        public async Task<FinalExamDto> SubmitPeAsync(int partialId, SubmitPeDto dto)
        {
            var partial = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Include(p => p.FinalExam)
                .Include(p => p.PeChecklists)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial not found.");

            var feStatus = partial.FinalExam.Status;
            if (feStatus != (int)FinalExamStatusEnum.Open && feStatus != (int)FinalExamStatusEnum.Submitted)
            {
                throw new InvalidOperationException($"Cannot submit result. Final Exam status is '{((FinalExamStatusEnum)feStatus)}'. Allowed statuses: Open, Submitted.");
            }

            if (partial.Type != (int)FinalExamPartialType.Practical)
                throw new ArgumentException("Not a Practical Exam.");

            var learningProgress = await _uow.LearningProgressRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == partial.FinalExam.EnrollmentId);

            if (learningProgress == null || learningProgress.Status != (int)LearningProgressStatusEnum.Completed)
            {
                throw new InvalidOperationException("Learning progress must be completed before submitting the Practical Exam.");
            }

            int passCount = 0;
            int totalCount = partial.PeChecklists.Count;

            foreach (var itemDto in dto.Checklist)
            {
                var entity = partial.PeChecklists.FirstOrDefault(c => c.Id == itemDto.Id);
                if (entity != null)
                {
                    entity.IsPass = itemDto.IsPass;
                    if (itemDto.IsPass == true) passCount++;

                    _uow.GetDbContext().Entry(entity).State = EntityState.Modified;
                }
            }

            if (totalCount > 0)
            {
                partial.Marks = ((decimal)passCount / totalCount) * 10;
            }
            else
            {
                partial.Marks = 0;
            }

            partial.CompleteTime = DateTime.UtcNow;
            partial.IsPass = dto.IsOverallPass;
            partial.Status = (int)FinalExamPartialStatus.Approved;

            await _uow.FinalExamPartialRepository.UpdateAsync(partial);
            await _uow.SaveChangesAsync();
            await _finalExamsService.RecalculateFinalExamScore(partial.FinalExamId);

            return await _finalExamsService.GetFinalExamByIdAsync(partial.FinalExamId);
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
                .Include(p => p.PeChecklists)
                .FirstOrDefaultAsync(p => p.Id == partialId);

            if (partial == null) throw new KeyNotFoundException("Partial exam not found.");
            if (partial.FinalExam.Enrollment.TraineeId != userId) throw new UnauthorizedAccessException("Access denied.");
            if (partial.Type != (int)FinalExamPartialType.Practical)
                throw new ArgumentException("Not a Practical Exam.");

            return partial.PeChecklists.Select(c => new PeChecklistItemDto
            {
                Id = c.Id,
                Name = c.Name ?? "Unassigned",
                Description = c.Description,
                IsPass = c.IsPass
            }).ToList();
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
            return partial;
        }

        private static FinalExamPartialType ParseExamType(string type)
        {
            return type.Trim().ToLower() switch
            {
                "theory" => FinalExamPartialType.Theory,
                "simulation" => FinalExamPartialType.Simulation,
                "practical" => FinalExamPartialType.Practical,
                _ => throw new ArgumentException($"Invalid exam type '{type}'. Allowed: 'Theory', 'Simulation', 'Practical'.")
            };
        }

        private string GetTypeName(int typeId)
        {
            if (Enum.IsDefined(typeof(FinalExamPartialType), typeId))
            {
                return ((FinalExamPartialType)typeId).ToString();
            }
            return "Unknown";
        }

        private string GetFinalExamPartialStatusName(int statusId)
        {
            if (Enum.IsDefined(typeof(FinalExamPartialStatus), statusId))
            {
                return ((FinalExamPartialStatus)statusId).ToString();
            }
            return "Unknown";
        }

        private FinalExamPartialDto MapToPartialDto(FinalExamPartial p, bool isInstructor)
        {
            var theory = p.FeTheories.FirstOrDefault();
            var sim = p.FeSimulations.FirstOrDefault();
            int statusId = p.Status ?? 0;
            List<SeTaskDto>? tasks = null;
            if (p.Type == (int)FinalExamPartialType.Simulation && sim != null)
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
                    Name = c.Name ?? "Unassigned",
                    Description = c.Description,
                    IsPass = c.IsPass
                }).ToList(),
                Tasks = tasks
            };
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}