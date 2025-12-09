using Lssctc.ProgramManagement.Activities.Services; // Added
using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class PracticesService : IPracticesService
    {
        private readonly IUnitOfWork _uow;
        private readonly IActivitySessionService _sessionService; // Added

        public PracticesService(IUnitOfWork uow, IActivitySessionService sessionService) // Updated Constructor
        {
            _uow = uow;
            _sessionService = sessionService;
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

            // Validate PracticeCode uniqueness if provided
            if (!string.IsNullOrWhiteSpace(createDto.PracticeCode))
            {
                var normalizedCode = createDto.PracticeCode.Trim();
                bool codeExists = await _uow.PracticeRepository
                    .ExistsAsync(p => p.PracticeCode != null &&
                                     p.PracticeCode.ToLower() == normalizedCode.ToLower() &&
                                     (p.IsDeleted == null || p.IsDeleted == false));

                if (codeExists)
                    throw new ArgumentException($"Practice code '{normalizedCode}' already exists.");
            }

            var practice = new Practice
            {
                PracticeName = createDto.PracticeName.Trim(),
                PracticeDescription = createDto.PracticeDescription?.Trim(),
                EstimatedDurationMinutes = createDto.EstimatedDurationMinutes,
                DifficultyLevel = createDto.DifficultyLevel,
                MaxAttempts = createDto.MaxAttempts,
                PracticeCode = createDto.PracticeCode?.Trim(),
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

            // Validate PracticeCode uniqueness if provided and different from current
            if (!string.IsNullOrWhiteSpace(updateDto.PracticeCode))
            {
                var normalizedCode = updateDto.PracticeCode.Trim();

                // Only check if the code is different from the current one
                if (practice.PracticeCode?.ToLower() != normalizedCode.ToLower())
                {
                    bool codeExists = await _uow.PracticeRepository
                        .ExistsAsync(p => p.Id != id &&
                                         p.PracticeCode != null &&
                                         p.PracticeCode.ToLower() == normalizedCode.ToLower() &&
                                         (p.IsDeleted == null || p.IsDeleted == false));

                    if (codeExists)
                        throw new ArgumentException($"Practice code '{normalizedCode}' already exists.");
                }
            }

            practice.PracticeName = updateDto.PracticeName?.Trim() ?? practice.PracticeName;
            practice.PracticeDescription = updateDto.PracticeDescription?.Trim() ?? practice.PracticeDescription;
            practice.EstimatedDurationMinutes = updateDto.EstimatedDurationMinutes ?? practice.EstimatedDurationMinutes;
            practice.DifficultyLevel = updateDto.DifficultyLevel ?? practice.DifficultyLevel;
            practice.MaxAttempts = updateDto.MaxAttempts ?? practice.MaxAttempts;
            practice.IsActive = updateDto.IsActive ?? practice.IsActive;

            // Update PracticeCode if provided
            if (!string.IsNullOrWhiteSpace(updateDto.PracticeCode))
                practice.PracticeCode = updateDto.PracticeCode.Trim();

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

            if (practice.ActivityPractices != null && practice.ActivityPractices.Any())
                throw new InvalidOperationException("Cannot delete a practice linked to activities.");

            // Soft delete only
            practice.IsDeleted = true;

            await _uow.PracticeRepository.UpdateAsync(practice);
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

        #region Trainee Practices
        public async Task<IEnumerable<TraineePracticeDto>> GetPracticesForTraineeAsync(int traineeId, int classId)
        {
            // 1. Get the trainee's learning progress ID for the specific class.
            var learningProgress = await _uow.LearningProgressRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(lp => lp.Enrollment)
                .Where(lp => lp.Enrollment.TraineeId == traineeId && lp.Enrollment.ClassId == classId)
                .Select(lp => new { lp.Id })
                .FirstOrDefaultAsync();

            if (learningProgress == null)
            {
                return new List<TraineePracticeDto>();
            }

            // 2. Get all 'Practice' ActivityRecords for this trainee's progress.
            var practiceActivityRecords = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ar => ar.SectionRecord.LearningProgressId == learningProgress.Id &&
                             ar.ActivityType == (int)ActivityType.Practice)
                .Select(ar => new
                {
                    ar.Id,
                    ar.ActivityId,
                    ar.IsCompleted
                })
                .ToListAsync();

            if (!practiceActivityRecords.Any())
            {
                return new List<TraineePracticeDto>();
            }

            // 3. Get the mapping from ActivityId -> Practice (and include task templates)
            var activityIds = practiceActivityRecords.Select(ar => ar.ActivityId).Distinct();
            var activityPracticeMap = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ap => ap.Practice) // Get the Practice details
                    .ThenInclude(p => p.PracticeTasks) // ...and its task links
                        .ThenInclude(pt => pt.Task) // ...and the task details (template)
                .Where(ap => activityIds.Contains(ap.ActivityId) &&
                             ap.Practice.IsDeleted != true)
                .ToDictionaryAsync(ap => ap.ActivityId, ap => ap.Practice); // Map ActivityId -> Practice

            // 4. Get all *current* practice attempts for these activity records in one query
            var activityRecordIds = practiceActivityRecords.Select(ar => ar.Id).ToList();
            var currentAttemptsMap = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks) // Include the attempt tasks
                .Where(pa => activityRecordIds.Contains(pa.ActivityRecordId) && pa.IsCurrent)
                .ToDictionaryAsync(pa => pa.ActivityRecordId); // Map ActivityRecordId -> PracticeAttempt

            // 5. Combine the lists
            var results = new List<TraineePracticeDto>();
            foreach (var ar in practiceActivityRecords)
            {
                if (ar.ActivityId.HasValue && activityPracticeMap.TryGetValue(ar.ActivityId.Value, out var practice))
                {
                    var traineePracticeDto = new TraineePracticeDto
                    {
                        // Trainee Status
                        ActivityRecordId = ar.Id,
                        ActivityId = ar.ActivityId.Value,
                        IsCompleted = ar.IsCompleted ?? false,

                        // Practice Details
                        Id = practice.Id,
                        PracticeName = practice.PracticeName,
                        PracticeCode = practice.PracticeCode, // Added
                        PracticeDescription = practice.PracticeDescription,
                        EstimatedDurationMinutes = practice.EstimatedDurationMinutes,
                        DifficultyLevel = practice.DifficultyLevel,
                        MaxAttempts = practice.MaxAttempts,
                        CreatedDate = practice.CreatedDate,
                        IsActive = practice.IsActive,
                        Tasks = new List<TraineeTaskDto>()
                    };

                    // Try to find the current attempt
                    currentAttemptsMap.TryGetValue(ar.Id, out var currentAttempt);

                    // Get the task templates (PracticeTask links to SimTask)
                    var taskTemplates = practice.PracticeTasks
                        .Where(pt => pt.Task != null && pt.Task.IsDeleted != true)
                        .Select(pt => pt.Task);

                    if (currentAttempt != null)
                    {
                        // --- CASE 1: Trainee has an attempt ---
                        var attemptTasksMap = currentAttempt.PracticeAttemptTasks
                            .Where(pat => pat.TaskId.HasValue)
                            .ToDictionary(pat => pat.TaskId!.Value);

                        foreach (var template in taskTemplates)
                        {
                            attemptTasksMap.TryGetValue(template.Id, out var attemptTask);

                            traineePracticeDto.Tasks.Add(new TraineeTaskDto
                            {
                                TaskId = template.Id,
                                TaskName = template.TaskName,
                                TaskCode = template.TaskCode, // Added
                                TaskDescription = template.TaskDescription,
                                ExpectedResult = template.ExpectedResult,

                                // Data from the attempt (if it exists)
                                PracticeAttemptTaskId = attemptTask?.Id ?? 0,
                                IsPass = attemptTask?.IsPass ?? false,
                                Score = attemptTask?.Score,
                                Description = attemptTask?.Description
                            });
                        }
                    }
                    else
                    {
                        // --- CASE 2: No attempt yet, build from template ---
                        foreach (var template in taskTemplates)
                        {
                            traineePracticeDto.Tasks.Add(new TraineeTaskDto
                            {
                                TaskId = template.Id,
                                TaskName = template.TaskName,
                                TaskCode = template.TaskCode, // Added
                                TaskDescription = template.TaskDescription,
                                ExpectedResult = template.ExpectedResult,
                                PracticeAttemptTaskId = 0,
                                IsPass = false,
                                Score = null,
                                Description = null
                            });
                        }
                    }

                    results.Add(traineePracticeDto);
                }
            }

            return results;
        }

        public async Task<TraineePracticeDto?> GetPracticeForTraineeByActivityIdAsync(int traineeId, int activityRecordId)
        {
            // 1. Get the ActivityRecord and verify ownership and type
            var activityRecord = await _uow.ActivityRecordRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ar => ar.SectionRecord.LearningProgress.Enrollment)
                .Where(ar => ar.Id == activityRecordId &&
                             ar.SectionRecord.LearningProgress.Enrollment.TraineeId == traineeId &&
                             ar.ActivityType == (int)ActivityType.Practice)
                .Select(ar => new
                {
                    ar.Id,
                    ar.ActivityId,
                    ar.IsCompleted,
                    ClassId = ar.SectionRecord.LearningProgress.Enrollment.ClassId // Get ClassId
                })
                .FirstOrDefaultAsync();

            if (activityRecord == null)
            {
                throw new KeyNotFoundException("Practice activity record not found for this trainee.");
            }

            // --- FIX: CHECK ACCESS TIME ---
            if (activityRecord.ActivityId.HasValue)
            {
                await _sessionService.CheckActivityAccess(activityRecord.ClassId, activityRecord.ActivityId.Value);
            }
            // ------------------------------

            // 2. Get the associated Practice and its task templates
            if (!activityRecord.ActivityId.HasValue)
            {
                throw new KeyNotFoundException("Activity record is not linked to a practice template.");
            }

            var practice = await _uow.ActivityPracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ap => ap.Practice)
                    .ThenInclude(p => p.PracticeTasks)
                        .ThenInclude(pt => pt.Task)
                .Where(ap => ap.ActivityId == activityRecord.ActivityId.Value && ap.Practice.IsDeleted != true)
                .Select(ap => ap.Practice)
                .FirstOrDefaultAsync();

            if (practice == null)
            {
                throw new KeyNotFoundException("Practice template not found for this activity.");
            }

            // 3. Get the *current* practice attempt for this activity record
            var currentAttempt = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(pa => pa.PracticeAttemptTasks)
                .Where(pa => pa.ActivityRecordId == activityRecord.Id && pa.IsCurrent)
                .FirstOrDefaultAsync();

            // 4. Build the DTO
            var traineePracticeDto = new TraineePracticeDto
            {
                ActivityRecordId = activityRecord.Id,
                ActivityId = activityRecord.ActivityId.Value,
                IsCompleted = activityRecord.IsCompleted ?? false,
                Id = practice.Id,
                PracticeName = practice.PracticeName,
                PracticeCode = practice.PracticeCode, // Added
                PracticeDescription = practice.PracticeDescription,
                EstimatedDurationMinutes = practice.EstimatedDurationMinutes,
                DifficultyLevel = practice.DifficultyLevel,
                MaxAttempts = practice.MaxAttempts,
                CreatedDate = practice.CreatedDate,
                IsActive = practice.IsActive,
                Tasks = new List<TraineeTaskDto>()
            };

            // 5. Populate the Tasks list
            var taskTemplates = practice.PracticeTasks
                .Where(pt => pt.Task != null && pt.Task.IsDeleted != true)
                .Select(pt => pt.Task);

            if (currentAttempt != null)
            {
                var attemptTasksMap = currentAttempt.PracticeAttemptTasks
                    .Where(pat => pat.TaskId.HasValue)
                    .ToDictionary(pat => pat.TaskId!.Value);

                foreach (var template in taskTemplates)
                {
                    attemptTasksMap.TryGetValue(template.Id, out var attemptTask);

                    traineePracticeDto.Tasks.Add(new TraineeTaskDto
                    {
                        TaskId = template.Id,
                        TaskName = template.TaskName,
                        TaskCode = template.TaskCode, // Added
                        TaskDescription = template.TaskDescription,
                        ExpectedResult = template.ExpectedResult,
                        PracticeAttemptTaskId = attemptTask?.Id ?? 0,
                        IsPass = attemptTask?.IsPass ?? false,
                        Score = attemptTask?.Score,
                        Description = attemptTask?.Description
                    });
                }
            }
            else
            {
                foreach (var template in taskTemplates)
                {
                    traineePracticeDto.Tasks.Add(new TraineeTaskDto
                    {
                        TaskId = template.Id,
                        TaskName = template.TaskName,
                        TaskCode = template.TaskCode, // Added
                        TaskDescription = template.TaskDescription,
                        ExpectedResult = template.ExpectedResult,
                        PracticeAttemptTaskId = 0,
                        IsPass = false,
                        Score = null,
                        Description = null
                    });
                }
            }

            return traineePracticeDto;
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
                IsActive = p.IsActive,
                PracticeCode = p.PracticeCode
            };
        }

        #endregion
    }
}