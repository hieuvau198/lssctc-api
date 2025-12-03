using Lssctc.ProgramManagement.Programs.Dtos;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramImportService
    {
        Task<ProgramDto> ImportProgramFromExcelAsync(IFormFile file);
        Task DeleteImportedProgramAsync(int programId); // Added this
    }
}