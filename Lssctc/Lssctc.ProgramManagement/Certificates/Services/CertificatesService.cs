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

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var entity = await _context.Certificates.FindAsync(id);
            if (entity == null) return false;

            entity.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }
    }
}