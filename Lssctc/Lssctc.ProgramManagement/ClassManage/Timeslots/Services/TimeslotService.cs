using ExcelDataReader;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
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
        public async Task CreateAttendanceForClassAsync(int classId)
        {
            // 1. Get all Timeslots for the class
            var timeslots = await _uow.TimeslotRepository
                .GetAllAsQueryable()
                .Where(t => t.ClassId == classId && t.IsDeleted == false)
                .Select(t => t.Id)
                .ToListAsync();

            if (!timeslots.Any()) return;

            // 2. Get all valid Enrollments (Enrolled or Inprogress)
            var validStatuses = new[] { (int)EnrollmentStatusEnum.Enrolled, (int)EnrollmentStatusEnum.Inprogress };
            var enrollments = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == classId && validStatuses.Contains(e.Status ?? 0) && e.IsDeleted == false)
                .Select(e => e.Id)
                .ToListAsync();

            if (!enrollments.Any()) return;

            // 3. Find existing attendance records to prevent duplicates
            var existingAttendances = await _uow.AttendanceRepository
                .GetAllAsQueryable()
                .Where(a => timeslots.Contains(a.TimeslotId) && enrollments.Contains(a.EnrollmentId))
                .AsNoTracking()
                .ToListAsync();

            var existingMap = existingAttendances.ToDictionary(a => (a.TimeslotId, a.EnrollmentId));

            // 4. Create new Attendance records if they don't exist
            foreach (var timeslotId in timeslots)
            {
                foreach (var enrollmentId in enrollments)
                {
                    if (!existingMap.ContainsKey((timeslotId, enrollmentId)))
                    {
                        var newAttendance = new Attendance
                        {
                            EnrollmentId = enrollmentId,
                            TimeslotId = timeslotId,
                            Name = $"Attendance for Timeslot {timeslotId}",
                            Status = (int)AttendanceStatusEnum.NotStarted, // Default status
                            IsActive = true,
                            IsDeleted = false
                        };
                        await _uow.AttendanceRepository.CreateAsync(newAttendance);
                    }
                }
            }
            await _uow.SaveChangesAsync();
        }
        public async Task<TimeslotDto> CreateTimeslotAsync(CreateTimeslotDto dto, int creatorId)
        {
            // 1. Validate Timeslot Duration
            ValidateTimeslotDuration(dto.StartTime, dto.EndTime.Value);

            // 2. Validate Class existence and status
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            if (targetClass.Status == (int)ClassStatusEnum.Completed || targetClass.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot create timeslots for completed or cancelled classes.");

            // 3. Validate Creator/Instructor Authorization
            var creatorUser = await _uow.UserRepository.GetByIdAsync(creatorId);
            if (creatorUser?.Role == (int)UserRoleEnum.Instructor)
            {
                var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                    ci.ClassId == dto.ClassId && ci.InstructorId == creatorId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException($"Instructor {creatorId} is not assigned to class {dto.ClassId}.");
            }

            // --- NEW LOGIC: Check Instructor Conflict ---
            // Check if ANY instructor is assigned to this class
            var currentInstructorAssignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ci => ci.ClassId == dto.ClassId);

            if (currentInstructorAssignment != null)
            {
                var instructorId = currentInstructorAssignment.InstructorId;

                // Check against timeslots in OTHER classes assigned to this instructor
                var hasConflict = await _uow.TimeslotRepository
                    .GetAllAsQueryable()
                    .Include(t => t.Class)
                    .ThenInclude(c => c.ClassInstructors)
                    .AnyAsync(t =>
                        t.IsDeleted == false &&
                        // Timeslot belongs to a DIFFERENT class
                        t.ClassId != dto.ClassId &&
                        // That different class is assigned to the SAME instructor
                        t.Class.ClassInstructors.Any(ci => ci.InstructorId == instructorId) &&
                        // And the time overlaps
                        t.StartTime < dto.EndTime.Value && t.EndTime > dto.StartTime
                    );

                if (hasConflict)
                {
                    throw new InvalidOperationException("The assigned instructor has a schedule conflict with this new timeslot in another class.");
                }
            }
            // --------------------------------------------

            var newTimeslot = new Timeslot
            {
                ClassId = dto.ClassId,
                Name = dto.Name!.Trim(),
                LocationDetail = dto.LocationDetail!.Trim(),
                LocationBuilding = dto.LocationBuilding!.Trim(),
                LocationRoom = dto.LocationRoom!.Trim(),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime.Value,
                Status = (int)TimeslotStatusEnum.NotStarted,
                IsDeleted = false
            };

            await _uow.TimeslotRepository.CreateAsync(newTimeslot);
            await _uow.SaveChangesAsync();

            var created = await GetTimeslotQuery().FirstAsync(t => t.Id == newTimeslot.Id);
            return MapToDto(created);
        }

        public async Task<IEnumerable<TimeslotDto>> CreateListTimeslotAsync(CreateListTimeslotDto dto, int creatorId)
        {
            var createdList = new List<TimeslotDto>();
            foreach (var createDto in dto.Timeslots)
            {
                // Re-use single creation logic to ensure all validations are run
                var createdTimeslot = await CreateTimeslotAsync(createDto, creatorId);
                createdList.Add(createdTimeslot);
            }
            return createdList;
        }
        public async Task<TimeslotDto> UpdateTimeslotAsync(int timeslotId, UpdateTimeslotDto dto, int updaterId)
        {
            // 1. Validate Timeslot Duration (Issue 4)
            ValidateTimeslotDuration(dto.StartTime, dto.EndTime.Value);

            var existing = await _uow.TimeslotRepository.GetByIdAsync(timeslotId);
            if (existing == null || existing.IsDeleted == true)
                throw new KeyNotFoundException($"Timeslot with ID {timeslotId} not found.");

            // 2. Validate Updater Authorization (Allow Admin to bypass this check)
            var updaterUser = await _uow.UserRepository.GetByIdAsync(updaterId);
            if (updaterUser?.Role == (int)UserRoleEnum.Instructor)
            {
                var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                    ci.ClassId == existing.ClassId && ci.InstructorId == updaterId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException($"Instructor {updaterId} is not authorized to update timeslot in class {existing.ClassId}.");
            }

            var currentInstructorAssignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ci => ci.ClassId == existing.ClassId);

            if (currentInstructorAssignment != null)
            {
                var instructorId = currentInstructorAssignment.InstructorId;

                // Check against timeslots in OTHER classes
                var hasConflict = await _uow.TimeslotRepository
                    .GetAllAsQueryable()
                    .Include(t => t.Class)
                    .ThenInclude(c => c.ClassInstructors)
                    .AnyAsync(t =>
                        t.IsDeleted == false &&
                        t.ClassId != existing.ClassId && // Ensure we look at other classes
                        t.Class.ClassInstructors.Any(ci => ci.InstructorId == instructorId) &&
                        t.StartTime < dto.EndTime.Value && t.EndTime > dto.StartTime
                    );

                if (hasConflict)
                {
                    throw new InvalidOperationException("The assigned instructor has a schedule conflict with this updated time in another class.");
                }
            }

            // 3. Apply updates (all fields from DTO are required and validated at DTO level)
            existing.Name = dto.Name!.Trim();
            existing.LocationDetail = dto.LocationDetail!.Trim();
            existing.LocationBuilding = dto.LocationBuilding!.Trim();
            existing.LocationRoom = dto.LocationRoom!.Trim();
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime.Value;
            existing.Status = dto.Status ?? existing.Status;

            await _uow.TimeslotRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            var updated = await GetTimeslotQuery().FirstAsync(t => t.Id == existing.Id);
            return MapToDto(updated);
        }
        public async Task<IEnumerable<ImportTimeslotRecordDto>> ImportTimeslotsAsync(int classId, IFormFile file, int creatorId)
        {
            // 1. Validate Class existence and status
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            if (targetClass.Status == (int)ClassStatusEnum.Completed || targetClass.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot import timeslots for completed or cancelled classes.");

            // 2. Validate Creator/Instructor Authorization (Allow Admin to bypass this check)
            var creatorUser = await _uow.UserRepository.GetByIdAsync(creatorId);
            if (creatorUser?.Role == (int)UserRoleEnum.Instructor)
            {
                var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                    ci.ClassId == classId && ci.InstructorId == creatorId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException($"Instructor {creatorId} is not assigned to class {classId}.");
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var resultRecords = new List<ImportTimeslotRecordDto>();
            var timeslotsToCreate = new List<Timeslot>();

            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                var dataTable = result.Tables[0];
                int rowNum = 1;

                foreach (DataRow row in dataTable.Rows)
                {
                    rowNum++;
                    var record = new ImportTimeslotRecordDto { RowNumber = rowNum };

                    try
                    {
                        // Safely read and trim values (assuming structure: Name, LocDetail, LocBuilding, LocRoom, StartTime, EndTime)
                        record.Name = row[0]?.ToString()?.Trim();
                        record.LocationDetail = row[1]?.ToString()?.Trim();
                        record.LocationBuilding = row[2]?.ToString()?.Trim();
                        record.LocationRoom = row[3]?.ToString()?.Trim();

                        string startTimeStr = row[4]?.ToString()?.Trim() ?? "";
                        string endTimeStr = row[5]?.ToString()?.Trim() ?? "";

                        // Basic Validation for required fields
                        if (string.IsNullOrWhiteSpace(record.Name) && string.IsNullOrWhiteSpace(startTimeStr)) continue; // Skip empty rows
                        if (string.IsNullOrWhiteSpace(record.Name)) throw new ArgumentException("Name is required (Column A).");
                        if (!DateTime.TryParse(startTimeStr, out var startTime)) throw new ArgumentException("Invalid Start Time format (Column E).");
                        if (!DateTime.TryParse(endTimeStr, out var endTime)) throw new ArgumentException("Invalid End Time format (Column F).");
                        if (string.IsNullOrWhiteSpace(record.LocationDetail)) throw new ArgumentException("Location Detail is required (Column B).");

                        record.StartTime = startTime;
                        record.EndTime = endTime;

                        // Apply Duration Validation (Issue 4)
                        ValidateTimeslotDuration(startTime, endTime);

                        var newTimeslot = new Timeslot
                        {
                            ClassId = classId,
                            Name = record.Name!,
                            LocationDetail = record.LocationDetail!,
                            LocationBuilding = record.LocationBuilding ?? string.Empty,
                            LocationRoom = record.LocationRoom ?? string.Empty,
                            StartTime = startTime,
                            EndTime = endTime,
                            Status = (int)TimeslotStatusEnum.NotStarted,
                            IsDeleted = false
                        };

                        timeslotsToCreate.Add(newTimeslot);
                        resultRecords.Add(record);
                    }
                    catch (Exception ex)
                    {
                        record.ErrorMessage = $"Error in row {rowNum}: {ex.Message}";
                        resultRecords.Add(record);
                    }
                }
            }

            // Save valid records to DB
            if (timeslotsToCreate.Any())
            {
                foreach (var timeslot in timeslotsToCreate)
                {
                    await _uow.TimeslotRepository.CreateAsync(timeslot);
                }
                await _uow.SaveChangesAsync();
            }

            return resultRecords;
        }
        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByClassAndInstructorAsync(int classId, int instructorId)
        {
            // [FIX] Check user role first. Only enforce assignment check for Instructors.
            var user = await _uow.UserRepository.GetByIdAsync(instructorId);

            // If the user is an Instructor, they MUST be assigned to the class.
            // If the user is Admin (or other authorized roles), we skip this check.
            if (user != null && user.Role == (int)UserRoleEnum.Instructor)
            {
                var isAssigned = await _uow.ClassInstructorRepository.ExistsAsync(ci =>
                    ci.ClassId == classId && ci.InstructorId == instructorId);

                if (!isAssigned)
                    throw new UnauthorizedAccessException($"Instructor {instructorId} is not assigned to class {classId}.");
            }

            var timeslots = await GetTimeslotQuery()
                .Where(t => t.ClassId == classId)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            return timeslots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByInstructorForWeekAsync(int instructorId, DateTime dateInWeek)
        {
            var startOfWeek = GetStartOfWeek(dateInWeek);
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
                    AttendanceStatus = status,

                    // [ADDED] Map the description to the Note field
                    Note = attendance?.Description
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
                    // This attaches the 'attendance' entity to the Context
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

                // [FIX] Break the graph to prevent "already being tracked" error.
                // We nullify the navigation property so UpdateAsync(timeslot) 
                // doesn't try to attach the duplicate Attendance entities we loaded earlier.
                timeslot.Attendances = null;
                timeslot.Class = null; // Good practice to detach Class as well to be safe

                await _uow.TimeslotRepository.UpdateAsync(timeslot);
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

        public async Task<IEnumerable<TimeslotDto>> GetTimeslotsByTraineeForWeekAsync(int traineeId, DateTime dateInWeek)
        {
            var startOfWeek = GetStartOfWeek(dateInWeek);
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

        public async Task<IEnumerable<TraineeAttendanceRecordDto>> GetTraineeAttendanceHistoryAsync(int classId, int traineeId)
        {
            // 1. Check Enrollment
            var enrollment = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == traineeId && e.IsDeleted != true);

            if (enrollment == null)
                throw new UnauthorizedAccessException($"Trainee {traineeId} is not enrolled in class {classId}.");

            // 2. Get all Timeslots for the Class
            var timeslots = await GetTimeslotQuery()
                .Where(t => t.ClassId == classId)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            if (!timeslots.Any()) return Enumerable.Empty<TraineeAttendanceRecordDto>();

            // 3. Get existing Attendance records for this Enrollment
            var attendances = await _uow.AttendanceRepository.GetAllAsQueryable()
                .AsNoTracking()
                .Where(a => a.EnrollmentId == enrollment.Id && a.IsDeleted != true)
                .ToListAsync();

            var attendanceMap = attendances.ToDictionary(a => a.TimeslotId);

            // 4. Map to DTO
            var result = timeslots.Select(t =>
            {
                attendanceMap.TryGetValue(t.Id, out var att);

                string status = "NotStarted";
                string? note = null;

                if (att != null && att.Status.HasValue)
                {
                    status = Enum.GetName(typeof(AttendanceStatusEnum), att.Status.Value) ?? "NotStarted";
                    note = att.Description;
                }
                else
                {
                    // Optional: If timeslot is completed/cancelled but no attendance record exists, logic can be handled here.
                    // For now, default to "NotStarted" or implies Absent if strictly required.
                    if (t.Status == (int)TimeslotStatusEnum.Cancelled) status = "Cancelled";
                }

                return new TraineeAttendanceRecordDto
                {
                    TimeslotId = t.Id,
                    TimeslotName = t.Name,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Location = $"{t.LocationRoom} - {t.LocationBuilding}",
                    AttendanceStatus = status,
                    Note = note
                };
            });

            return result;
        }


        #region helper

        private static DateTime GetStartOfWeek(DateTime dateInWeek)
        {
            // Normalize to date only (keep time as 00:00:00)
            var date = dateInWeek.Date;

            // DayOfWeek.Monday is 1, DayOfWeek.Sunday is 0 (in C# default enum)
            // Calculate offset from Monday. 
            // C# DayOfWeek starts from Sunday (0), Monday (1), ..., Saturday (6)
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

            // Return the Monday of the week
            return date.AddDays(-1 * diff);
        }
        private static void ValidateTimeslotDuration(DateTime startTime, DateTime endTime)
        {
            var duration = endTime - startTime;

            if (duration.TotalMinutes < 15)
                throw new ArgumentException($"Timeslot duration ({duration.TotalMinutes:F0} minutes) cannot be shorter than 15 minutes.");

            if (duration.TotalHours > 12)
                throw new ArgumentException($"Timeslot duration ({duration.TotalHours:F0} hours) cannot be longer than 12 hours.");
        }
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
        #endregion
    }
}