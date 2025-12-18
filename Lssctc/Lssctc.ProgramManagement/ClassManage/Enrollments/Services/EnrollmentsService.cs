using Lssctc.ProgramManagement.Accounts.Authens.Services;
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
        private readonly IMailService _mailService;

        public EnrollmentsService(IUnitOfWork uow, IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
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
                
                // Fetch the updated enrollment with all required navigation properties
                var updatedEnrollment = await GetEnrollmentQuery().FirstAsync(e => e.Id == existingEnrollment.Id);
                
                // Send email notification for re-enrollment
                await SendEnrollmentEmailAsync(updatedEnrollment);
                
                return MapToDto(updatedEnrollment);
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
            
            // Send email notification
            await SendEnrollmentEmailAsync(created);
            
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

        #region Internal Methods
        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            if (enrollment == null) throw new ArgumentNullException(nameof(enrollment));

            await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateEnrollmentsAsync(IEnumerable<Enrollment> enrollments)
        {
            if (enrollments == null || !enrollments.Any()) return;

            foreach (var enrollment in enrollments)
            {
                await _uow.EnrollmentRepository.UpdateAsync(enrollment);
            }
            await _uow.SaveChangesAsync();
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

        /// <summary>
        /// Sends enrollment notification email to the trainee
        /// </summary>
        private async Task SendEnrollmentEmailAsync(Enrollment enrollment)
        {
            try
            {
                // Extract trainee information
                string traineeFullname = enrollment.Trainee?.IdNavigation?.Fullname ?? "Student";
                string traineeEmail = enrollment.Trainee?.IdNavigation?.Email;

                // Validate email address
                if (string.IsNullOrWhiteSpace(traineeEmail))
                {
                    // Log or silently skip if no email
                    return;
                }

                // Extract class information
                string className = enrollment.Class?.Name ?? "N/A";
                string classCode = enrollment.Class?.ClassCode?.Name ?? "N/A";
                string startDate = enrollment.Class?.StartDate.ToString("dd/MM/yyyy") ?? "TBD";
                string endDate = enrollment.Class?.EndDate?.ToString("dd/MM/yyyy") ?? "TBD";

                string emailSubject = $"🎓 Enrollment Confirmation - {className}";

                string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            margin: 0; 
            padding: 0; 
            background-color: #f4f4f4; 
        }}
        .email-container {{ 
            max-width: 600px; 
            margin: 20px auto; 
            background-color: #ffffff; 
            border-radius: 10px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1); 
            overflow: hidden; 
        }}
        .email-header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: #ffffff; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .email-header h1 {{ 
            margin: 0; 
            font-size: 28px; 
            font-weight: 600; 
        }}
        .email-header .icon {{ 
            font-size: 48px; 
            margin-bottom: 10px; 
        }}
        .email-body {{ 
            padding: 30px; 
        }}
        .greeting {{ 
            font-size: 18px; 
            font-weight: 500; 
            color: #333; 
            margin-bottom: 20px; 
        }}
        .message {{ 
            font-size: 16px; 
            color: #555; 
            margin-bottom: 25px; 
            line-height: 1.8; 
        }}
        .class-info {{ 
            background-color: #f8f9fa; 
            border-left: 4px solid #667eea; 
            padding: 20px; 
            margin: 20px 0; 
            border-radius: 5px; 
        }}
        .class-info-title {{ 
            font-size: 20px; 
            font-weight: 600; 
            color: #667eea; 
            margin-bottom: 15px; 
        }}
        .info-row {{ 
            display: flex; 
            padding: 8px 0; 
            border-bottom: 1px solid #e0e0e0; 
        }}
        .info-row:last-child {{ 
            border-bottom: none; 
        }}
        .info-label {{ 
            font-weight: 600; 
            color: #666; 
            min-width: 120px; 
        }}
        .info-value {{ 
            color: #333; 
            font-weight: 500; 
        }}
        .cta-button {{ 
            display: inline-block; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: #ffffff; 
            padding: 14px 30px; 
            text-decoration: none; 
            border-radius: 5px; 
            font-weight: 600; 
            font-size: 16px; 
            margin: 20px 0; 
            text-align: center; 
        }}
        .cta-button:hover {{ 
            opacity: 0.9; 
        }}
        .divider {{ 
            height: 1px; 
            background-color: #e0e0e0; 
            margin: 25px 0; 
        }}
        .footer {{ 
            background-color: #f8f9fa; 
            padding: 20px; 
            text-align: center; 
            font-size: 14px; 
            color: #666; 
        }}
        .footer-note {{ 
            margin-top: 15px; 
            font-size: 12px; 
            color: #999; 
        }}
        .highlight {{ 
            color: #667eea; 
            font-weight: 600; 
        }}
        @media only screen and (max-width: 600px) {{ 
            .email-container {{ 
                margin: 10px; 
                border-radius: 5px; 
            }}
            .email-header {{ 
                padding: 20px 15px; 
            }}
            .email-header h1 {{ 
                font-size: 22px; 
            }}
            .email-body {{ 
                padding: 20px 15px; 
            }}
            .info-row {{ 
                flex-direction: column; 
            }}
            .info-label {{ 
                margin-bottom: 5px; 
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <div class='icon'>🎓</div>
            <h1>Enrollment Confirmed!</h1>
        </div>
        
        <div class='email-body'>
            <div class='greeting'>
                Dear <span class='highlight'>{traineeFullname}</span>,
            </div>
            
            <div class='message'>
                Congratulations! We are delighted to inform you that you have been <strong>successfully enrolled</strong> in the training class. We look forward to supporting you on your learning journey.
            </div>
            
            <div class='class-info'>
                <div class='class-info-title'>📋 Class Information</div>
                <div class='info-row'>
                    <span class='info-label'>Class Name:</span>
                    <span class='info-value'>{className}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Class Code:</span>
                    <span class='info-value'>{classCode}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Start Date:</span>
                    <span class='info-value'>{startDate}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>End Date:</span>
                    <span class='info-value'>{endDate}</span>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='#' class='cta-button'>View My Schedule</a>
            </div>
            
            <div class='divider'></div>
            
            <div class='message'>
                <strong>Next Steps:</strong>
                <ul style='margin-top: 10px; padding-left: 20px:'>
                    <li>Log in to the training management system</li>
                    <li>Review your class schedule and materials</li>
                    <li>Prepare any required documents or prerequisites</li>
                    <li>Contact your instructor if you have any questions</li>
                </ul>
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                If you have any questions or concerns, please don't hesitate to reach out to our support team.
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                Best regards,<br>
                <strong>LSSCTC Training Management Team</strong>
            </div>
        </div>
        
        <div class='footer'>
            <p>© 2024 LSSCTC Training Center. All rights reserved.</p>
            <p class='footer-note'>
                This is an automated message. Please do not reply directly to this email.
            </p>
        </div>
    </div>
</body>
</html>";

                await _mailService.SendEmailAsync(traineeEmail, emailSubject, emailBody);
            }
            catch (Exception)
            {
                // Silently ignore email sending failures to avoid rolling back the enrollment transaction
                // In production, you might want to log this error
            }
        }

        #endregion
    }
}