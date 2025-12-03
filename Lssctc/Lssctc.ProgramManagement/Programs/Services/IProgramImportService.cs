using Lssctc.ProgramManagement.Programs.Dtos;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public interface IProgramImportService
    {
        Task<ProgramDto> ImportProgramFromExcelAsync(IFormFile file);
    }
}
