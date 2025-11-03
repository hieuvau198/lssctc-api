using Lssctc.ProgramManagement.Sections.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Sections.Services
{
    public class SectionsService : ISectionsService
    {
        private readonly IUnitOfWork _uow;
        public SectionsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Sections
        public async Task<IEnumerable<SectionDto>> GetAllSectionsAsync()
        {
            var sections = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.IsDeleted != true)
                .ToListAsync();

            return sections.Select(MapToDto);
        }

        public async Task<PagedResult<SectionDto>> GetSectionsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.IsDeleted != true)
                .Select(s => MapToDto(s));

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);
            return pagedResult;
        }

        public async Task<SectionDto?> GetSectionByIdAsync(int id)
        {
            var section = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.Id == id && s.IsDeleted != true)
                .FirstOrDefaultAsync();

            return section == null ? null : MapToDto(section);
        }

        public async Task<SectionDto> CreateSectionAsync(CreateSectionDto createDto)
        {
            var section = new Section
            {
                SectionTitle = createDto.SectionTitle!.Trim(),
                SectionDescription = string.IsNullOrWhiteSpace(createDto.SectionDescription) ? null : createDto.SectionDescription.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes!.Value,
                IsDeleted = false
            };

            await _uow.SectionRepository.CreateAsync(section);
            await _uow.SaveChangesAsync();

            return MapToDto(section);
        }


        public async Task<SectionDto> UpdateSectionAsync(int id, UpdateSectionDto updateDto)
        {
            var section = await _uow.SectionRepository.GetByIdAsync(id);
            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {id} not found.");
            }

            section.SectionTitle = updateDto.SectionTitle!.Trim();
            section.SectionDescription = string.IsNullOrWhiteSpace(updateDto.SectionDescription) ? null : updateDto.SectionDescription.Trim();
            section.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes!.Value;

            await _uow.SectionRepository.UpdateAsync(section);
            await _uow.SaveChangesAsync();

            return MapToDto(section);
        }


        public async Task DeleteSectionAsync(int id)
        {
            var section = await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.Id == id)
                .Include(s => s.CourseSections)
                .FirstOrDefaultAsync();

            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {id} not found.");
            }

            // Check if section is associated with any courses
            if (section.CourseSections != null && section.CourseSections.Any())
            {
                throw new InvalidOperationException("Cannot delete section associated with courses.");
            }

            section.IsDeleted = true;
            await _uow.SectionRepository.UpdateAsync(section);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Course Sections

        public async Task<IEnumerable<SectionDto>> GetSectionsByCourseIdAsync(int courseId)
        {
            var courseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .Include(cs => cs.Section)
                .ToListAsync();

            var sections = courseSections
                .Where(cs => cs.Section != null && cs.Section.IsDeleted != true)
                .Select(cs => MapToDto(cs.Section!));

            return sections;
        }

        public async Task AddSectionToCourseAsync(int courseId, int sectionId)
        {
            // Verify course exists
            var course = await _uow.CourseRepository.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            // Verify section exists
            var section = await _uow.SectionRepository.GetByIdAsync(sectionId);
            if (section == null || section.IsDeleted == true)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found.");
            }

            // Check if the section is already added to the course
            var existingCourseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (existingCourseSection != null)
            {
                throw new InvalidOperationException($"Section with ID {sectionId} is already added to course {courseId}.");
            }

            // Get the max order for this course
            var maxOrder = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .MaxAsync(cs => (int?)cs.SectionOrder) ?? 0;

            var courseSection = new CourseSection
            {
                CourseId = courseId,
                SectionId = sectionId,
                SectionOrder = maxOrder + 1
            };

            await _uow.CourseSectionRepository.CreateAsync(courseSection);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveSectionFromCourseAsync(int courseId, int sectionId)
        {
            var courseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (courseSection == null)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found in course {courseId}.");
            }

            await _uow.CourseSectionRepository.DeleteAsync(courseSection);
            await _uow.SaveChangesAsync();

            // Reorder remaining sections
            await ReorderCourseSectionsAsync(courseId);
        }

        public async Task UpdateCourseSectionOrderAsync(int courseId, int sectionId, int newOrder)
        {
            if (newOrder < 1)
            {
                throw new ArgumentException("Section order must be greater than 0.", nameof(newOrder));
            }

            var courseSection = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId && cs.SectionId == sectionId)
                .FirstOrDefaultAsync();

            if (courseSection == null)
            {
                throw new KeyNotFoundException($"Section with ID {sectionId} not found in course {courseId}.");
            }

            var allCourseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .ToListAsync();

            var oldOrder = courseSection.SectionOrder;

            if (oldOrder == newOrder)
            {
                return; // No change needed
            }

            // Remove from current position
            allCourseSections.Remove(courseSection);

            // Insert at new position (newOrder - 1 because list is 0-indexed)
            var insertIndex = Math.Min(newOrder - 1, allCourseSections.Count);
            allCourseSections.Insert(insertIndex, courseSection);

            // Update all orders
            for (int i = 0; i < allCourseSections.Count; i++)
            {
                allCourseSections[i].SectionOrder = i + 1;
                await _uow.CourseSectionRepository.UpdateAsync(allCourseSections[i]);
            }

            await _uow.SaveChangesAsync();
        }

        private async Task ReorderCourseSectionsAsync(int courseId)
        {
            var courseSections = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .OrderBy(cs => cs.SectionOrder)
                .ToListAsync();

            for (int i = 0; i < courseSections.Count; i++)
            {
                courseSections[i].SectionOrder = i + 1;
                await _uow.CourseSectionRepository.UpdateAsync(courseSections[i]);
            }

            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Mapping
        private static SectionDto MapToDto(Section s)
        {
            return new SectionDto
            {
                Id = s.Id,
                SectionTitle = s.SectionTitle,
                SectionDescription = s.SectionDescription,
                EstimatedDurationMinutes = s.EstimatedDurationMinutes
            };
        }
        #endregion
    }
}