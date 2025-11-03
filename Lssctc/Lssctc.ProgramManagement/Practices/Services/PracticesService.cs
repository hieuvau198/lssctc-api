using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class PracticesService : IPracticesService
    {
        private readonly IUnitOfWork _uow;

        public PracticesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Practices CRUD

        public async Task<IEnumerable<PracticeDto>> GetAllPracticesAsync()
        {
            var practices = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .ToListAsync();

            return practices.Select(MapToDto);
        }

        public async Task<PagedResult<PracticeDto>> GetPracticesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.PracticeRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .Select(p => MapToDto(p));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PracticeDto?> GetPracticeByIdAsync(int id)
        {
            var practice = await _uow.PracticeRepository.GetByIdAsync(id);
            if (practice == null || practice.IsDeleted == true)
                return null;

            return MapToDto(practice);
        }

        public async Task<PracticeDto> CreatePracticeAsync(CreatePracticeDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.PracticeName))
                throw new ArgumentException("Practice name is required.");

            var practice = new Practice
            {
                PracticeName = createDto.PracticeName.Trim(),
                PracticeDescription = createDto.PracticeDescription?.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes,
                DifficultyLevel = createDto.DifficultyLevel,
                MaxAttempts = createDto.MaxAttempts,
                CreatedDate = DateTime.UtcNow,
                IsActive = createDto.IsActive ?? true,
                IsDeleted = false
            };

            await _uow.PracticeRepository.CreateAsync(practice);
            await _uow.SaveChangesAsync();

            return MapToDto(practice);
        }

        public async Task<PracticeDto> UpdatePracticeAsync(int id, UpdatePracticeDto updateDto)
        {
            var practice = await _uow.PracticeRepository.GetByIdAsync(id);
            if (practice == null || practice.IsDeleted == true)
                throw new KeyNotFoundException($"Practice with ID {id} not found.");

            practice.PracticeName = updateDto.PracticeName?.Trim() ?? practice.PracticeName;
            practice.PracticeDescription = updateDto.PracticeDescription?.Trim() ?? practice.PracticeDescription;
            practice.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes ?? practice.EstimatedDurationMinutes;
            practice.DifficultyLevel = updateDto.DifficultyLevel ?? practice.DifficultyLevel;
            practice.MaxAttempts = updateDto.MaxAttempts ?? practice.MaxAttempts;
            practice.IsActive = updateDto.IsActive ?? practice.IsActive;

            await _uow.PracticeRepository.UpdateAsync(practice);
            await _uow.SaveChangesAsync();

            return MapToDto(practice);
        }

        public async Task DeletePracticeAsync(int id)
        {
            var practice = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .Include(p => p.ActivityPractices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (practice == null)
                throw new KeyNotFoundException($"Practice with ID {id} not found.");

            if (practice.ActivityPractices.Any())
                throw new InvalidOperationException("Cannot delete a practice linked to activities.");

            await _uow.PracticeRepository.DeleteAsync(practice);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Activity Practices

        public async Task<IEnumerable<PracticeDto>> GetPracticesByActivityAsync(int activityId)
        {
            var practices = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .Include(ap => ap.Practice)
                .Where(ap => ap.ActivityId == activityId && (ap.IsDeleted == null || ap.IsDeleted == false))
                .Select(ap => ap.Practice)
                .Where(p => p.IsDeleted == null || p.IsDeleted == false)
                .ToListAsync();

            return practices.Select(p => MapToDto(p));
        }


        public async Task AddPracticeToActivityAsync(int activityId, int practiceId)
        {
            var activity = await _uow.ActivityRepository
                .GetAllAsQueryable()
                .Include(a => a.ActivityMaterials)
                .Include(a => a.ActivityQuizzes)
                .Include(a => a.ActivityPractices)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            var practice = await _uow.PracticeRepository.GetByIdAsync(practiceId);

            if (activity == null)
                throw new KeyNotFoundException($"Activity with ID {activityId} not found.");

            if (practice == null)
                throw new KeyNotFoundException($"Practice with ID {practiceId} not found.");

            // Rule: One activity can only have one practice
            if (activity.ActivityPractices.Any())
                throw new InvalidOperationException("This activity already has a practice assigned.");

            // Rule: If activity has quiz or material, cannot assign practice
            if (activity.ActivityMaterials.Any())
                throw new InvalidOperationException("This activity already has a material assigned. Cannot add practice.");

            if (activity.ActivityQuizzes.Any())
                throw new InvalidOperationException("This activity already has a quiz assigned. Cannot add practice.");

            // Rule: Prevent duplicate links
            bool exists = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AnyAsync(ap => ap.ActivityId == activityId && ap.PracticeId == practiceId);

            if (exists)
                throw new InvalidOperationException("This practice is already linked to the activity.");

            var link = new ActivityPractice
            {
                ActivityId = activityId,
                PracticeId = practiceId,
                IsActive = true,
                IsDeleted = false
            };

            await _uow.ActivityPracticeRepository.CreateAsync(link);
            await _uow.SaveChangesAsync();
        }

        public async Task RemovePracticeFromActivityAsync(int activityId, int practiceId)
        {
            var link = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ap => ap.ActivityId == activityId && ap.PracticeId == practiceId);

            if (link == null)
                throw new KeyNotFoundException("Practice is not linked to this activity.");

            await _uow.ActivityPracticeRepository.DeleteAsync(link);
            await _uow.SaveChangesAsync();
        }

        #endregion

        #region Mapping Helpers

        private static PracticeDto MapToDto(Practice p)
        {
            return new PracticeDto
            {
                Id = p.Id,
                PracticeName = p.PracticeName,
                PracticeDescription = p.PracticeDescription,
                EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                DifficultyLevel = p.DifficultyLevel,
                MaxAttempts = p.MaxAttempts,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive
            };
        }

        #endregion
    }
}
