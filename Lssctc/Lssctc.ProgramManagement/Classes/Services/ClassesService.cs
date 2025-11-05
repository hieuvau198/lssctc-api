using Lssctc.ProgramManagement.Classes.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Classes.Services
{
    public class ClassesService : IClassesService
    {
        private readonly IUnitOfWork _uow;
        public ClassesService(IUnitOfWork uow)
        {
            _uow = uow;
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

            if (existing.StartDate < DateTime.UtcNow || existing.EndDate <= existing.StartDate.AddDays(2))
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
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment); // No await, just mark for update
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
