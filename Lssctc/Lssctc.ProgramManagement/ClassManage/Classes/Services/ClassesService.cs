using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassesService : IClassesService
    {
        private readonly IUnitOfWork _uow;
        private readonly ClassManageHandler _handler;
        public ClassesService(IUnitOfWork uow)
        {
            _uow = uow;
            _handler = new ClassManageHandler(uow);
        }

        #region Classes

        public async Task<IEnumerable<ClassDto>> GetAllClassesAsync()
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Select(c => MapToDto(c));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<ClassDto?> GetClassByIdAsync(int id)
        {
            var c = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync(c => c.Id == id);

            return c == null ? null : MapToDto(c);
        }

        public async Task<ClassDto> CreateClassAsync(CreateClassDto dto)
        {
            if (dto.StartDate < DateTime.UtcNow)
                throw new InvalidOperationException("Start date cannot be in the past.");

            if (!dto.EndDate.HasValue || dto.EndDate <= dto.StartDate.AddDays(2))
                throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            if (string.IsNullOrWhiteSpace(dto.ClassCode))
                throw new ArgumentException("Class code is required.");

            // Check for existing class code (case-insensitive)
            var existingClassCode = await _uow.ClassCodeRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(cc => cc.Name.ToLower() == dto.ClassCode.Trim().ToLower());

            if (existingClassCode != null)
                throw new InvalidOperationException($"Class code '{existingClassCode.Name}' already exists.");

            // Find the matching ProgramCourse by ProgramId and CourseId
            var programCourse = await _uow.ProgramCourseRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(pc => pc.ProgramId == dto.ProgramId && pc.CourseId == dto.CourseId);

            if (programCourse == null)
                throw new KeyNotFoundException("No matching ProgramCourse found for the given ProgramId and CourseId.");

            // Create the ClassCode
            var classCodeEntity = new ClassCode
            {
                Name = dto.ClassCode.Trim()
            };
            await _uow.ClassCodeRepository.CreateAsync(classCodeEntity);
            await _uow.SaveChangesAsync();

            // Create the new Class
            var newClass = new Class
            {
                Name = dto.Name.Trim(),
                Capacity = dto.Capacity,
                ProgramCourseId = programCourse.Id,
                ClassCodeId = classCodeEntity.Id,
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = (int)ClassStatusEnum.Draft
            };

            await _uow.ClassRepository.CreateAsync(newClass);
            await _uow.SaveChangesAsync();

            return MapToDto(newClass);
        }


        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync()
                ;
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft)
                throw new InvalidOperationException("Only classes in 'Draft' status can be updated.");

            if (dto.StartDate < DateTime.UtcNow)
                throw new InvalidOperationException("Start date cannot be in the past.");

            if (!dto.EndDate.HasValue || dto.EndDate <= dto.StartDate.AddDays(2))
                throw new InvalidOperationException("End date must be at least 3 days after the start date.");

            existing.Name = dto.Name.Trim();
            existing.Capacity = dto.Capacity;
            existing.Description = dto.Description?.Trim() ?? existing.Description;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            return MapToDto(existing);
        }

        public async Task OpenClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft)
                throw new InvalidOperationException("Only 'Draft' classes can be opened.");

            existing.Status = (int)ClassStatusEnum.Open;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task StartClassAsync(int id)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassInstructors)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Draft &&
                existing.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("Only 'Draft' or 'Open' classes can be started.");

            if (existing.EndDate <= existing.StartDate.AddDays(2))
                throw new InvalidOperationException("Invalid start or end date.");

            if (existing.ClassInstructors == null || !existing.ClassInstructors.Any())
                throw new InvalidOperationException("Cannot start class without instructors.");

            // Check for at least one *enrolled* student, not just *any* enrollment
            if (existing.Enrollments == null || !existing.Enrollments.Any(e => e.Status == (int)EnrollmentStatusEnum.Enrolled))
                throw new InvalidOperationException("Cannot start class without at least one enrolled student.");

            // Update enrollment statuses
            foreach (var enrollment in existing.Enrollments)
            {
                if (enrollment.Status == (int)EnrollmentStatusEnum.Enrolled)
                {
                    // Move enrolled students to Inprogress
                    enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Pending)
                {
                    // Auto-reject pending applications as the class is starting
                    enrollment.Status = (int)EnrollmentStatusEnum.Rejected;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
            }

            existing.Status = (int)ClassStatusEnum.Inprogress;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            await _handler.EnsureProgressScaffoldingForClassAsync(id);

            //// Find all 'NotStarted' progresses for this class and set them to 'InProgress'
            //var progressesToStart = await _uow.LearningProgressRepository
            //    .GetAllAsQueryable()
            //    .Where(lp => lp.Enrollment.ClassId == id && lp.Status == (int)LearningProgressStatusEnum.NotStarted)
            //    .ToListAsync();

            //foreach (var progress in progressesToStart)
            //{
            //    progress.Status = (int)LearningProgressStatusEnum.InProgress;
            //    progress.LastUpdated = DateTime.UtcNow;
            //    await _uow.LearningProgressRepository.UpdateAsync(progress);
            //}

        }

        public async Task CompleteClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Only 'Inprogress' classes can be completed.");

            existing.Status = (int)ClassStatusEnum.Completed;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task CancelClassAsync(int id)
        {
            var existing = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
                throw new KeyNotFoundException($"Class with ID {id} not found.");

            if (existing.Enrollments != null && existing.Enrollments.Any())
                throw new InvalidOperationException("Cannot cancel a class with enrolled students.");

            if (existing.Status == (int)ClassStatusEnum.Inprogress ||
                existing.Status == (int)ClassStatusEnum.Completed ||
                existing.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Cannot cancel a class that is in progress, completed, or already cancelled.");

            existing.Status = (int)ClassStatusEnum.Cancelled;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Classes By other Filters

        public async Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.ProgramId == programId && c.ProgramCourse.CourseId == courseId)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ProgramCourse)
                .Include(c => c.ClassCode)
                .Where(c => c.ProgramCourse.CourseId == courseId)
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId)
        {
            var classes = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ClassCode)
                .Select(ci => ci.Class)
                .ToListAsync();

            return classes.Select(MapToDto);
        }


        #endregion

        #region Classes By Trainee

        public async Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId)
        {
            var classes = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                .Select(e => e.Class)
                .Distinct()
                .ToListAsync();

            return classes.Select(MapToDto);
        }

        public async Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 1. Define the IQueryable. 
            //    We MUST Include related data (Class.ProgramCourse, Class.ClassCode) *before*
            //    we Select the Class.
            var query = _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                // Include the Class and its children *first*
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                // Now Select the fully-loaded Class entity
                .Select(e => e.Class)
                .Distinct();

            var pagedEntities = await query.ToPagedResultAsync(pageNumber, pageSize);

            var dtoItems = pagedEntities.Items.Select(MapToDto).ToList();

            return new PagedResult<ClassDto>
            {
                Items = dtoItems,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId)
        {
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == classId &&
                            e.TraineeId == traineeId &&
                            (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                             e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                             e.Status == (int)EnrollmentStatusEnum.Completed))
                .Include(e => e.Class)
                    .ThenInclude(c => c.ProgramCourse)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode)
                .FirstOrDefaultAsync();

            // If an enrollment is found, map the associated class to its DTO
            return enrollment == null ? null : MapToDto(enrollment.Class);
        }

        #endregion

        #region Mapping

        private static ClassDto MapToDto(Class c)
        {
            string classStatus = c.Status.HasValue
                ? Enum.GetName(typeof(ClassStatusEnum), c.Status.Value) ?? "Cancelled"
                : "Cancelled";

            return new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                ClassCode = c.ClassCode?.Name ?? "CLS099",
                ProgramId = c.ProgramCourse.ProgramId,
                CourseId = c.ProgramCourse.CourseId,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = classStatus
            };
        }

        #endregion
    }
}
