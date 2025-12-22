using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FETemplateService : IFETemplateService
    {
        private readonly IUnitOfWork _uow;

        public FETemplateService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<FinalExamTemplateDto?> GetTemplatesByClassIdAsync(int classId)
        {
            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (template == null) return null;

            return new FinalExamTemplateDto
            {
                Id = template.Id,
                ClassId = template.ClassId,
                Status = template.Status,
                PartialTemplates = template.FinalExamPartialsTemplates.Select(pt => new FinalExamPartialsTemplateDto
                {
                    Id = pt.Id,
                    FinalExamTemplateId = pt.FinalExamTemplateId,
                    Type = pt.Type,
                    TypeName = GetTypeName(pt.Type),
                    Weight = pt.Weight
                }).ToList()
            };
        }

        public async Task CreateTemplateAsync(int classId)
        {
            var exists = await _uow.FinalExamTemplateRepository.ExistsAsync(t => t.ClassId == classId);
            if (exists) return;

            var template = new FinalExamTemplate
            {
                ClassId = classId,
                Status = 1 // Active / NotYet
            };
            await _uow.FinalExamTemplateRepository.CreateAsync(template);
            await _uow.SaveChangesAsync();

            // Default Weights
            await EnsureTemplatePartialExists(template, 1, 30); // Theory
            await EnsureTemplatePartialExists(template, 2, 20); // Simulation
            await EnsureTemplatePartialExists(template, 3, 50); // Practical

            await _uow.SaveChangesAsync();
        }

        public async Task UpdateTemplatePartialAsync(int classId, int type, decimal weight)
        {
            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (template == null) throw new KeyNotFoundException($"Final Exam Template for class {classId} not found.");

            var partialTemplate = template.FinalExamPartialsTemplates.FirstOrDefault(p => p.Type == type);
            if (partialTemplate != null)
            {
                partialTemplate.Weight = weight;
                await _uow.FinalExamPartialsTemplateRepository.UpdateAsync(partialTemplate);
            }
            else
            {
                await EnsureTemplatePartialExists(template, type, weight);
            }
            await _uow.SaveChangesAsync();
        }

        public async Task ResetFinalExamAsync(int classId)
        {
            // --- 1. Ensure Final Exam Template Exists ---
            await CreateTemplateAsync(classId);

            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstAsync(t => t.ClassId == classId);

            // [FIX START] Check if the entity is already being tracked to avoid conflict
            var trackedTemplate = _uow.GetDbContext().ChangeTracker.Entries<FinalExamTemplate>()
                                    .FirstOrDefault(e => e.Entity.Id == template.Id)?.Entity;

            var targetTemplate = trackedTemplate ?? template;

            // Reset template status to NotYet if needed, or keep as is. Usually reset implies starting over.
            targetTemplate.Status = (int)FinalExamStatusEnum.NotYet;
            await _uow.FinalExamTemplateRepository.UpdateAsync(targetTemplate);
            // [FIX END]

            // --- 2. Ensure Student Final Exams & Partials Exist (Reset Logic) ---
            var classInfo = await _uow.ClassRepository.GetByIdAsync(classId);
            var defaultTime = classInfo?.EndDate ?? DateTime.UtcNow.AddMonths(1);

            var existingCodes = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Select(p => p.ExamCode)
                .Where(c => c != null)
                .ToListAsync();
            var codesSet = new HashSet<string>(existingCodes!);

            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.IsDeleted != true)
                .ToListAsync();

            foreach (var enrollment in enrollments)
            {
                var finalExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                    .Include(fe => fe.FinalExamPartials)
                    .FirstOrDefaultAsync(fe => fe.EnrollmentId == enrollment.Id);

                if (finalExam == null)
                {
                    finalExam = new FinalExam
                    {
                        EnrollmentId = enrollment.Id,
                        IsPass = null,
                        TotalMarks = 0,
                        Status = (int)FinalExamStatusEnum.NotYet
                    };
                    await _uow.FinalExamRepository.CreateAsync(finalExam);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    // If resetting, ensure status is NotYet
                    finalExam.Status = (int)FinalExamStatusEnum.NotYet;
                    finalExam.TotalMarks = 0;
                    finalExam.IsPass = null;
                    await _uow.FinalExamRepository.UpdateAsync(finalExam);
                }

                // Ensure Partials match the Template
                // Note: We use 'template' (the one with Included partials) for reading structure
                // But if 'trackedTemplate' had modifications to partials that weren't saved, we might need care.
                // Assuming structure is stable after CreateTemplateAsync.
                foreach (var partTemplate in template.FinalExamPartialsTemplates)
                {
                    var existingPartial = finalExam.FinalExamPartials.FirstOrDefault(p => p.Type == partTemplate.Type);

                    if (existingPartial == null)
                    {
                        var newCode = FEHelper.GenerateExamCode(codesSet);
                        codesSet.Add(newCode);

                        var newPartial = new FinalExamPartial
                        {
                            FinalExamId = finalExam.Id,
                            Type = partTemplate.Type,
                            ExamWeight = partTemplate.Weight,
                            Marks = 0,
                            Duration = 60,
                            StartTime = defaultTime,
                            EndTime = defaultTime.AddMinutes(60),
                            Status = (int)FinalExamPartialStatus.NotYet,
                            ExamCode = newCode
                        };
                        await _uow.FinalExamPartialRepository.CreateAsync(newPartial);
                    }
                    else
                    {
                        // Reset partial data
                        existingPartial.Status = (int)FinalExamPartialStatus.NotYet;
                        existingPartial.Marks = 0;
                        existingPartial.IsPass = null;

                        // Sync weight
                        if (existingPartial.ExamWeight != partTemplate.Weight)
                        {
                            existingPartial.ExamWeight = partTemplate.Weight;
                        }
                        await _uow.FinalExamPartialRepository.UpdateAsync(existingPartial);
                    }
                }
            }
            await _uow.SaveChangesAsync();
        }

        public async Task<ClassExamConfigDto> GetClassExamConfigAsync(int classId)
        {
            var template = await GetTemplatesByClassIdAsync(classId);
            if (template == null)
            {
                await CreateTemplateAsync(classId);
                template = await GetTemplatesByClassIdAsync(classId);
            }

            var exampleExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories).ThenInclude(t => t.Quiz)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations).ThenInclude(s => s.Practice)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists)
                .FirstOrDefaultAsync(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true);

            var configDto = new ClassExamConfigDto { ClassId = classId };

            if (template != null)
            {
                configDto.Status = GetFinalExamStatusName(template.Status);

                foreach (var tempPartial in template.PartialTemplates)
                {
                    var p = exampleExam?.FinalExamPartials.FirstOrDefault(ep => ep.Type == tempPartial.Type);

                    var theory = p?.FeTheories.FirstOrDefault();
                    var sim = p?.FeSimulations.FirstOrDefault();

                    List<PeChecklistItemDto>? checklist = null;
                    if (tempPartial.Type == 3 && p?.PeChecklists != null)
                    {
                        checklist = p.PeChecklists.Select(c => new PeChecklistItemDto
                        {
                            Id = c.Id,
                            Name = c.Name ?? "Unassigned",
                            Description = c.Description,
                            IsPass = c.IsPass
                        }).ToList();
                    }

                    configDto.PartialConfigs.Add(new FinalExamPartialConfigDto
                    {
                        Type = GetTypeName(tempPartial.Type),
                        ExamWeight = tempPartial.Weight,
                        Duration = p?.Duration,
                        StartTime = p?.StartTime,
                        EndTime = p?.EndTime,
                        ExamCode = p?.ExamCode,
                        QuizId = theory?.QuizId,
                        QuizName = theory?.Quiz?.Name,
                        PracticeId = sim?.PracticeId,
                        PracticeName = sim?.Practice?.PracticeName,
                        Checklist = checklist
                    });
                }
            }
            else if (exampleExam != null)
            {
                foreach (var p in exampleExam.FinalExamPartials)
                {
                    configDto.PartialConfigs.Add(new FinalExamPartialConfigDto
                    {
                        Type = GetTypeName(p.Type ?? 0),
                        ExamWeight = p.ExamWeight,
                        Duration = p.Duration,
                        ExamCode = p.ExamCode
                    });
                }
            }

            return configDto;
        }

        public async Task UpdatePartialsConfigForClassAsync(UpdateClassPartialConfigDto dto)
        {
            int typeId = ParseExamType(dto.Type);

            var classExists = await _uow.ClassRepository.ExistsAsync(c => c.Id == dto.ClassId);
            if (!classExists) throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            // 1. Update Template (Weights)
            await CreateTemplateAsync(dto.ClassId);

            if (dto.ExamWeight.HasValue)
            {
                await UpdateTemplatePartialAsync(dto.ClassId, typeId, dto.ExamWeight.Value);
            }

            // 2. Validate References
            if (typeId == 1 && dto.QuizId.HasValue)
            {
                var quizExists = await _uow.QuizRepository.ExistsAsync(q => q.Id == dto.QuizId.Value);
                if (!quizExists) throw new KeyNotFoundException($"Quiz with ID {dto.QuizId.Value} not found.");
            }
            else if (typeId == 2 && dto.PracticeId.HasValue)
            {
                var practiceExists = await _uow.PracticeRepository.ExistsAsync(p => p.Id == dto.PracticeId.Value);
                if (!practiceExists) throw new KeyNotFoundException($"Practice with ID {dto.PracticeId.Value} not found.");
            }

            // 3. Update All Student Partials
            var partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                .Where(p => p.FinalExam.Enrollment.ClassId == dto.ClassId && p.Type == typeId)
                .Include(p => p.FeTheories)
                .Include(p => p.FeSimulations)
                .Include(p => p.PeChecklists)
                .ToListAsync();

            if (!partials.Any())
            {
                await ResetFinalExamAsync(dto.ClassId);
                partials = await _uow.FinalExamPartialRepository.GetAllAsQueryable()
                    .Where(p => p.FinalExam.Enrollment.ClassId == dto.ClassId && p.Type == typeId)
                    .Include(p => p.FeTheories)
                    .Include(p => p.FeSimulations)
                    .Include(p => p.PeChecklists)
                    .ToListAsync();
            }

            var examIdsToRecalculate = new HashSet<int>();

            foreach (var p in partials)
            {
                if (dto.ExamWeight.HasValue) p.ExamWeight = dto.ExamWeight;
                if (dto.Duration.HasValue) p.Duration = dto.Duration;

                if (dto.StartTime.HasValue) p.StartTime = dto.StartTime.Value.AddHours(-7);
                if (dto.EndTime.HasValue) p.EndTime = dto.EndTime.Value.AddHours(-7);

                if (typeId == 1 && dto.QuizId.HasValue)
                {
                    var theory = p.FeTheories.FirstOrDefault();
                    if (theory != null) { theory.QuizId = dto.QuizId.Value; await _uow.FeTheoryRepository.UpdateAsync(theory); }
                    else { await _uow.FeTheoryRepository.CreateAsync(new FeTheory { FinalExamPartialId = p.Id, QuizId = dto.QuizId.Value }); }
                }
                else if (typeId == 2 && dto.PracticeId.HasValue)
                {
                    var sim = p.FeSimulations.FirstOrDefault();
                    if (sim != null)
                    {
                        sim.PracticeId = dto.PracticeId.Value;
                        await _uow.FeSimulationRepository.UpdateAsync(sim);
                        await EnsureSeTasksForSimulationAsync(sim.Id, dto.PracticeId.Value);
                    }
                    else
                    {
                        var newSim = new FeSimulation { FinalExamPartialId = p.Id, PracticeId = dto.PracticeId.Value };
                        await _uow.FeSimulationRepository.CreateAsync(newSim);
                        await _uow.SaveChangesAsync();
                        await EnsureSeTasksForSimulationAsync(newSim.Id, dto.PracticeId.Value);
                    }
                }
                else if (typeId == 3 && dto.ChecklistConfig != null && dto.ChecklistConfig.Any())
                {
                    if (p.PeChecklists != null)
                    {
                        foreach (var oldChecklist in p.PeChecklists.ToList())
                        {
                            _uow.GetDbContext().Set<PeChecklist>().Remove(oldChecklist);
                        }
                    }

                    foreach (var item in dto.ChecklistConfig)
                    {
                        _uow.GetDbContext().Set<PeChecklist>().Add(new PeChecklist
                        {
                            FinalExamPartialId = p.Id,
                            Name = item.Name,
                            Description = item.Description,
                            IsPass = null
                        });
                    }
                }

                await _uow.FinalExamPartialRepository.UpdateAsync(p);
                examIdsToRecalculate.Add(p.FinalExamId);
            }
            await _uow.SaveChangesAsync();

            await RecalculateScoresForExams(examIdsToRecalculate);
        }

        public async Task UpdateClassExamWeightsAsync(int classId, UpdateClassWeightsDto dto)
        {
            if (Math.Abs((dto.TheoryWeight + dto.SimulationWeight + dto.PracticalWeight) - 1.0m) > 0.0001m)
            {
                throw new InvalidOperationException("The sum of weights must be exactly 1.0 (100%).");
            }

            await CreateTemplateAsync(classId);

            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (template == null) throw new KeyNotFoundException($"Final Exam Template for class {classId} not found.");

            var trackedTemplate = _uow.GetDbContext().ChangeTracker.Entries<FinalExamTemplate>()
                                    .FirstOrDefault(e => e.Entity.Id == template.Id)?.Entity;

            var targetTemplate = trackedTemplate ?? template;

            UpdateOrAddPartial(targetTemplate, 1, dto.TheoryWeight * 100);
            UpdateOrAddPartial(targetTemplate, 2, dto.SimulationWeight * 100);
            UpdateOrAddPartial(targetTemplate, 3, dto.PracticalWeight * 100);

            if (trackedTemplate == null)
            {
                await _uow.FinalExamTemplateRepository.UpdateAsync(targetTemplate);
            }

            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .Where(fe => fe.Enrollment.ClassId == classId)
                .ToListAsync();

            if (!exams.Any())
            {
                await ResetFinalExamAsync(classId);
                exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                    .Include(fe => fe.FinalExamPartials)
                    .Where(fe => fe.Enrollment.ClassId == classId)
                    .ToListAsync();
            }

            foreach (var exam in exams)
            {
                foreach (var partial in exam.FinalExamPartials)
                {
                    switch (partial.Type)
                    {
                        case 1: partial.ExamWeight = dto.TheoryWeight * 100; break;
                        case 2: partial.ExamWeight = dto.SimulationWeight * 100; break;
                        case 3: partial.ExamWeight = dto.PracticalWeight * 100; break;
                    }
                }

                await RecalculateFinalExamScoreInternal(exam);
                await _uow.FinalExamRepository.UpdateAsync(exam);
            }

            await _uow.SaveChangesAsync();
        }

        private void UpdateOrAddPartial(FinalExamTemplate template, int type, decimal weight)
        {
            var partial = template.FinalExamPartialsTemplates.FirstOrDefault(p => p.Type == type);
            if (partial != null)
            {
                partial.Weight = weight;
            }
            else
            {
                template.FinalExamPartialsTemplates.Add(new FinalExamPartialsTemplate
                {
                    FinalExamTemplateId = template.Id,
                    Type = type,
                    Weight = weight
                });
            }
        }

        private async Task RecalculateScoresForExams(IEnumerable<int> examIds)
        {
            _uow.GetDbContext().ChangeTracker.Clear();

            var exams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials)
                .Where(fe => examIds.Contains(fe.Id))
                .ToListAsync();

            foreach (var exam in exams)
            {
                await RecalculateFinalExamScoreInternal(exam);
                await _uow.FinalExamRepository.UpdateAsync(exam);
            }
            await _uow.SaveChangesAsync();
        }

        private Task RecalculateFinalExamScoreInternal(FinalExam finalExam)
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
            return Task.CompletedTask;
        }

        private async Task EnsureTemplatePartialExists(FinalExamTemplate template, int type, decimal weight)
        {
            if (!template.FinalExamPartialsTemplates.Any(p => p.Type == type))
            {
                var partialTemplate = new FinalExamPartialsTemplate
                {
                    FinalExamTemplateId = template.Id,
                    Type = type,
                    Weight = weight
                };
                await _uow.FinalExamPartialsTemplateRepository.CreateAsync(partialTemplate);
                template.FinalExamPartialsTemplates.Add(partialTemplate);
            }
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

        private string GetTypeName(int typeId) => typeId switch
        {
            1 => "Theory",
            2 => "Simulation",
            3 => "Practical",
            _ => "Unknown"
        };

        // [FIXED] Status Mapping
        private string GetFinalExamStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "NotYet",
                2 => "Submitted",
                3 => "Completed",
                4 => "Cancelled",
                5 => "Open", // Added Open status
                _ => "Unknown"
            };
        }

        private async Task EnsureSeTasksForSimulationAsync(int feSimulationId, int practiceId)
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
    }
}