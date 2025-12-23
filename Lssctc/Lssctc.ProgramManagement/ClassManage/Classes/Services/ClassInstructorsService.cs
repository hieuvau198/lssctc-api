using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Lssctc.ProgramManagement.Accounts.Authens.Services; // [1] Add namespace
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassInstructorsService : IClassInstructorsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService; // [2] Add private field

        // [2] Inject IMailService in constructor
        public ClassInstructorsService(IUnitOfWork uow, IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
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

            if (instructor.IdNavigation.Role != (int)UserRoleEnum.Instructor)
                throw new InvalidOperationException($"User {instructor.IdNavigation.Fullname} does not have the 'Instructor' role.");

            var existingAssignment = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AnyAsync(ci => ci.ClassId == classId);

            if (existingAssignment)
                throw new InvalidOperationException("This class already has an instructor assigned.");

            var targetClassTimeslots = await _uow.TimeslotRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(t => t.ClassId == classId && t.IsDeleted == false)
                .Select(t => new { t.StartTime, t.EndTime })
                .ToListAsync();

            if (targetClassTimeslots.Any())
            {
                var otherClassIds = await _uow.ClassInstructorRepository
                    .GetAllAsQueryable()
                    .Where(ci => ci.InstructorId == instructorId && ci.ClassId != classId)
                    .Select(ci => ci.ClassId)
                    .ToListAsync();

                if (otherClassIds.Any())
                {
                    var instructorExistingTimeslots = await _uow.TimeslotRepository
                        .GetAllAsQueryable()
                        .AsNoTracking()
                        .Where(t => otherClassIds.Contains(t.ClassId) && t.IsDeleted == false)
                        .Select(t => new { t.Class.Name, t.StartTime, t.EndTime })
                        .ToListAsync();

                    foreach (var targetSlot in targetClassTimeslots)
                    {
                        foreach (var existingSlot in instructorExistingTimeslots)
                        {
                            if (targetSlot.StartTime < existingSlot.EndTime && targetSlot.EndTime > existingSlot.StartTime)
                            {
                                throw new InvalidOperationException(
                                    $"Instructor has a schedule conflict. " +
                                    $"Target class slot ({targetSlot.StartTime:g} - {targetSlot.EndTime:t}) overlaps with " +
                                    $"existing class '{existingSlot.Name}' slot ({existingSlot.StartTime:g} - {existingSlot.EndTime:t})."
                                );
                            }
                        }
                    }
                }
            }

            var newAssignment = new ClassInstructor
            {
                ClassId = classId,
                InstructorId = instructorId,
                Position = "Main"
            };

            await _uow.ClassInstructorRepository.CreateAsync(newAssignment);
            await _uow.SaveChangesAsync();

            // [3] Send Email Notification in Vietnamese
            try
            {
                var emailSubject = $"[Thông báo] Phân công giảng dạy lớp {classToUpdate.ClassCode} - {classToUpdate.Name}";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h3>Xin chào {instructor.IdNavigation.Fullname},</h3>
                        <p>Bạn đã được phân công làm giảng viên cho lớp học: <strong>{classToUpdate.Name} ({classToUpdate.ClassCode})</strong>.</p>
                        <p>Thông tin lớp học:</p>
                        <ul>
                            <li><strong>Mã lớp:</strong> {classToUpdate.ClassCode}</li>
                            <li><strong>Tên lớp:</strong> {classToUpdate.Name}</li>
                            <li><strong>Ngày bắt đầu:</strong> {classToUpdate.StartDate:dd/MM/yyyy}</li>
                        </ul>
                        <p>Vui lòng đăng nhập vào hệ thống để kiểm tra lịch dạy chi tiết và tài liệu liên quan.</p>
                        <br/>
                        <p>Trân trọng,</p>
                        <p><strong>Ban Quản lý Đào tạo</strong></p>
                    </div>";

                await _mailService.SendEmailAsync(instructor.IdNavigation.Email, emailSubject, emailBody);
            }
            catch (Exception)
            {
                // Optionally log the error here. 
                // We swallow the exception to ensure the API call doesn't fail just because the email failed,
                // since the assignment data was already committed successfully.
            }
        }

        public async Task RemoveInstructorAsync(int classId)
        {
            var classToUpdate = await _uow.ClassRepository.GetByIdAsync(classId);
            if (classToUpdate == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

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
            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date.");

            var busyInstructorIds = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .Include(ci => ci.Class)
                .Where(ci => ci.Class != null &&
                            (ci.Class.Status == (int)ClassStatusEnum.Open ||
                             ci.Class.Status == (int)ClassStatusEnum.Inprogress) &&
                            ci.Class.StartDate < endDate &&
                            ci.Class.EndDate > startDate)
                .Select(ci => ci.InstructorId)
                .Distinct()
                .ToListAsync();

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
                Fullname = i.IdNavigation.Fullname,
                Email = i.IdNavigation.Email,
                PhoneNumber = i.IdNavigation.PhoneNumber,
                AvatarUrl = i.IdNavigation.AvatarUrl,
                InstructorCode = i.InstructorCode,
                HireDate = i.HireDate
            };
        }

        #endregion
    }
}