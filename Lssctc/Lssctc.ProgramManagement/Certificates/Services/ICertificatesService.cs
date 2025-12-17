using Lssctc.ProgramManagement.Certificates.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public interface ICertificatesService
    {
        // Existing methods
        Task<IEnumerable<CertificateTemplateDto>> GetAllTemplatesAsync();
        Task<CertificateTemplateDto> GetTemplateByIdAsync(int id);
        Task<CertificateTemplateDto> CreateTemplateAsync(CreateCertificateTemplateDto dto);
        Task<bool> DeleteTemplateAsync(int id);

        // New methods implemented
        Task<CertificateTemplateDto> UpdateTemplateAsync(int id, CreateCertificateTemplateDto dto);
        Task<CertificateTemplateDto> GetCertificateByCourseIdAsync(int courseId);
        Task<CertificateTemplateDto> GetCertificateByClassIdAsync(int classId);
        Task<bool> AssignCertificateToCourseAsync(int courseId, int certificateId);
        Task<bool> AutoAssignCertificateToCourseAsync(int courseId);
    }
}