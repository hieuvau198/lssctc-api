using Lssctc.ProgramManagement.Sections.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Sections.Services
{
    public interface ISectionsService
    {
        // Get all sections
        Task<IEnumerable<SectionDto>> GetAllSectionsAsync();

        // Get all sections paged
        Task<PagedResult<SectionDto>> GetSectionsAsync(int pageNumber, int pageSize);

        // Get section by id
        Task<SectionDto?> GetSectionByIdAsync(int id);

        // Create section
        Task<SectionDto> CreateSectionAsync(CreateSectionDto createDto);

        // Update section
        Task<SectionDto> UpdateSectionAsync(int id, UpdateSectionDto updateDto);

        // Delete section
        Task DeleteSectionAsync(int id);

        // Get sections by course id
        Task<IEnumerable<SectionDto>> GetSectionsByCourseIdAsync(int courseId);

        // Add section to course
        Task AddSectionToCourseAsync(int courseId, int sectionId);

        // Remove section from course
        Task RemoveSectionFromCourseAsync(int courseId, int sectionId);

        // Update course section order
        Task UpdateCourseSectionOrderAsync(int courseId, int sectionId, int newOrder);

        Task<IEnumerable<SectionDto>> ImportSectionsFromExcelAsync(int courseId, IFormFile file);
    }
}