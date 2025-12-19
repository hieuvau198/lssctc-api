using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
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
                Status = 1 // Active
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
                // If for some reason it doesn't exist, create it
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

            // --- 2. Ensure Student Final Exams & Partials Exist (Reset Logic) ---
            var classInfo = await _uow.ClassRepository.GetByIdAsync(classId);
            var defaultTime = classInfo?.EndDate ?? DateTime.UtcNow.AddMonths(1);

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

                // Ensure Partials match the Template
                foreach (var partTemplate in template.FinalExamPartialsTemplates)
                {
                    var existingPartial = finalExam.FinalExamPartials.FirstOrDefault(p => p.Type == partTemplate.Type);

                    if (existingPartial == null)
                    {
                        var newPartial = new FinalExamPartial
                        {
                            FinalExamId = finalExam.Id,
                            Type = partTemplate.Type,
                            ExamWeight = partTemplate.Weight,
                            Marks = 0,
                            Duration = 60,
                            StartTime = defaultTime,
                            EndTime = defaultTime.AddMinutes(60),
                            Status = (int)FinalExamPartialStatus.NotYet
                        };
                        await _uow.FinalExamPartialRepository.CreateAsync(newPartial);
                    }
                    else
                    {
                        // Sync weight from template if resetting/checking
                        if (existingPartial.ExamWeight != partTemplate.Weight)
                        {
                            existingPartial.ExamWeight = partTemplate.Weight;
                            await _uow.FinalExamPartialRepository.UpdateAsync(existingPartial);
                        }
                    }
                }
            }
            await _uow.SaveChangesAsync();
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

        private string GetTypeName(int typeId) => typeId switch
        {
            1 => "Theory",
            2 => "Simulation",
            3 => "Practical",
            _ => "Unknown"
        };
    }
}