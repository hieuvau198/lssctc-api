using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.Certificates.Services;
using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Services;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Services;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassesService : IClassesService
    {
        private readonly IUnitOfWork _uow;
        private readonly ClassManageHandler _handler;
        private readonly ClassImportHandler _importHandler;
        private readonly ClassCleanupHandler _cleanupHandler;
        private readonly ITimeslotService _timeslotService;
        private readonly IActivitySessionService _activitySessionService;
        private readonly IFinalExamsService _finalExamsService;
        private readonly IEnrollmentsService _enrollmentsService;
        private readonly ITraineeCertificatesService _traineeCertificatesService;

        private const int VietnamTimeZoneOffset = 7;

        public ClassesService(
            IUnitOfWork uow,
            IMailService mailService,
            ITimeslotService timeslotService,
            IActivitySessionService activitySessionService,
            IFinalExamsService finalExamsService,
            IEnrollmentsService enrollmentsService,
            ITraineeCertificatesService traineeCertificatesService)
        {
            _uow = uow;
            _timeslotService = timeslotService;
            _activitySessionService = activitySessionService;
            _finalExamsService = finalExamsService;
            _enrollmentsService = enrollmentsService;
            _traineeCertificatesService = traineeCertificatesService;

            // Instantiate Handlers
            _handler = new ClassManageHandler(uow);
            _importHandler = new ClassImportHandler(uow, mailService);
            _cleanupHandler = new ClassCleanupHandler(uow);
        }

        #region Classes

        public async Task<IEnumerable<ClassDto>> GetAllClassesAsync()
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? status = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ClassStatusEnum>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(c => c.Status == (int)parsedStatus);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    (c.ClassCode != null && c.ClassCode.Name.ToLower().Contains(searchLower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchLower))
                );
            }

            // Sorting logic
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                bool isDesc = sortDirection?.ToLower() == "desc";
                if (sortBy.ToLower() == "startdate") query = isDesc ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate);
                else if (sortBy.ToLower() == "enddate") query = isDesc ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate);
            }
            else
            {
                query = query.OrderByDescending(c => c.Id);
            }

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);
            return new PagedResult<ClassDto>
            {
                Items = pagedEntities.Items.Select(MapToDto).ToList(),
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ClassDto?> GetClassByIdAsync(int id)
        {
            var c = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? null : MapToDto(c);
        }

        public async Task<ClassDto> CreateClassAsync(CreateClassDto dto)
        {
            var startDateUtc = dto.StartDate.AddHours(-VietnamTimeZoneOffset);
            var endDateUtc = dto.EndDate?.AddHours(-VietnamTimeZoneOffset);

            if (startDateUtc < DateTime.UtcNow.AddDays(-30)) throw new InvalidOperationException("Start date cannot be more than 30 days in the past.");
            if (!endDateUtc.HasValue || endDateUtc <= startDateUtc.AddDays(2)) throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            var existingClassCode = await _uow.ClassCodeRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(cc => cc.Name.ToLower() == dto.ClassCode.Trim().ToLower());
            if (existingClassCode != null) throw new InvalidOperationException($"Class code '{existingClassCode.Name}' already exists.");

            var programCourse = await _uow.ProgramCourseRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(pc => pc.ProgramId == dto.ProgramId && pc.CourseId == dto.CourseId);
            if (programCourse == null) throw new KeyNotFoundException("No matching ProgramCourse found.");

            var classCodeEntity = new ClassCode { Name = dto.ClassCode.Trim() };
            await _uow.ClassCodeRepository.CreateAsync(classCodeEntity);
            await _uow.SaveChangesAsync();

            var newClass = new Class
            {
                Name = dto.Name.Trim(),
                Capacity = dto.Capacity,
                ProgramCourseId = programCourse.Id,
                ClassCodeId = classCodeEntity.Id,
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                Status = (int)ClassStatusEnum.Draft,
                BackgroundImageUrl = dto.BackgroundImageUrl ?? "https://templates.framework-y.com/lightwire/images/wide-1.jpg"
            };

            await _uow.ClassRepository.CreateAsync(newClass);
            await _uow.SaveChangesAsync();
            await _finalExamsService.AutoCreateFinalExamsForClassAsync(newClass.Id);

            return (await GetClassByIdAsync(newClass.Id))!;
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable()
                 .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                 .Include(c => c.ClassCode)
                 .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) throw new KeyNotFoundException($"Class with ID {id} not found.");
            if (existing.Status != (int)ClassStatusEnum.Draft) throw new InvalidOperationException("Only classes in 'Draft' status can be updated.");

            var startDateUtc = dto.StartDate.AddHours(-VietnamTimeZoneOffset);
            var endDateUtc = dto.EndDate?.AddHours(-VietnamTimeZoneOffset);

            if (startDateUtc < DateTime.UtcNow.AddDays(-30)) throw new InvalidOperationException("Start date cannot be more than 30 days in the past.");
            if (!endDateUtc.HasValue || endDateUtc <= startDateUtc.AddDays(2)) throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            existing.Name = dto.Name.Trim();
            existing.Capacity = dto.Capacity;
            existing.Description = dto.Description?.Trim() ?? existing.Description;
            existing.StartDate = startDateUtc;
            existing.EndDate = endDateUtc;
            if (dto.BackgroundImageUrl != null) existing.BackgroundImageUrl = dto.BackgroundImageUrl;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            return MapToDto(existing);
        }

        public async Task OpenClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Class with ID {id} not found.");
            if (existing.Status != (int)ClassStatusEnum.Draft) throw new InvalidOperationException("Only 'Draft' classes can be opened.");

            existing.Status = (int)ClassStatusEnum.Open;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task StartClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ClassInstructors).Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) throw new KeyNotFoundException($"Class with ID {id} not found.");
            if (existing.Status != (int)ClassStatusEnum.Draft && existing.Status != (int)ClassStatusEnum.Open) throw new InvalidOperationException("Only 'Draft' or 'Open' classes can be started.");
            if (existing.ClassInstructors == null || !existing.ClassInstructors.Any()) throw new InvalidOperationException("Cannot start class without instructors.");
            if (existing.Enrollments == null || !existing.Enrollments.Any(e => e.Status == (int)EnrollmentStatusEnum.Enrolled)) throw new InvalidOperationException("Cannot start class without at least one enrolled student.");

            foreach (var enrollment in existing.Enrollments)
            {
                if (enrollment.Status == (int)EnrollmentStatusEnum.Enrolled)
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Pending)
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Rejected;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
            }

            existing.Status = (int)ClassStatusEnum.Inprogress;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            await _handler.EnsureProgressScaffoldingForClassAsync(id);
            await _timeslotService.CreateAttendanceForClassAsync(id);
            await _activitySessionService.CreateSessionsOnClassStartAsync(id);
            await _finalExamsService.AutoCreateFinalExamsForClassAsync(id);
        }

        public async Task CompleteClassAsync(int id)
        {
            // 1. Validate Class
            var existingClass = await _uow.ClassRepository.GetByIdAsync(id);
            if (existingClass == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");
            if (existingClass.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Only 'Inprogress' classes can be completed.");

            // 2. Validate Final Exams Status
            // Get all final exams for this class via enrollments
            var finalExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment)
                .Where(fe => fe.Enrollment.ClassId == id)
                .ToListAsync();

            bool allExamsResolved = finalExams.All(fe =>
                fe.Status == (int)FinalExamStatusEnum.Completed ||
                fe.Status == (int)FinalExamStatusEnum.Cancelled);

            if (!allExamsResolved)
                throw new InvalidOperationException("Cannot complete class. All Final Exams must be either Completed or Cancelled.");

            // 3. Update Enrollments
            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == id)
                .Include(e => e.FinalExams)
                .ToListAsync();

            var enrollmentsToUpdate = new List<Enrollment>();

            foreach (var enrollment in enrollments)
            {
                // Status Inprogress -> Check Exam Result
                if (enrollment.Status == (int)EnrollmentStatusEnum.Inprogress)
                {
                    // Find the relevant final exam (assuming the latest one if multiple exist)
                    var finalExam = enrollment.FinalExams
                        .OrderByDescending(fe => fe.Id)
                        .FirstOrDefault();

                    if (finalExam != null && finalExam.IsPass == true)
                    {
                        enrollment.Status = (int)EnrollmentStatusEnum.Completed;
                    }
                    else
                    {
                        // If IsPass is false OR null, or no exam found, they fail
                        enrollment.Status = (int)EnrollmentStatusEnum.Failed;
                    }
                    enrollmentsToUpdate.Add(enrollment);
                }
                // Ignored Statuses
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Cancelled ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Rejected ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Completed ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Failed)
                {
                    // Unchanged
                    continue;
                }
                // Other Statuses (e.g. Pending, Enrolled) -> Cancelled
                else
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Cancelled;
                    enrollmentsToUpdate.Add(enrollment);
                }
            }

            // Batch update enrollments using EnrollmentService
            if (enrollmentsToUpdate.Any())
            {
                await _enrollmentsService.UpdateEnrollmentsAsync(enrollmentsToUpdate);
            }

            // 4. Update Class Status
            existingClass.Status = (int)ClassStatusEnum.Completed;
            await _uow.ClassRepository.UpdateAsync(existingClass);
            await _uow.SaveChangesAsync();

            // 5. Generate Certificates
            await _traineeCertificatesService.CreateTraineeCertificatesForCompleteClass(id);
        }

        public async Task CancelClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable().Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) throw new KeyNotFoundException($"Class with ID {id} not found.");
            if (existing.Enrollments != null && existing.Enrollments.Any()) throw new InvalidOperationException("Cannot cancel a class with enrolled students.");
            if (existing.Status == (int)ClassStatusEnum.Inprogress || existing.Status == (int)ClassStatusEnum.Completed || existing.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot cancel a class that is in progress, completed, or already cancelled.");

            existing.Status = (int)ClassStatusEnum.Cancelled;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        // Delegated to Helper
        public async Task DeleteClassDataRecursiveAsync(int classId)
        {
            await _cleanupHandler.DeleteClassDataRecursiveAsync(classId);
        }

        // Delegated to Helper
        public async Task<string> ImportTraineesToClassAsync(int classId, IFormFile file)
        {
            return await _importHandler.ImportTraineesToClassAsync(classId, file);
        }

        #endregion

        #region Read Operations (Queries)

        public async Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId)
        {
            var classes = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ProgramCourse).Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.ProgramId == programId && c.ProgramCourse.CourseId == courseId)
                .ToListAsync();
            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            var classes = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ProgramCourse).Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.CourseId == courseId)
                .ToListAsync();
            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId)
        {
            var classes = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ProgramCourse).Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.CourseId == courseId && (c.Status == (int)ClassStatusEnum.Open || c.Status == (int)ClassStatusEnum.Inprogress))
                .ToListAsync();
            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId)
        {
            var classes = await _uow.ClassInstructorRepository.GetAllAsQueryable()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class).ThenInclude(c => c.ProgramCourse)
                .Include(ci => ci.Class).ThenInclude(c => c.ClassCode)
                .Select(ci => ci.Class)
                .ToListAsync();
            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId)
        {
            var classes = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId && (e.Status == (int)EnrollmentStatusEnum.Enrolled || e.Status == (int)EnrollmentStatusEnum.Inprogress || e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(e => e.Class).ThenInclude(c => c.ClassCode)
                .Select(e => e.Class).Distinct().ToListAsync();
            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId && (e.Status == (int)EnrollmentStatusEnum.Enrolled || e.Status == (int)EnrollmentStatusEnum.Inprogress || e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(e => e.Class).ThenInclude(c => c.ClassCode)
                .Select(e => e.Class).Distinct();

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);
            return new PagedResult<ClassDto>
            {
                Items = pagedEntities.Items.Select(MapToDto).ToList(),
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId)
        {
            var enrollment = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == classId && e.TraineeId == traineeId && (e.Status == (int)EnrollmentStatusEnum.Enrolled || e.Status == (int)EnrollmentStatusEnum.Inprogress || e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class).ThenInclude(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(e => e.Class).ThenInclude(c => c.ClassCode)
                .FirstOrDefaultAsync();
            return enrollment == null ? null : MapToDto(enrollment.Class);
        }

        #endregion

        #region Mapping

        private static ClassDto MapToDto(Class c)
        {
            string classStatus = c.Status.HasValue ? Enum.GetName(typeof(ClassStatusEnum), c.Status.Value) ?? "Cancelled" : "Cancelled";
            var startDateVn = c.StartDate.AddHours(VietnamTimeZoneOffset);
            var endDateVn = c.EndDate?.AddHours(VietnamTimeZoneOffset);

            return new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                ClassCode = c.ClassCode?.Name ?? "CLS099",
                ProgramId = c.ProgramCourse.ProgramId,
                CourseId = c.ProgramCourse.CourseId,
                Description = c.Description,
                StartDate = startDateVn,
                EndDate = endDateVn,
                Status = classStatus,
                DurationHours = c.ProgramCourse.Course?.DurationHours,
                BackgroundImageUrl = c.BackgroundImageUrl
            };
        }

        #endregion
    }
}