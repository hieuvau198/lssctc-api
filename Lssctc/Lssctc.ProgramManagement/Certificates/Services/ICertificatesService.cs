using Lssctc.ProgramManagement.Certificates.Dtos;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public interface ICertificatesService
    {
        Task<IEnumerable<CertificateTemplateDto>> GetAllTemplatesAsync();
        Task<CertificateTemplateDto> GetTemplateByIdAsync(int id);
        Task<CertificateTemplateDto> CreateTemplateAsync(CreateCertificateTemplateDto dto);
        Task<bool> DeleteTemplateAsync(int id);
    }
}
