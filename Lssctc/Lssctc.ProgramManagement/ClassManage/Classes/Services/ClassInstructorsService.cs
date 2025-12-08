using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassInstructorsService : IClassInstructorsService
    {
        private readonly IUnitOfWork _uow;

        public ClassInstructorsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task AssignInstructorAsync(int classId, int instructorId)
        {
            var classToUpdate = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classToUpdate == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            var instructor = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .Include(i => i.IdNavigation)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found.");

            // BR 1: Check if the user has the 'Instructor' role
            if (instructor.IdNavigation.Role != (int)UserRoleEnum.Instructor)
                throw new InvalidOperationException($"User {instructor.IdNavigation.Fullname} does not have the 'Instructor' role.");

            // BR 3: Check if class already has an instructor
            var existingAssignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AnyAsync(ci => ci.ClassId == classId);

            if (existingAssignment)
                throw new InvalidOperationException("This class already has an instructor assigned.");

            // BR 2: Check if instructor is assigned to another *active* class
            var otherActiveAssignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Include(ci => ci.Class)
                .AnyAsync(ci => ci.InstructorId == instructorId &&
                               (ci.Class.Status == (int)ClassStatusEnum.Open ||
                                ci.Class.Status == (int)ClassStatusEnum.Inprogress));

            if (otherActiveAssignment)
                throw new InvalidOperationException("This instructor is already assigned to another active class.");

            var newAssignment = new ClassInstructor
            {
                ClassId = classId,
                InstructorId = instructorId,
                Position = "Main"
            };

            await _uow.ClassInstructorRepository.CreateAsync(newAssignment);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveInstructorAsync(int classId)
        {
            var classToUpdate = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classToUpdate == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            // BR 4: Can only remove if class is 'Draft'
            if (classToUpdate.Status != (int)ClassStatusEnum.Draft)
                throw new InvalidOperationException("Instructors can only be removed from classes in 'Draft' status.");

            var assignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ci => ci.ClassId == classId);

            if (assignment != null)
            {
                await _uow.ClassInstructorRepository.DeleteAsync(assignment);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<ClassInstructorDto?> GetInstructorByClassIdAsync(int classId)
        {
            var assignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ci => ci.ClassId == classId);

            if (assignment == null)
                return null;

            var instructor = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .Include(i => i.IdNavigation)
                .FirstOrDefaultAsync(i => i.Id == assignment.InstructorId);

            return instructor == null ? null : MapToDto(instructor);
        }

        public async Task<IEnumerable<ClassInstructorDto>> GetAvailableInstructorsAsync(DateTime startDate, DateTime endDate)
        {
            // Validate date range
            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date.");

            // Get all instructors who are assigned to classes with status Open or Inprogress
            // that have date ranges overlapping with the specified date range
            var busyInstructorIds = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Include(ci => ci.Class)
                .Where(ci => ci.Class != null &&
                            (ci.Class.Status == (int)ClassStatusEnum.Open ||
                             ci.Class.Status == (int)ClassStatusEnum.Inprogress) &&
                            // Check date overlap: class starts before query ends AND class ends after query starts
                            ci.Class.StartDate < endDate &&
                            ci.Class.EndDate > startDate)
                .Select(ci => ci.InstructorId)
                .Distinct()
                .ToListAsync();

            // Get all active instructors who are NOT in the busy list
            var availableInstructors = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .Include(i => i.IdNavigation)
                .Where(i => i.IsActive == true &&
                           i.IsDeleted == false &&
                           i.IdNavigation.IsActive == true &&
                           i.IdNavigation.IsDeleted == false &&
                           i.IdNavigation.Role == (int)UserRoleEnum.Instructor &&
                           !busyInstructorIds.Contains(i.Id))
                .ToListAsync();

            return availableInstructors.Select(MapToDto);
        }

        #region Mapping

        private static ClassInstructorDto MapToDto(Instructor i)
        {
            return new ClassInstructorDto
            {
                Id = i.Id,
                Fullname = i.IdNavigation.Fullname, // <-- MODIFIED
                Email = i.IdNavigation.Email, // <-- ADDED
                PhoneNumber = i.IdNavigation.PhoneNumber, // <-- ADDED
                AvatarUrl = i.IdNavigation.AvatarUrl, // <-- ADDED
                InstructorCode = i.InstructorCode,
                HireDate = i.HireDate
            };
        }

        #endregion
    }
}