using Lssctc.Share.Contexts;
using Lssctc.Share.Entities;
using Lssctc.ProgramManagement.Certificates.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public class CertificatesService : ICertificatesService
    {
        private readonly LssctcDbContext _context;

        public CertificatesService(LssctcDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CertificateTemplateDto>> GetAllTemplatesAsync()
        {
            return await _context.Certificates
                .Where(c => c.IsActive == true)
                .Select(c => new CertificateTemplateDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    TemplateHtml = c.TemplateHtml,
                    IsActive = c.IsActive ?? false
                }).ToListAsync();
        }

        public async Task<CertificateTemplateDto> GetTemplateByIdAsync(int id)
        {
            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null) return null;

            return new CertificateTemplateDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                TemplateHtml = entity.TemplateHtml,
                IsActive = entity.IsActive ?? false
            };
        }

        public async Task<CertificateTemplateDto> CreateTemplateAsync(CreateCertificateTemplateDto dto)
        {
            var entity = new Certificate
            {
                Name = dto.Name,
                Description = dto.Description,
                TemplateHtml = dto.TemplateHtml,
                IsActive = true
            };

            _context.Certificates.Add(entity);
            await _context.SaveChangesAsync();

            return await GetTemplateByIdAsync(entity.Id);
        }

        public async Task<CertificateTemplateDto> UpdateTemplateAsync(int id, CreateCertificateTemplateDto dto)
        {
            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null || entity.IsActive == false) return null;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.TemplateHtml = dto.TemplateHtml;

            _context.Certificates.Update(entity);
            await _context.SaveChangesAsync();

            return await GetTemplateByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CertificateTemplateDto> GetCertificateByCourseIdAsync(int courseId)
        {
            var certificate = await (from cc in _context.CourseCertificates
                                     join c in _context.Certificates on cc.CertificateId equals c.Id
                                     where cc.CourseId == courseId && cc.IsActive == true && c.IsActive == true
                                     select c).FirstOrDefaultAsync();

            if (certificate == null) return null;

            return new CertificateTemplateDto
            {
                Id = certificate.Id,
                Name = certificate.Name,
                Description = certificate.Description,
                TemplateHtml = certificate.TemplateHtml,
                IsActive = certificate.IsActive ?? false
            };
        }

        public async Task<CertificateTemplateDto> GetCertificateByClassIdAsync(int classId)
        {
            // Relationship: Class -> ProgramCourse -> Course -> CourseCertificate -> Certificate
            var certificate = await (from cls in _context.Classes
                                     join pc in _context.ProgramCourses on cls.ProgramCourseId equals pc.Id
                                     join cc in _context.CourseCertificates on pc.CourseId equals cc.CourseId
                                     join c in _context.Certificates on cc.CertificateId equals c.Id
                                     where cls.Id == classId && cc.IsActive == true && c.IsActive == true
                                     select c).FirstOrDefaultAsync();

            if (certificate == null) return null;

            return new CertificateTemplateDto
            {
                Id = certificate.Id,
                Name = certificate.Name,
                Description = certificate.Description,
                TemplateHtml = certificate.TemplateHtml,
                IsActive = certificate.IsActive ?? false
            };
        }

        public async Task<bool> AssignCertificateToCourseAsync(int courseId, int certificateId)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            var certificateExists = await _context.Certificates.AnyAsync(c => c.Id == certificateId && c.IsActive == true);

            if (!courseExists || !certificateExists) return false;

            // 1. Deactivate existing active certificates for this course (enforce single active certificate rule)
            var existingAssignments = await _context.CourseCertificates
                .Where(cc => cc.CourseId == courseId && cc.IsActive == true)
                .ToListAsync();

            if (existingAssignments.Any())
            {
                foreach (var assignment in existingAssignments)
                {
                    assignment.IsActive = false;
                }
            }

            // 2. Check if the specific assignment already exists (active or inactive)
            var targetAssignment = await _context.CourseCertificates
                .FirstOrDefaultAsync(cc => cc.CourseId == courseId && cc.CertificateId == certificateId);

            if (targetAssignment != null)
            {
                // Reactivate or keep active
                targetAssignment.IsActive = true;
                _context.CourseCertificates.Update(targetAssignment);
            }
            else
            {
                // Create new assignment
                var newAssignment = new CourseCertificate
                {
                    CourseId = courseId,
                    CertificateId = certificateId,
                    IsActive = true,
                    PassingScore = 0 // Default value, can be adjustable if needed
                };
                _context.CourseCertificates.Add(newAssignment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AutoAssignCertificateToCourseAsync(int courseId)
        {
            // Get the first valid certificate in the system
            var firstCertificate = await _context.Certificates
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (firstCertificate == null) return false;

            return await AssignCertificateToCourseAsync(courseId, firstCertificate.Id);
        }
    }
}