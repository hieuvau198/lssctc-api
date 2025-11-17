using Lssctc.ProgramManagement.Materials.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Materials.Services
{
    public class MaterialsService : IMaterialsService
    {
        private readonly IUnitOfWork _uow;

        public MaterialsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Learning Materials

        public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync()
        {
            var materials = await _uow.LearningMaterialRepository
                .GetAllAsQueryable()
                .ToListAsync();

            return materials.Select(MapToDto);
        }

        public async Task<PagedResult<MaterialDto>> GetMaterialsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.LearningMaterialRepository
                .GetAllAsQueryable()
                .Select(m => MapToDto(m));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(int id)
        {
            var material = await _uow.LearningMaterialRepository
                .GetByIdAsync(id);

            return material == null ? null : MapToDto(material);
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("Name is required.");

            var material = new LearningMaterial
            {
                Name = createDto.Name.Trim(),
                Description = createDto.Description?.Trim() ?? string.Empty,
                MaterialUrl = createDto.MaterialUrl?.Trim() ?? string.Empty,
                LearningMaterialType = ParseLearningMaterialType(createDto.LearningMaterialType)
            };

            await _uow.LearningMaterialRepository.CreateAsync(material);
            await _uow.SaveChangesAsync();

            return MapToDto(material);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialDto updateDto)
        {
            var material = await _uow.LearningMaterialRepository.GetByIdAsync(id);
            if (material == null)
                throw new KeyNotFoundException($"Material with ID {id} not found.");

            material.Name = updateDto.Name?.Trim() ?? material.Name;
            material.Description = updateDto.Description?.Trim() ?? material.Description;
            material.MaterialUrl = updateDto.MaterialUrl?.Trim() ?? material.MaterialUrl;
            material.LearningMaterialType = ParseLearningMaterialType(updateDto.LearningMaterialType);

            await _uow.LearningMaterialRepository.UpdateAsync(material);
            await _uow.SaveChangesAsync();

            return MapToDto(material);
        }

        public async Task DeleteMaterialAsync(int id)
        {
            var material = await _uow.LearningMaterialRepository
                .GetAllAsQueryable()
                .Include(m => m.ActivityMaterials)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
                throw new KeyNotFoundException($"Material with ID {id} not found.");

            if (material.ActivityMaterials.Any())
            {
                // --- ADDED LOGIC ---
                // Check if any linked activities are part of a locked course
                var linkedActivityIds = material.ActivityMaterials.Select(am => am.ActivityId).ToList();
                foreach (var activityId in linkedActivityIds)
                {
                    if (await IsActivityLockedAsync(activityId))
                    {
                        throw new InvalidOperationException("Cannot delete material. It is assigned to an activity that is part of a course already in use.");
                    }
                }
                // --- END ADDED LOGIC ---

                // If not locked, but still linked, throw original error
                throw new InvalidOperationException("Cannot delete material linked to activities. Please remove it from all activities first.");
            }

            await _uow.LearningMaterialRepository.DeleteAsync(material);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Activity Materials

        public async Task<IEnumerable<ActivityMaterialDto>> GetMaterialsByActivityAsync(int activityId)
        {
            var materials = await _uow.ActivityMaterialRepository
                .GetAllAsQueryable()
                .Where(am => am.ActivityId == activityId)
                .Include(am => am.LearningMaterial)
                .ToListAsync();

            return materials.Select(MapToActivityMaterialDto);
        }

        public async Task AddMaterialToActivityAsync(int activityId, int materialId)
        {
            // --- ADDED LOGIC (BR 1) ---
            if (await IsActivityLockedAsync(activityId))
            {
                throw new InvalidOperationException("Cannot add material. This activity is part of a course that is already in use.");
            }
            // --- END ADDED LOGIC ---

            // Validate both entities
            var activity = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Include(a => a.ActivityMaterials)
                .Include(a => a.ActivityQuizzes)
                .Include(a => a.ActivityPractices)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            var material = await _uow.LearningMaterialRepository.GetByIdAsync(materialId);

            if (activity == null)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

            if (material == null)
                throw new KeyNotFoundException($"Material with ID {materialId} not found.");

            // Only one material allowed per activity
            if (activity.ActivityMaterials.Any())
                throw new InvalidOperationException("This activity already has a material assigned. Only one material is allowed per activity.");

            // If activity has quiz or practice, cannot add material
            if (activity.ActivityQuizzes.Any())
                throw new InvalidOperationException("This activity already has a quiz assigned. Cannot add material.");

            if (activity.ActivityPractices.Any())
                throw new InvalidOperationException("This activity already has a practice assigned. Cannot add material.");

            // Check if this specific material is already linked (redundant but safe)
            bool exists = await _uow.ActivityMaterialRepository
                .GetAllAsQueryable()
                .AnyAsync(am => am.ActivityId == activityId && am.LearningMaterialId == materialId);

            if (exists)
                throw new InvalidOperationException("This material is already assigned to the activity.");

            var newLink = new ActivityMaterial
            {
                ActivityId = activityId,
                LearningMaterialId = materialId,
                Name = material.Name,
                Description = material.Description
            };

            await _uow.ActivityMaterialRepository.CreateAsync(newLink);
            await _uow.SaveChangesAsync();
        }


        public async Task RemoveMaterialFromActivityAsync(int activityId, int materialId)
        {
            // --- ADDED LOGIC (BR 1) ---
            if (await IsActivityLockedAsync(activityId))
            {
                throw new InvalidOperationException("Cannot remove material. This activity is part of a course that is already in use.");
            }
            // --- END ADDED LOGIC ---

            var link = await _uow.ActivityMaterialRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(am => am.ActivityId == activityId && am.LearningMaterialId == materialId);

            if (link == null)
                throw new KeyNotFoundException("Material is not linked to the activity.");

            await _uow.ActivityMaterialRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region --- ADDED HELPER METHODS ---

        /// <summary>
        /// Checks if a course is "locked" (i.e., tied to a class that is InProgress, Completed, or Cancelled).
        /// </summary>
        private async Task<bool> IsCourseLockedAsync(int courseId)
        {
            var lockedStatuses = new[] {
                (int)ClassStatusEnum.Inprogress,
                (int)ClassStatusEnum.Completed,
                (int)ClassStatusEnum.Cancelled
            };

            bool isLocked = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AnyAsync(c => c.ProgramCourse.CourseId == courseId &&
                               c.Status.HasValue &&
                               lockedStatuses.Contains(c.Status.Value));
            return isLocked;
        }

        /// <summary>
        /// Checks if a specific section is "locked" by being part of any locked course.
        /// </summary>
        private async Task<bool> IsSectionLockedAsync(int sectionId)
        {
            var courseIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.SectionId == sectionId)
                .Select(cs => cs.CourseId)
                .Distinct()
                .ToListAsync();

            if (!courseIds.Any()) return false;

            foreach (var courseId in courseIds)
            {
                if (await IsCourseLockedAsync(courseId)) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a specific activity is "locked" by being part of any locked section.
        /// </summary>
        private async Task<bool> IsActivityLockedAsync(int activityId)
        {
            var sectionIds = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.ActivityId == activityId)
                .Select(sa => sa.SectionId)
                .Distinct()
                .ToListAsync();

            if (!sectionIds.Any()) return false;

            foreach (var sectionId in sectionIds)
            {
                if (await IsSectionLockedAsync(sectionId)) return true;
            }
            return false;
        }

        #endregion

        #region Mapping Helpers

        private static MaterialDto MapToDto(LearningMaterial m)
        {
            return new MaterialDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                MaterialUrl = m.MaterialUrl,
                LearningMaterialType = m.LearningMaterialType.HasValue
                    ? Enum.GetName(typeof(LearningMaterialTypeEnum), m.LearningMaterialType.Value)
                    : null
            };
        }

        private static ActivityMaterialDto MapToActivityMaterialDto(ActivityMaterial am)
        {
            return new ActivityMaterialDto
            {
                Id = am.Id,
                ActivityId = am.ActivityId,
                LearningMaterialId = am.LearningMaterialId,
                Name = am.Name,
                Description = am.Description,
                LearningMaterialType = am.LearningMaterial.LearningMaterialType.HasValue
                    ? Enum.GetName(typeof(LearningMaterialTypeEnum), am.LearningMaterial.LearningMaterialType.Value)
                    : null,
                MaterialUrl = am.LearningMaterial.MaterialUrl // <-- ADDED
            };
        }

        private static int? ParseLearningMaterialType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;

            if (Enum.TryParse(typeof(LearningMaterialTypeEnum), type, true, out var parsed))
            {
                return (int)(LearningMaterialTypeEnum)parsed!;
            }

            throw new ArgumentException($"Invalid LearningMaterialType value: {type}");
        }

        #endregion
    }
}