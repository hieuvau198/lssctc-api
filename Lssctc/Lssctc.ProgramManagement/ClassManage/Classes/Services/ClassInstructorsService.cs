using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassInstructorsService : IClassInstructorsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService;

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

            // Send Email Notification in Vietnamese with Button
            try
            {
                var emailSubject = $"[Thông báo] Phân công giảng dạy lớp {classToUpdate.ClassCode} - {classToUpdate.Name}";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                        <h2 style='color: #0056b3; text-align: center;'>Thông báo Phân công Giảng dạy</h2>
                        <p>Xin chào <strong>{instructor.IdNavigation.Fullname}</strong>,</p>
                        <p>Bạn đã được phân công làm giảng viên chính thức cho lớp học sau:</p>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 5px 0;'><strong>Mã lớp:</strong> {classToUpdate.ClassCode}</p>
                            <p style='margin: 5px 0;'><strong>Tên lớp:</strong> {classToUpdate.Name}</p>
                            <p style='margin: 5px 0;'><strong>Ngày bắt đầu dự kiến:</strong> {classToUpdate.StartDate:dd/MM/yyyy}</p>
                        </div>

                        <p>Vui lòng truy cập hệ thống để xem lịch giảng dạy chi tiết và chuẩn bị tài liệu cần thiết.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://lssctc.site' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Truy cập Hệ thống LSSCTC</a>
                        </div>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='font-size: 12px; color: #666; text-align: center;'>Trân trọng,<br/>Ban Quản lý Đào tạo LSSCTC</p>
                    </div>";

                await _mailService.SendEmailAsync(instructor.IdNavigation.Email, emailSubject, emailBody);
            }
            catch (Exception)
            {
                // Optionally log exception
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