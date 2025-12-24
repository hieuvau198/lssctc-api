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

        public async Task<PagedResult<MaterialDto>> GetMaterialsAsync(int pageNumber, int pageSize, int? instructorId)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<LearningMaterial> query = _uow.LearningMaterialRepository.GetAllAsQueryable();

            if (instructorId.HasValue && instructorId.Value > 0)
            {
                query = query.Where(m => m.MaterialAuthors.Any(ma => ma.InstructorId == instructorId.Value));
            }

            var materialDtoQuery = query.Select(m => MapToDto(m));
            return await materialDtoQuery.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(int id)
        {
            var material = await _uow.LearningMaterialRepository.GetByIdAsync(id);
            return material == null ? null : MapToDto(material);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(int id, int? instructorId)
        {
            IQueryable<LearningMaterial> query = _uow.LearningMaterialRepository.GetAllAsQueryable();

            if (instructorId.HasValue && instructorId.Value > 0)
            {
                query = query.Where(m => m.Id == id && m.MaterialAuthors.Any(ma => ma.InstructorId == instructorId.Value));
            }
            else
            {
                query = query.Where(m => m.Id == id);
            }

            var material = await query.FirstOrDefaultAsync();
            return material == null ? null : MapToDto(material);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByInstructorIdAsync(int instructorId)
        {
            var materials = await _uow.LearningMaterialRepository
                .GetAllAsQueryable()
                .Where(m => m.MaterialAuthors.Any(ma => ma.InstructorId == instructorId))
                .ToListAsync();

            return materials.Select(MapToDto);
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto)
        {
            return await CreateMaterialAsync(createDto, instructorId: 0);
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto createDto, int instructorId)
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

            if (instructorId > 0)
            {
                var instructor = await _uow.InstructorRepository.GetByIdAsync(instructorId);
                if (instructor == null)
                    throw new ArgumentException($"Instructor with ID {instructorId} not found.");

                var materialAuthor = new MaterialAuthor
                {
                    InstructorId = instructorId,
                    MaterialId = material.Id
                };

                await _uow.MaterialAuthorRepository.CreateAsync(materialAuthor);
                await _uow.SaveChangesAsync();
            }

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

        public async Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialDto updateDto, int? instructorId)
        {
            if (instructorId.HasValue && instructorId.Value > 0)
            {
                var isAuthor = await _uow.MaterialAuthorRepository.ExistsAsync(ma => ma.MaterialId == id && ma.InstructorId == instructorId.Value);
                if (!isAuthor)
                    throw new KeyNotFoundException($"Material with ID {id} not found.");
            }

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
            // 1. Retrieve Material with its Activity links
            var material = await _uow.LearningMaterialRepository
                .GetAllAsQueryable()
                .Include(m => m.ActivityMaterials)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
                throw new KeyNotFoundException($"Material with ID {id} not found.");

            // 2. CHECK USAGE: Activity Records
            // We strictly check if any activity using this material has generated student records.
            if (material.ActivityMaterials.Any())
            {
                // Get the list of Activity IDs that use this material
                var linkedActivityIds = material.ActivityMaterials.Select(am => am.ActivityId).Distinct().ToList();

                // Query the ActivityRecordRepository to see if any of these activities are actually in use
                var isUsedInClasses = await _uow.ActivityRecordRepository
                    .GetAllAsQueryable()
                    .AnyAsync(ar => ar.ActivityId.HasValue && linkedActivityIds.Contains(ar.ActivityId.Value));

                if (isUsedInClasses)
                {
                    throw new InvalidOperationException("Cannot delete material because it is currently used in class activities (Activity Records exist).");
                }

                // 3. CLEANUP: Remove links to Activities
                // If not used in classes, we must manually remove the links in the ActivityMaterial table 
                // before deleting the material to avoid Foreign Key errors.
                var linksToDelete = material.ActivityMaterials.ToList();
                foreach (var link in linksToDelete)
                {
                    await _uow.ActivityMaterialRepository.DeleteAsync(link);
                }
            }

            // 4. CLEANUP: Remove Material Authors
            // Even though we don't check for permissions, we must clean up the author association table.
            var materialAuthors = await _uow.MaterialAuthorRepository
                .GetAllAsQueryable()
                .Where(ma => ma.MaterialId == id)
                .ToListAsync();

            foreach (var materialAuthor in materialAuthors)
            {
                await _uow.MaterialAuthorRepository.DeleteAsync(materialAuthor);
            }

            // 5. DELETE: The Material itself
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

            if (activity.ActivityMaterials.Any())
                throw new InvalidOperationException("This activity already has a material assigned. Only one material is allowed per activity.");

            if (activity.ActivityQuizzes.Any())
                throw new InvalidOperationException("This activity already has a quiz assigned. Cannot add material.");

            if (activity.ActivityPractices.Any())
                throw new InvalidOperationException("This activity already has a practice assigned. Cannot add material.");

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
            var link = await _uow.ActivityMaterialRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(am => am.ActivityId == activityId && am.LearningMaterialId == materialId);

            if (link == null)
                throw new KeyNotFoundException("Material is not linked to the activity.");

            await _uow.ActivityMaterialRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Trainee Materials
        public async Task<TraineeMaterialResponseDto> GetMaterialsForTraineeAsync(int activityRecordId)
        {
            var record = await _uow.ActivityRecordRepository.GetAllAsQueryable()
                .Include(ar => ar.SectionRecord)
                    .ThenInclude(sr => sr.LearningProgress)
                        .ThenInclude(lp => lp.Enrollment)
                .FirstOrDefaultAsync(ar => ar.Id == activityRecordId);

            if (record == null)
                throw new KeyNotFoundException("Activity Record not found.");

            var activityId = record.ActivityId.GetValueOrDefault();
            var classId = record.SectionRecord.LearningProgress.Enrollment.ClassId;
            var session = await _uow.ActivitySessionRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(s => s.ClassId == classId && s.ActivityId == activityId && s.IsActive == true);

            var status = new TraineeSessionStatusDto
            {
                IsOpen = true,
                Message = "Available"
            };

            if (session != null)
            {
                status.StartTime = session.StartTime;
                status.EndTime = session.EndTime;
                var now = DateTime.Now;

                if (status.StartTime.HasValue && now < status.StartTime.Value)
                {
                    status.IsOpen = false;
                    status.Message = "Not started yet";
                }
                else if (status.EndTime.HasValue && now > status.EndTime.Value)
                {
                    status.IsOpen = false;
                    status.Message = "Expired";
                }
            }

            var materials = await GetMaterialsByActivityAsync(activityId);

            return new TraineeMaterialResponseDto
            {
                Materials = materials,
                SessionStatus = status
            };
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
                MaterialUrl = am.LearningMaterial.MaterialUrl
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