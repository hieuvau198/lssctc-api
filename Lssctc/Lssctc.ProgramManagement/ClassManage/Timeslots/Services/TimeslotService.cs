using Lssctc.ProgramManagement.ClassManage.Timeslots.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Lssctc.ProgramManagement.ClassManage.Timeslots.Services
{
    public class TimeslotService : ITimeslotService
    {
        private readonly IUnitOfWork _uow;

        public TimeslotService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // --- Instructor APIs ---
        // Thêm vào class TimeslotService

        public async Task<TimeslotDto> CreateTimeslotAsync(CreateTimeslotDto dto, int creatorId)
        {
            // 1. Validate Class existence and status
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            // BR: Only allow timeslot creation if class is in 'Draft', 'Open', or 'Inprogress'
            if (targetClass.Status == (int)ClassStatusEnum.Completed || targetClass.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot create timeslots for completed or cancelled classes.");

            // 2. Validate Creator/Instructor Authorization (Allow Admin to bypass this check)
            var creatorUser = await _uow.UserRepository.GetByIdAsync(creatorId);
            if (creatorUser?.Role == (int)UserRoleEnum.Instructor)
            {
                var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                    ci.ClassId == dto.ClassId && ci.InstructorId == creatorId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException($"Instructor {creatorId} is not assigned to class {dto.ClassId}.");
            }

            // 3. Validate Date/Time
            if (dto.EndTime <= dto.StartTime)
                throw new ArgumentException("End time must be after start time.");

            // Optional BR: Check for conflict (omitted for brevity, assume caller handles logic or allows overlap)

            // 4. Create new Timeslot entity
            var newTimeslot = new Timeslot
            {
                ClassId = dto.ClassId,
                Name = dto.Name?.Trim(),
                LocationDetail = dto.LocationDetail?.Trim(),
                LocationBuilding = dto.LocationBuilding?.Trim(),
                LocationRoom = dto.LocationRoom?.Trim(),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime.Value,
                Status = (int)TimeslotStatusEnum.NotStarted,
                IsDeleted = false
            };

            await _uow.TimeslotRepository.CreateAsync(newTimeslot);
            await _uow.SaveChangesAsync();

            // 5. Return DTO by refetching with navigation data for accurate mapping
            var created = await GetTimeslotQuery().FirstAsync(t => t.Id == newTimeslot.Id);
            return MapToDto(created);
        }
        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByClassAndInstructorAsync(int classId, int instructorId)
        {
            var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                ci.ClassId == classId && ci.InstructorId == instructorId);

            if (!isAssigned)
                throw new UnauthorizedAccessException($"Instructor {instructorId} is not assigned to class {classId}.");

            var timeslots = await GetTimeslotQuery()
                .Where(t => t.ClassId == classId)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            return timeslots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByInstructorForWeekAsync(int instructorId, DateTime weekStart)
        {
            var startOfWeek = weekStart.Date;
            var endOfWeek = startOfWeek.AddDays(7);

            var assignedClassIds = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Where(ci => ci.InstructorId == instructorId)
                .Select(ci => ci.ClassId)
                .ToListAsync();

            if (!assignedClassIds.Any())
                return Enumerable.Empty<TimeslotDto>();

            var timeslots = await GetTimeslotQuery()
                .Where(t => assignedClassIds.Contains(t.ClassId) &&
                            t.StartTime >= startOfWeek &&
                            t.StartTime < endOfWeek)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            return timeslots.Select(MapToDto);
        }

        public async Task<TimeslotAttendanceDto> GetAttendanceListForTimeslotAsync(int timeslotId, int instructorId)
        {
            var timeslot = await GetTimeslotQuery().FirstOrDefaultAsync(t => t.Id == timeslotId);
            if (timeslot == null)
                throw new KeyNotFoundException($"Timeslot with ID {timeslotId} not found.");

            // 1. Verify instructor permission
            var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                ci.ClassId == timeslot.ClassId && ci.InstructorId == instructorId);
            if (!isAssigned)
                throw new UnauthorizedAccessException($"Instructor {instructorId} is not assigned to class {timeslot.ClassId}.");

            // 2. Get all enrollments (trainees) in the class that are 'Enrolled' or 'Inprogress'
            var validEnrollmentStatuses = new[] { (int)EnrollmentStatusEnum.Enrolled, (int)EnrollmentStatusEnum.Inprogress };
            var traineesInClass = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(e => e.ClassId == timeslot.ClassId && validEnrollmentStatuses.Contains(e.Status ?? -1))
                .Include(e => e.Trainee).ThenInclude(t => t.IdNavigation)
                .Select(e => new
                {
                    e.Id,
                    e.TraineeId,
                    e.Trainee.TraineeCode,
                    e.Trainee.IdNavigation.Fullname,
                    e.Trainee.IdNavigation.AvatarUrl
                })
                .ToListAsync();

            if (!traineesInClass.Any())
            {
                // Return empty list of trainees
                return new TimeslotAttendanceDto
                {
                    TimeslotId = timeslot.Id,
                    TimeslotName = timeslot.Name,
                    ClassId = timeslot.ClassId,
                    ClassName = timeslot.Class.Name,
                    StartTime = timeslot.StartTime,
                    EndTime = timeslot.EndTime,
                    Trainees = new List<TraineeAttendanceDto>()
                };
            }

            // 3. Get existing attendance records for this timeslot
            var existingAttendance = await _uow.AttendanceRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(a => a.TimeslotId == timeslotId)
                .ToDictionaryAsync(a => a.EnrollmentId);

            // 4. Map to DTO
            var traineeAttendanceList = traineesInClass.Select(t =>
            {
                existingAttendance.TryGetValue(t.Id, out var attendance);
                string status = attendance?.Status.HasValue ?? false
                    ? Enum.GetName(typeof(AttendanceStatusEnum), attendance.Status.Value) ?? "NotStarted"
                    : "NotStarted";

                return new TraineeAttendanceDto
                {
                    EnrollmentId = t.Id,
                    TraineeId = t.TraineeId,
                    TraineeName = t.Fullname ?? t.TraineeCode,
                    TraineeCode = t.TraineeCode,
                    AvatarUrl = t.AvatarUrl,
                    AttendanceStatus = status
                };
            }).ToList();

            return new TimeslotAttendanceDto
            {
                TimeslotId = timeslot.Id,
                TimeslotName = timeslot.Name,
                ClassId = timeslot.ClassId,
                ClassName = timeslot.Class.Name,
                StartTime = timeslot.StartTime,
                EndTime = timeslot.EndTime,
                Trainees = traineeAttendanceList
            };
        }

        public async Task<TimeslotAttendanceDto> SubmitAttendanceForTimeslotAsync(int timeslotId, int instructorId, SubmitAttendanceDto dto)
        {
            var timeslot = await GetTimeslotQuery().FirstOrDefaultAsync(t => t.Id == timeslotId);
            if (timeslot == null)
                throw new KeyNotFoundException($"Timeslot with ID {timeslotId} not found.");

            // 1. Verify instructor permission
            var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                ci.ClassId == timeslot.ClassId && ci.InstructorId == instructorId);
            if (!isAssigned)
                throw new UnauthorizedAccessException($"Instructor {instructorId} is not assigned to class {timeslot.ClassId}.");

            // 2. Validate all submitted enrollment IDs belong to this class
            var submittedEnrollmentIds = dto.AttendanceRecords.Select(r => r.EnrollmentId).ToHashSet();
            var validEnrollmentIds = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == timeslot.ClassId && submittedEnrollmentIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync();

            if (validEnrollmentIds.Count != submittedEnrollmentIds.Count)
                throw new ArgumentException("One or more submitted enrollment IDs are invalid or do not belong to this class.");

            // 3. Process submissions
            var existingAttendance = await _uow.AttendanceRepository
                .GetAllAsQueryable()
                .Where(a => a.TimeslotId == timeslotId && submittedEnrollmentIds.Contains(a.EnrollmentId))
                .ToListAsync();

            var existingMap = existingAttendance.ToDictionary(a => a.EnrollmentId);

            foreach (var record in dto.AttendanceRecords)
            {
                // Check if an Attendance record already exists
                if (existingMap.TryGetValue(record.EnrollmentId, out var attendance))
                {
                    // Update existing record
                    attendance.Status = record.Status;
                    attendance.Description = record.Note;
                    await _uow.AttendanceRepository.UpdateAsync(attendance);
                }
                else
                {
                    // Create new Attendance record
                    var newAttendance = new Attendance
                    {
                        EnrollmentId = record.EnrollmentId,
                        TimeslotId = timeslotId,
                        Status = record.Status,
                        Description = record.Note,
                        // Name is inherited from Enrollment (we could fetch it, but skipping for simplicity here)
                        Name = $"Attendance for Timeslot {timeslotId}",
                        IsActive = true,
                        IsDeleted = false
                    };
                    await _uow.AttendanceRepository.CreateAsync(newAttendance);
                }
            }

            // 4. Update Timeslot Status if necessary (e.g., if it's the first submission, set to Ongoing)
            if (timeslot.Status < (int)TimeslotStatusEnum.Ongoing)
            {
                timeslot.Status = (int)TimeslotStatusEnum.Ongoing;
                await _uow.TimeslotRepository.UpdateAsync(timeslot); // Changed from _uow.ClassRepository.UpdateAsync(timeslot.Class)
            }

            await _uow.SaveChangesAsync();

            // 5. Return the updated list for verification
            return await GetAttendanceListForTimeslotAsync(timeslotId, instructorId);
        }


        // --- Trainee APIs ---

        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByClassAndTraineeAsync(int classId, int traineeId)
        {
            var isEnrolled = await _uow.EnrollmentRepository.ExistsAsync(e =>
                e.ClassId == classId && e.TraineeId == traineeId &&
                (e.Status == (int)EnrollmentStatusEnum.Enrolled || e.Status == (int)EnrollmentStatusEnum.Inprogress));

            if (!isEnrolled)
                throw new UnauthorizedAccessException($"Trainee {traineeId} is not actively enrolled in class {classId}.");

            var timeslots = await GetTimeslotQuery()
                .Where(t => t.ClassId == classId)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            return timeslots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByTraineeForWeekAsync(int traineeId, DateTime weekStart)
        {
            var startOfWeek = weekStart.Date;
            var endOfWeek = startOfWeek.AddDays(7);

            var enrolledClassIds = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled || e.Status == (int)EnrollmentStatusEnum.Inprogress))
                .Select(e => e.ClassId)
                .ToListAsync();

            if (!enrolledClassIds.Any())
                return Enumerable.Empty<TimeslotDto>();

            var timeslots = await GetTimeslotQuery()
                .Where(t => enrolledClassIds.Contains(t.ClassId) &&
                            t.StartTime >= startOfWeek &&
                            t.StartTime < endOfWeek)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            return timeslots.Select(MapToDto);
        }


        // --- Helper Methods ---
        private IQueryable<Timeslot> GetTimeslotQuery()
        {
            return _uow.TimeslotRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.IsDeleted == false)
                .Include(t => t.Class)
                .Include(t => t.Attendances);
        }

        private TimeslotDto MapToDto(Timeslot t)
        {
            // Use local method to parse enum from integer value
            string status = t.Status.HasValue
                ? Enum.GetName(typeof(TimeslotStatusEnum), t.Status.Value) ?? "NotStarted"
                : "NotStarted";

            return new TimeslotDto
            {
                Id = t.Id,
                ClassId = t.ClassId,
                ClassName = t.Class?.Name,
                Name = t.Name,
                LocationDetail = t.LocationDetail,
                LocationBuilding = t.LocationBuilding,
                LocationRoom = t.LocationRoom,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Status = status
            };
        }
    }
}