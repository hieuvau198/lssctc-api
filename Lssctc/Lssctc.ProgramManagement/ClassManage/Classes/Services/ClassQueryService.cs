using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.Classes.Services;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassQueryService : IClassQueryService
    {
        private readonly IUnitOfWork _uow;
        private const int VietnamTimeZoneOffset = 7;

        public ClassQueryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Standard Queries

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

        #endregion

        #region Filter Queries

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

        #endregion

        #region Trainee Queries

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

        #region New Query API

        public async Task<IEnumerable<ClassWithEnrollmentDto>> GetAvailableClassesByProgramCourseForTraineeAsync(int programId, int courseId, int? traineeId)
        {
            // 1. Get all "Available" classes (Open or Inprogress) for the specific Program & Course
            var availableClasses = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.ProgramId == programId
                         && c.ProgramCourse.CourseId == courseId
                         && (c.Status == (int)ClassStatusEnum.Open || c.Status == (int)ClassStatusEnum.Inprogress))
                .ToListAsync();

            // 2. If traineeId is provided, fetch their active enrollments to check status
            var enrolledClassIds = new HashSet<int>();
            if (traineeId.HasValue)
            {
                var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                    .Where(e => e.TraineeId == traineeId.Value
                             && (e.Status == (int)EnrollmentStatusEnum.Enrolled
                              || e.Status == (int)EnrollmentStatusEnum.Inprogress
                              || e.Status == (int)EnrollmentStatusEnum.Completed))
                    .Select(e => e.ClassId)
                    .ToListAsync();

                enrolledClassIds = new HashSet<int>(enrollments);
            }

            // 3. Map to DTO
            return availableClasses.Select(c => MapToClassWithEnrollmentDto(c, enrolledClassIds.Contains(c.Id)));
        }

        #endregion

        #region Mappers

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

        private static ClassWithEnrollmentDto MapToClassWithEnrollmentDto(Class c, bool isEnrolled)
        {
            string classStatus = c.Status.HasValue ? Enum.GetName(typeof(ClassStatusEnum), c.Status.Value) ?? "Cancelled" : "Cancelled";
            var startDateVn = c.StartDate.AddHours(VietnamTimeZoneOffset);
            var endDateVn = c.EndDate?.AddHours(VietnamTimeZoneOffset);

            return new ClassWithEnrollmentDto
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
                BackgroundImageUrl = c.BackgroundImageUrl,
                IsEnrolled = isEnrolled
            };
        }

        #endregion
    }
}