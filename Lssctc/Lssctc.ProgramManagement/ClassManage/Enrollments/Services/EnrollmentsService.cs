using Lssctc.ProgramManagement.ClassManage.Enrollments.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Enrollments.Services
{
    public class EnrollmentsService : IEnrollmentsService
    {

        private readonly IUnitOfWork _uow;

        public EnrollmentsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Trainee Enrollments

        public async Task<EnrollmentDto> EnrollInClassAsync(int traineeId, CreateEnrollmentDto dto)
        {
            // 1. Find the class and check its status
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            // BR: trainee can enroll in a class only if the class status is 'Open'
            if (targetClass.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("You can only enroll in classes that are 'Open'.");

            // 2. Check for existing enrollment
            var existingEnrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(e => e.ClassId == dto.ClassId && e.TraineeId == traineeId);

            if (existingEnrollment != null)
            {
                // If they cancelled, let them re-enroll. Otherwise, they are already in.
                if (existingEnrollment.Status != (int)EnrollmentStatusEnum.Cancelled && existingEnrollment.Status != (int)EnrollmentStatusEnum.Rejected)
                    throw new InvalidOperationException("You are already enrolled or pending in this class.");

                // Re-enrolling: Update status to Pending
                existingEnrollment.Status = (int)EnrollmentStatusEnum.Pending;
                existingEnrollment.EnrollDate = DateTime.UtcNow;
                await _uow.EnrollmentRepository.UpdateAsync(existingEnrollment);
                await _uow.SaveChangesAsync();
                return MapToDto(existingEnrollment);
            }

            // 3. Check class capacity
            if (targetClass.Capacity.HasValue)
            {
                int currentEnrolled = targetClass.Enrollments.Count(e =>
                    e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                    e.Status == (int)EnrollmentStatusEnum.Pending);

                if (currentEnrolled >= targetClass.Capacity.Value)
                    throw new InvalidOperationException("Class is full.");
            }

            // 4. Create new enrollment
            var newEnrollment = new Enrollment
            {
                ClassId = dto.ClassId,
                TraineeId = traineeId,
                EnrollDate = DateTime.UtcNow,
                Status = (int)EnrollmentStatusEnum.Pending, // BR: enrollment auto have status Pending
                IsActive = true,
                IsDeleted = false
            };

            await _uow.EnrollmentRepository.CreateAsync(newEnrollment);
            await _uow.SaveChangesAsync();

            // We need to fetch the full object for mapping
            var created = await GetEnrollmentQuery().FirstAsync(e => e.Id == newEnrollment.Id);
            return MapToDto(created);
        }

        public async Task WithdrawFromClassAsync(int traineeId, int classId)
        {
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == traineeId);

            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment not found.");

            // BR: trainee can withdraw from a class only if class status is 'Draft' or 'Open'
            if (enrollment.Class.Status != (int)ClassStatusEnum.Draft &&
                enrollment.Class.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("You can only withdraw from a class that has not started.");

            if (enrollment.Status == (int)EnrollmentStatusEnum.Cancelled)
                throw new InvalidOperationException("You are already withdrawn from this class.");

            enrollment.Status = (int)EnrollmentStatusEnum.Cancelled; // BR: enrollment status become 'Cancelled'
            await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();
        }

        public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId)
        {
            var enrollments = await GetEnrollmentQuery()
                .Where(e => e.TraineeId == traineeId)
                .ToListAsync();

            return enrollments.Select(MapToDto);
        }

        public async Task<PagedResult<EnrollmentDto>> GetMyEnrollmentsAsync(int traineeId, int pageNumber, int pageSize)
        {
            var query = GetEnrollmentQuery()
                .Where(e => e.TraineeId == traineeId)
                .Select(e => MapToDto(e));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<EnrollmentDto?> GetMyEnrollmentByIdAsync(int traineeId, int enrollmentId)
        {
            var enrollment = await GetEnrollmentQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.TraineeId == traineeId);

            return enrollment == null ? null : MapToDto(enrollment);
        }

        public async Task<EnrollmentDto?> GetMyEnrollmentByClassIdAsync(int traineeId, int classId)
        {
            var enrollment = await GetEnrollmentQuery()
                .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == traineeId);

            return enrollment == null ? null : MapToDto(enrollment);
        }

        #endregion

        #region Admin/Instructor/Manager Roles

        public async Task<EnrollmentDto> AddTraineeToClassAsync(InstructorAddTraineeDto dto)
        {
            // 1. Find class and check status
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {dto.ClassId} not found.");

            // BR: instructor can add trainee to a class only if class status is 'Draft' or 'Open'
            if (targetClass.Status != (int)ClassStatusEnum.Draft &&
                targetClass.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("You can only add trainees to 'Draft' or 'Open' classes.");

            // 2. Check if trainee exists
            var trainee = await _uow.TraineeRepository.GetByIdAsync(dto.TraineeId);
            if (trainee == null)
                throw new KeyNotFoundException($"Trainee with ID {dto.TraineeId} not found.");

            // 3. Check for existing enrollment
            var existingEnrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(e => e.ClassId == dto.ClassId && e.TraineeId == dto.TraineeId);

            if (existingEnrollment != null)
            {
                // If cancelled/rejected, allow re-add.
                if (existingEnrollment.Status != (int)EnrollmentStatusEnum.Cancelled && existingEnrollment.Status != (int)EnrollmentStatusEnum.Rejected)
                    throw new InvalidOperationException("Trainee is already enrolled or pending in this class.");

                existingEnrollment.Status = (int)EnrollmentStatusEnum.Enrolled; // BR: auto have status 'Enrolled'
                existingEnrollment.EnrollDate = DateTime.UtcNow;
                await _uow.EnrollmentRepository.UpdateAsync(existingEnrollment);
                await _uow.SaveChangesAsync();
                return MapToDto(existingEnrollment);
            }

            // 4. Check class capacity
            if (targetClass.Capacity.HasValue)
            {
                int currentEnrolled = targetClass.Enrollments.Count(e =>
                    e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                    e.Status == (int)EnrollmentStatusEnum.Pending);

                if (currentEnrolled >= targetClass.Capacity.Value)
                    throw new InvalidOperationException("Class is full.");
            }

            // 5. Create new enrollment
            var newEnrollment = new Enrollment
            {
                ClassId = dto.ClassId,
                TraineeId = dto.TraineeId,
                EnrollDate = DateTime.UtcNow,
                Status = (int)EnrollmentStatusEnum.Enrolled, // BR: auto have status 'Enrolled'
                IsActive = true,
                IsDeleted = false
            };

            await _uow.EnrollmentRepository.CreateAsync(newEnrollment);
            await _uow.SaveChangesAsync();

            var created = await GetEnrollmentQuery().FirstAsync(e => e.Id == newEnrollment.Id);
            return MapToDto(created);
        }

        public async Task RemoveTraineeFromClassAsync(int enrollmentId)
        {
            var enrollment = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment not found.");

            // BR: instructor can remove trainee from a class only if class status is 'Draft' or 'Open'
            if (enrollment.Class.Status != (int)ClassStatusEnum.Draft &&
                enrollment.Class.Status != (int)ClassStatusEnum.Open)
                throw new InvalidOperationException("You can only remove trainees from a class that has not started.");

            enrollment.Status = (int)EnrollmentStatusEnum.Cancelled; // BR: enrollment status become 'Cancelled'
            await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();
        }

        public async Task<PagedResult<EnrollmentDto>> GetEnrollmentsForClassAsync(int classId, int pageNumber, int pageSize)
        {
            var query = GetEnrollmentQuery()
                .Where(e => e.ClassId == classId)
                .Select(e => MapToDto(e));

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<EnrollmentDto> ApproveEnrollmentAsync(int enrollmentId)
        {
            var enrollment = await GetEnrollmentQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment not found.");

            if (enrollment.Status != (int)EnrollmentStatusEnum.Pending)
                throw new InvalidOperationException("Only 'Pending' enrollments can be approved.");

            // Check class capacity
            if (enrollment.Class.Capacity.HasValue)
            {
                int currentEnrolled = await _uow.EnrollmentRepository
                    .GetAllAsQueryable()
                    .CountAsync(e => e.ClassId == enrollment.ClassId && e.Status == (int)EnrollmentStatusEnum.Enrolled);

                if (currentEnrolled >= enrollment.Class.Capacity.Value)
                    throw new InvalidOperationException("Class is full. Cannot approve enrollment.");
            }

            enrollment.Status = (int)EnrollmentStatusEnum.Enrolled;
            await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();

            return MapToDto(enrollment);
        }

        public async Task<EnrollmentDto> RejectEnrollmentAsync(int enrollmentId)
        {
            var enrollment = await GetEnrollmentQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment not found.");

            if (enrollment.Status != (int)EnrollmentStatusEnum.Pending)
                throw new InvalidOperationException("Only 'Pending' enrollments can be rejected.");

            enrollment.Status = (int)EnrollmentStatusEnum.Rejected;
            await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();

            return MapToDto(enrollment);
        }

        #endregion

        #region Mappping & Helpers

        private IQueryable<Enrollment> GetEnrollmentQuery()
        {
            return _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.IdNavigation) // Trainee -> User (for Fullname, Email, etc.)
                .Include(e => e.Class)
                    .ThenInclude(c => c.ClassCode);
        }

        private static EnrollmentDto MapToDto(Enrollment e)
        {
            string enrollmentStatus = e.Status.HasValue
                ? Enum.GetName(typeof(EnrollmentStatusEnum), e.Status.Value) ?? "Cancelled"
                : "Cancelled";

            return new EnrollmentDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassName = e.Class?.Name ?? "N/A",
                ClassCode = e.Class?.ClassCode?.Name ?? "N/A",
                TraineeId = e.TraineeId,
                TraineeName = e.Trainee?.IdNavigation.Fullname ?? "N/A",

                // --- MODIFIED MAPPING ---
                Email = e.Trainee?.IdNavigation.Email,
                PhoneNumber = e.Trainee?.IdNavigation.PhoneNumber,
                AvatarUrl = e.Trainee?.IdNavigation.AvatarUrl,
                // --- END MODIFIED MAPPING ---

                EnrollDate = e.EnrollDate,
                Status = enrollmentStatus
            };
        }

        #endregion
    }
}