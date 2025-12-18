using Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Services
{
    public class FETemplateService : IFETemplateService
    {
        private readonly IUnitOfWork _uow;

        public FETemplateService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<FinalExamTemplateDto>> GetTemplatesByClassIdAsync(int classId)
        {
            var templates = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .Where(t => t.ClassId == classId)
                .ToListAsync();

            return templates.Select(t => new FinalExamTemplateDto
            {
                Id = t.Id,
                ClassId = t.ClassId,
                Status = t.Status,
                PartialTemplates = t.FinalExamPartialsTemplates.Select(pt => new FinalExamPartialsTemplateDto
                {
                    Id = pt.Id,
                    FinalExamTemplateId = pt.FinalExamTemplateId,
                    Type = pt.Type,
                    TypeName = GetTypeName(pt.Type),
                    Weight = pt.Weight
                }).ToList()
            });
        }

        public async Task ResetFinalExamAsync(int classId)
        {
            // --- 1. Ensure Final Exam Template Exists ---
            var template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (template == null)
            {
                template = new FinalExamTemplate
                {
                    ClassId = classId,
                    Status = 1 // Active
                };
                await _uow.FinalExamTemplateRepository.CreateAsync(template);
                await _uow.SaveChangesAsync(); // Save to get Id
            }

            // --- 2. Ensure Template Partials Exist (Default Weights) ---
            await EnsureTemplatePartialExists(template, 1, 30); // Theory
            await EnsureTemplatePartialExists(template, 2, 20); // Simulation
            await EnsureTemplatePartialExists(template, 3, 50); // Practical

            await _uow.SaveChangesAsync();

            // Refresh template with partials
            template = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstAsync(t => t.ClassId == classId);

            // --- 3. Ensure Student Final Exams & Partials Exist (Reset Logic) ---
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
                    var exists = finalExam.FinalExamPartials.Any(p => p.Type == partTemplate.Type);
                    if (!exists)
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
                // Since we are using Generic Repository, we add via the repo
                await _uow.FinalExamPartialsTemplateRepository.CreateAsync(partialTemplate);

                // Add to local list to prevent re-adding in same transaction loop if needed
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