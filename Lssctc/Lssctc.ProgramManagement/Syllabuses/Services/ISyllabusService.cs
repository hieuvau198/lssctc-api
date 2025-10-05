using System.Collections.Generic;
using System.Threading.Tasks;
using Lssctc.ProgramManagement.Syllabuses.Dtos;

namespace Lssctc.ProgramManagement.Syllabuses.Services
{
    public interface ISyllabusService
    {
        // Syllabus CRUD
        Task<SyllabusDto?> GetSyllabusByIdAsync(int id);
        Task<List<SyllabusDto>> GetAllSyllabusesAsync();
        Task<SyllabusDto> CreateSyllabusAsync(CreateSyllabusDto dto);
        Task<SyllabusDto?> UpdateSyllabusAsync(int id, UpdateSyllabusDto dto);
        Task<bool> DeleteSyllabusAsync(int id);

        // Syllabus Section CRUD
        Task<SyllabusSectionDto?> GetSyllabusSectionByIdAsync(int id);
        Task<List<SyllabusSectionDto>> GetSyllabusSectionsBySyllabusIdAsync(int syllabusId);
        Task<SyllabusSectionDto> CreateSyllabusSectionAsync(CreateSyllabusSectionDto dto);
        Task<SyllabusSectionDto?> UpdateSyllabusSectionAsync(int id, UpdateSyllabusSectionDto dto);
        Task<bool> DeleteSyllabusSectionAsync(int id);
    }
}