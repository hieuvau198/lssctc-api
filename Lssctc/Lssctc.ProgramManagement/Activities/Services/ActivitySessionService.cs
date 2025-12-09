// File: Lssctc.ProgramManagement/Activities/Services/ActivitySessionService.cs
using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public class ActivitySessionService : IActivitySessionService
    {
        private readonly IUnitOfWork _uow;

        public ActivitySessionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // --- CORE LOGIC: Create Sessions on Class Start (Task 1) ---
        public async Task CreateSessionsOnClassStartAsync(int classId)
        {
            // 1. Get Class Details (Fix: Get the actual CourseId from ProgramCourse navigation)
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => new {
                    CourseId = c.ProgramCourse.CourseId, // <--- CHANGED: Get the real CourseId
                    c.StartDate,
                    c.EndDate
                })
                .FirstOrDefaultAsync();

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            // 2. Get all distinct Activities linked to the Class's Course
            var courseActivities = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .AsNoTracking() // Optimization
                .Where(cs => cs.CourseId == targetClass.CourseId) // <--- CHANGED: Compare with CourseId
                .Include(cs => cs.Section)
                    .ThenInclude(s => s.SectionActivities)
                    .ThenInclude(sa => sa.Activity)
                .SelectMany(cs => cs.Section.SectionActivities)
                .Select(sa => sa.Activity)
                .Where(a => a.IsDeleted == false)
                .Distinct()
                // Project to anonymous type first to ensure distinctness works on ID
                .Select(a => new { a.Id, a.ActivityTitle, a.ActivityType })
                .ToListAsync();

            if (!courseActivities.Any()) return;

            // 3. Get existing sessions for the current class to prevent duplicates
            var existingSessionActivityIds = await _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(s => s.ClassId == classId)
                .Select(s => s.ActivityId)
                .ToListAsync();

            var existingSet = new HashSet<int>(existingSessionActivityIds);
            var newSessions = new List<ActivitySession>();

            // 4. Create new Sessions
            foreach (var activity in courseActivities)
            {
                if (existingSet.Contains(activity.Id)) continue;

                var newSession = new ActivitySession
                {
                    ClassId = classId,
                    ActivityId = activity.Id,
                    IsActive = false,
                    StartTime = null,
                    EndTime = null,
                };

                // Logic: Material (Type 1) is auto-activated with class duration
                if (activity.ActivityType == (int)ActivityType.Material)
                {
                    newSession.IsActive = true;
                    newSession.StartTime = targetClass.StartDate;
                    newSession.EndTime = targetClass.EndDate;
                }

                newSessions.Add(newSession);
            }

            if (newSessions.Any())
            {
                foreach (var session in newSessions)
                {
                    await _uow.ActivitySessionRepository.CreateAsync(session);
                }
                await _uow.SaveChangesAsync();
            }
        }

        // --- CORE LOGIC: Enforcement Access Check (Task 2) ---
        public async Task CheckActivityAccess(int classId, int activityId)
        {
            var session = await GetSessionQuery()
                .FirstOrDefaultAsync(s => s.ClassId == classId && s.ActivityId == activityId);

            if (session == null)
            {
                // Optional: Try to auto-fix if missing (lazy creation)
                // await CreateSessionsOnClassStartAsync(classId);
                // session = await GetSessionQuery().FirstOrDefaultAsync(...);

                if (session == null)
                    throw new InvalidOperationException($"Activity Session not found for Activity ID {activityId} in Class ID {classId}. Access denied. (Configuration Error)");
            }

            // 1. Check IsActive
            if (session.IsActive == false)
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' is currently inactive. Please contact the instructor.");
            }

            // 2. Check Time Window
            var now = DateTime.UtcNow;

            if (session.StartTime.HasValue && now < session.StartTime.Value.ToUniversalTime())
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' will be available from {session.StartTime.Value:yyyy-MM-dd HH:mm:ss}.");
            }

            if (session.EndTime.HasValue && now > session.EndTime.Value.ToUniversalTime())
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' expired on {session.EndTime.Value:yyyy-MM-dd HH:mm:ss}.");
            }
        }

        // --- CRUD API (Task 3 & 4) ---

        public async Task<ActivitySessionDto> CreateActivitySessionAsync(CreateActivitySessionDto dto)
        {
            var existing = await _uow.ActivitySessionRepository
                .ExistsAsync(s => s.ClassId == dto.ClassId && s.ActivityId == dto.ActivityId);

            if (existing)
                throw new InvalidOperationException($"Activity Session already exists for Class ID {dto.ClassId} and Activity ID {dto.ActivityId}. Use Update API.");

            var newSession = new ActivitySession
            {
                ClassId = dto.ClassId,
                ActivityId = dto.ActivityId,
                IsActive = dto.IsActive,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            await _uow.ActivitySessionRepository.CreateAsync(newSession);
            await _uow.SaveChangesAsync();

            var created = await GetSessionQuery().FirstAsync(s => s.Id == newSession.Id);
            return MapToDto(created);
        }

        public async Task<ActivitySessionDto> UpdateActivitySessionAsync(int sessionId, UpdateActivitySessionDto dto)
        {
            var existing = await _uow.ActivitySessionRepository
                                     .GetAllAsQueryable()
                                     .Include(s => s.Activity)
                                     .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (existing == null)
                throw new KeyNotFoundException($"Activity Session with ID {sessionId} not found.");

            existing.IsActive = dto.IsActive;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;

            await _uow.ActivitySessionRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            var updated = await GetSessionQuery().FirstAsync(s => s.Id == existing.Id);
            return MapToDto(updated);
        }

        // --- Retrieval APIs ---
        public async Task<ActivitySessionDto> GetActivitySessionByIdAsync(int sessionId)
        {
            var session = await GetSessionQuery().FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
                throw new KeyNotFoundException($"Activity Session with ID {sessionId} not found.");

            return MapToDto(session);
        }

        public async Task<IEnumerable<ActivitySessionDto>> GetActivitySessionsByClassIdAsync(int classId)
        {
            var sessions = await GetSessionQuery()
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.Activity.ActivityTitle)
                .ToListAsync();

            // Auto-heal: If no sessions exist, try to create them (useful for classes created before this feature)
            if (!sessions.Any())
            {
                await CreateSessionsOnClassStartAsync(classId);
                // Refetch
                sessions = await GetSessionQuery()
                    .Where(s => s.ClassId == classId)
                    .OrderBy(s => s.Activity.ActivityTitle)
                    .ToListAsync();
            }

            return sessions.Select(MapToDto);
        }

        // --- Helpers ---
        private IQueryable<ActivitySession> GetSessionQuery()
        {
            return _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(s => s.Activity);
        }

        private static ActivitySessionDto MapToDto(ActivitySession s)
        {
            return new ActivitySessionDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ActivityId = s.ActivityId,
                ActivityTitle = s.Activity?.ActivityTitle ?? "Unknown Activity",
                ActivityType = s.Activity?.ActivityType ?? 0,
                IsActive = s.IsActive ?? false,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            };
        }
    }
}