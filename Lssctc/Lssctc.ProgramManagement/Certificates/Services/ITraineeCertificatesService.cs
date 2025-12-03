using Lssctc.ProgramManagement.Certificates.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Certificates.Services
{
    public interface ITraineeCertificatesService
    {
        Task<IEnumerable<TraineeCertificateResponseDto>> GetAllAsync();
        Task<TraineeCertificateResponseDto> GetByIdAsync(int id);
        Task<TraineeCertificateResponseDto> GetByCodeAsync(string code);
        Task<TraineeCertificateResponseDto> CreateCertificateAsync(CreateTraineeCertificateDto dto);
        Task<bool> DeleteCertificateAsync(int id);
    }
}