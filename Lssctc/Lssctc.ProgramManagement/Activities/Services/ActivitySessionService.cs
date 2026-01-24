// File: Lssctc.ProgramManagement/Activities/Services/ActivitySessionService.cs
using Lssctc.ProgramManagement.Activities.Dtos;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public class ActivitySessionService : IActivitySessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService;

        public ActivitySessionService(IUnitOfWork uow, IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
        }

        // --- CORE LOGIC: Create Sessions on Class Start (Task 1) ---
        public async Task CreateSessionsOnClassStartAsync(int classId)
        {
            // 1. Get Class Details (Fix: Get the actual CourseId from ProgramCourse navigation)
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => new {
                    CourseId = c.ProgramCourse.CourseId, // <--- CHANGED: Get the real CourseId
                    c.StartDate,
                    c.EndDate
                })
                .FirstOrDefaultAsync();

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            // 2. Get all distinct Activities linked to the Class's Course
            var courseActivities = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .AsNoTracking() // Optimization
                .Where(cs => cs.CourseId == targetClass.CourseId) // <--- CHANGED: Compare with CourseId
                .Include(cs => cs.Section)
                    .ThenInclude(s => s.SectionActivities)
                    .ThenInclude(sa => sa.Activity)
                .SelectMany(cs => cs.Section.SectionActivities)
                .Select(sa => sa.Activity)
                .Where(a => a.IsDeleted == false)
                .Distinct()
                // Project to anonymous type first to ensure distinctness works on ID
                .Select(a => new { a.Id, a.ActivityTitle, a.ActivityType })
                .ToListAsync();

            if (!courseActivities.Any()) return;

            // 3. Get existing sessions for the current class to prevent duplicates
            var existingSessionActivityIds = await _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(s => s.ClassId == classId)
                .Select(s => s.ActivityId)
                .ToListAsync();

            var existingSet = new HashSet<int>(existingSessionActivityIds);
            var newSessions = new List<ActivitySession>();

            // 4. Create new Sessions
            foreach (var activity in courseActivities)
            {
                if (existingSet.Contains(activity.Id)) continue;

                var newSession = new ActivitySession
                {
                    ClassId = classId,
                    ActivityId = activity.Id,
                    IsActive = false,
                    StartTime = null,
                    EndTime = null,
                };

                // Logic: Material (Type 1) is auto-activated with class duration
                if (activity.ActivityType == (int)ActivityType.Material)
                {
                    newSession.IsActive = true;
                    newSession.StartTime = targetClass.StartDate;
                    newSession.EndTime = targetClass.EndDate;
                }

                newSessions.Add(newSession);
            }

            if (newSessions.Any())
            {
                foreach (var session in newSessions)
                {
                    await _uow.ActivitySessionRepository.CreateAsync(session);
                }
                await _uow.SaveChangesAsync();
            }
        }

        // --- CORE LOGIC: Enforcement Access Check (Task 2) ---
        public async Task CheckActivityAccess(int classId, int activityId)
        {
            var session = await GetSessionQuery()
                .FirstOrDefaultAsync(s => s.ClassId == classId && s.ActivityId == activityId);

            if (session == null)
            {
                // Optional: Try to auto-fix if missing (lazy creation)
                // await CreateSessionsOnClassStartAsync(classId);
                // session = await GetSessionQuery().FirstOrDefaultAsync(...);

                if (session == null)
                    throw new InvalidOperationException($"Activity Session not found for Activity ID {activityId} in Class ID {classId}. Access denied. (Configuration Error)");
            }

            // 1. Check IsActive
            if (session.IsActive == false)
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' is currently inactive. Please contact the instructor.");
            }

            // 2. Check Time Window
            var now = DateTime.UtcNow.AddHours(7);

            if (session.StartTime.HasValue && now < session.StartTime.Value.ToUniversalTime())
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' will be available from {session.StartTime.Value:yyyy-MM-dd HH:mm:ss}.");
            }

            if (session.EndTime.HasValue && now > session.EndTime.Value.ToUniversalTime())
            {
                throw new UnauthorizedAccessException($"Access to Activity '{session.Activity.ActivityTitle}' expired on {session.EndTime.Value:yyyy-MM-dd HH:mm:ss}.");
            }
        }

        // --- CRUD API (Task 3 & 4) ---

        public async Task<ActivitySessionDto> CreateActivitySessionAsync(CreateActivitySessionDto dto)
        {
            var existing = await _uow.ActivitySessionRepository
                .ExistsAsync(s => s.ClassId == dto.ClassId && s.ActivityId == dto.ActivityId);

            if (existing)
                throw new InvalidOperationException($"Activity Session already exists for Class ID {dto.ClassId} and Activity ID {dto.ActivityId}. Use Update API.");

            var newSession = new ActivitySession
            {
                ClassId = dto.ClassId,
                ActivityId = dto.ActivityId,
                IsActive = dto.IsActive,
                // [FIX] Convert Client Input (Vietnam Time) to UTC
                StartTime = dto.StartTime.HasValue ? dto.StartTime : null,
                EndTime = dto.EndTime.HasValue ? dto.EndTime : null
            };

            await _uow.ActivitySessionRepository.CreateAsync(newSession);
            await _uow.SaveChangesAsync();

            var created = await GetSessionQuery().FirstAsync(s => s.Id == newSession.Id);
            return MapToDto(created);
        }

        public async Task<ActivitySessionDto> UpdateActivitySessionAsync(int sessionId, UpdateActivitySessionDto dto)
        {
            var existing = await _uow.ActivitySessionRepository
                                     .GetAllAsQueryable()
                                     .Include(s => s.Activity)
                                     .Include(s => s.Class)
                                     .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (existing == null)
                throw new KeyNotFoundException($"Activity Session with ID {sessionId} not found.");

            existing.IsActive = dto.IsActive;
            existing.StartTime = dto.StartTime.HasValue ? dto.StartTime : null;
            existing.EndTime = dto.EndTime.HasValue ? dto.EndTime : null;

            await _uow.ActivitySessionRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            // Post-Update: Send email notifications to trainees in Inprogress classes (background)
            var classId = existing.ClassId;
            var activityTitle = existing.Activity?.ActivityTitle ?? "Hoạt động";

            // Note: Keep using dto.StartTime/EndTime (Vietnam Time) for email display as the email template expects user-friendly time
            var startTime = dto.StartTime;
            var endTime = dto.EndTime;

            // Fetch class status separately to ensure we have it
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => new { c.Status, c.Name })
                .FirstOrDefaultAsync();

            var classStatus = targetClass?.Status;
            var className = targetClass?.Name ?? "Lớp học";

            // DEBUG: Log class status
            Console.WriteLine($"[ActivitySession Update] ClassId: {classId}, ClassStatus: {classStatus}, Expected: {(int)ClassStatusEnum.Inprogress}");

            // Only send emails if class status is Inprogress (ID 3)
            if (classStatus == (int)ClassStatusEnum.Inprogress)
            {
                Console.WriteLine($"[ActivitySession Update] Class is Inprogress, fetching trainees...");

                // IMPORTANT: Fetch trainee data BEFORE Task.Run to avoid DbContext disposal issues
                var traineeInfos = await _uow.EnrollmentRepository
                    .GetAllAsQueryable()
                    .AsNoTracking()
                    .Where(e => e.ClassId == classId &&
                                (e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                                 e.Status == (int)EnrollmentStatusEnum.Enrolled))
                    .Include(e => e.Trainee)
                        .ThenInclude(t => t.IdNavigation)
                    .Select(e => new TraineeEmailInfo
                    {
                        TraineeFullname = e.Trainee.IdNavigation.Fullname ?? "Học viên",
                        TraineeEmail = e.Trainee.IdNavigation.Email
                    })
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"[ActivitySession Update] Found {traineeInfos.Count} trainees to notify");

                if (traineeInfos.Any())
                {
                    // Now run email sending in background with pre-fetched data
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            Console.WriteLine($"[ActivitySession Update] Starting email sending task...");
                            await SendEmailsToTraineesAsync(traineeInfos, activityTitle, className, startTime, endTime);
                            Console.WriteLine($"[ActivitySession Update] Email sending task completed.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ActivitySession Update] Email sending failed: {ex.Message}");
                        }
                    });
                }
            }
            else
            {
                Console.WriteLine($"[ActivitySession Update] Class is NOT Inprogress (status: {classStatus}), skipping email.");
            }

            var updated = await GetSessionQuery().FirstAsync(s => s.Id == existing.Id);
            return MapToDto(updated);
        }

        // --- Retrieval APIs ---
        public async Task<ActivitySessionDto> GetActivitySessionByIdAsync(int sessionId)
        {
            var session = await GetSessionQuery().FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
                throw new KeyNotFoundException($"Activity Session with ID {sessionId} not found.");

            return MapToDto(session);
        }

        public async Task<IEnumerable<ActivitySessionDto>> GetActivitySessionsByClassIdAsync(int classId)
        {
            await CreateSessionsOnClassStartAsync(classId);

            var sessions = await GetSessionQuery()
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.Activity.ActivityTitle)
                .ToListAsync();

            return sessions.Select(MapToDto);
        }

        // --- Helpers ---
        private IQueryable<ActivitySession> GetSessionQuery()
        {
            return _uow.ActivitySessionRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(s => s.Activity);
        }

        private static ActivitySessionDto MapToDto(ActivitySession s)
        {
            return new ActivitySessionDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ActivityId = s.ActivityId,
                ActivityTitle = s.Activity?.ActivityTitle ?? "Unknown Activity",
                ActivityType = s.Activity?.ActivityType ?? 0,
                IsActive = s.IsActive ?? false,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            };
        }

        #region --- EMAIL NOTIFICATION HELPERS ---

        /// <summary>
        /// Helper class to hold trainee email information
        /// </summary>
        private class TraineeEmailInfo
        {
            public string TraineeFullname { get; set; } = "Học viên";
            public string? TraineeEmail { get; set; }
        }

        /// <summary>
        /// Sends emails to pre-fetched list of trainees (runs in background)
        /// </summary>
        private async Task SendEmailsToTraineesAsync(List<TraineeEmailInfo> traineeInfos, string activityTitle, string className, DateTime? startTime, DateTime? endTime)
        {
            string startTimeFormatted = startTime.HasValue
                ? startTime.Value.ToString("dd/MM/yyyy HH:mm")
                : "Chưa xác định";
            string endTimeFormatted = endTime.HasValue
                ? endTime.Value.ToString("dd/MM/yyyy HH:mm")
                : "Chưa xác định";

            // Send emails to each trainee
            foreach (var trainee in traineeInfos)
            {
                if (string.IsNullOrWhiteSpace(trainee.TraineeEmail))
                {
                    Console.WriteLine($"[Email] Trainee '{trainee.TraineeFullname}' has no email, skipping.");
                    continue;
                }

                try
                {
                    Console.WriteLine($"[Email] Sending email to: {trainee.TraineeEmail}");
                    
                    string emailSubject = $"📅 Cập nhật Lịch trình/Hoạt động - {activityTitle}";
                    string emailBody = GenerateActivitySessionUpdateEmailBody(
                        trainee.TraineeFullname,
                        activityTitle,
                        className,
                        startTimeFormatted,
                        endTimeFormatted);

                    await _mailService.SendEmailAsync(trainee.TraineeEmail, emailSubject, emailBody);
                    Console.WriteLine($"[Email] Successfully sent to: {trainee.TraineeEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email] Failed to send to {trainee.TraineeEmail}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sends email notifications to all trainees enrolled in the specified class
        /// when an activity session is updated.
        /// </summary>
        private async Task SendActivitySessionUpdateNotificationsAsync(int classId, string activityTitle, string className, DateTime? startTime, DateTime? endTime)
        {
            Console.WriteLine($"[Email] Fetching trainees for classId: {classId}");
            
            // Find all trainees enrolled in this class with Inprogress or Enrolled enrollment status
            var traineeInfos = await _uow.EnrollmentRepository
                .GetAllAsQueryable()
                .Where(e => e.ClassId == classId &&
                            (e.Status == (int)EnrollmentStatusEnum.Inprogress || 
                             e.Status == (int)EnrollmentStatusEnum.Enrolled))
                .Include(e => e.Trainee)
                    .ThenInclude(t => t.IdNavigation)
                .Select(e => new TraineeEmailInfo
                {
                    TraineeFullname = e.Trainee.IdNavigation.Fullname ?? "Học viên",
                    TraineeEmail = e.Trainee.IdNavigation.Email
                })
                .Distinct()
                .ToListAsync();

            Console.WriteLine($"[Email] Found {traineeInfos.Count} trainees to notify");

            if (!traineeInfos.Any())
            {
                Console.WriteLine($"[Email] No trainees to notify, returning.");
                return;
            }

            await SendEmailsToTraineesAsync(traineeInfos, activityTitle, className, startTime, endTime);
        }

        /// <summary>
        /// Generates the HTML email body for activity session update notifications (Vietnamese).
        /// </summary>
        private static string GenerateActivitySessionUpdateEmailBody(string traineeFullname, string activityTitle, string className, string startTime, string endTime)
        {
            return $@"
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
            background: linear-gradient(135deg, #FF9800 0%, #F57C00 100%); 
            color: #ffffff; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .email-header h1 {{ 
            margin: 0; 
            font-size: 24px; 
            font-weight: 600; 
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
        .update-info {{ 
            background-color: #fff3e0; 
            border-left: 4px solid #FF9800; 
            padding: 20px; 
            margin: 20px 0; 
            border-radius: 5px; 
        }}
        .update-info-title {{ 
            font-size: 18px; 
            font-weight: 600; 
            color: #E65100; 
            margin-bottom: 15px; 
        }}
        .info-row {{ 
            padding: 8px 0; 
            border-bottom: 1px solid #ffe0b2; 
        }}
        .info-row:last-child {{ 
            border-bottom: none; 
        }}
        .info-label {{ 
            font-weight: 600; 
            color: #666; 
        }}
        .info-value {{ 
            color: #333; 
            font-weight: 500; 
        }}
        .cta-button {{ 
            display: inline-block; 
            background: linear-gradient(135deg, #FF9800 0%, #F57C00 100%); 
            color: #ffffff; 
            padding: 14px 30px; 
            text-decoration: none; 
            border-radius: 5px; 
            font-weight: 600; 
            font-size: 16px; 
            margin: 20px 0; 
            text-align: center; 
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
            color: #E65100; 
            font-weight: 600; 
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <div class='icon'>📅</div>
            <h1>Cập nhật Lịch trình/Hoạt động</h1>
        </div>
        
        <div class='email-body'>
            <div class='greeting'>
                Xin chào <span class='highlight'>{traineeFullname}</span>,
            </div>
            
            <div class='message'>
                Hoạt động/Lịch trình trong lớp học <strong>{className}</strong> của bạn vừa được cập nhật. Vui lòng kiểm tra lại thông tin chi tiết bên dưới.
            </div>
            
            <div class='update-info'>
                <div class='update-info-title'>📋 Chi tiết cập nhật</div>
                <div class='info-row'>
                    <span class='info-label'>Hoạt động:</span>
                    <span class='info-value'>{activityTitle}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Lớp học:</span>
                    <span class='info-value'>{className}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Thời gian bắt đầu:</span>
                    <span class='info-value'>{startTime}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Thời gian kết thúc:</span>
                    <span class='info-value'>{endTime}</span>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='#' class='cta-button'>Xem chi tiết</a>
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                Nếu bạn có bất kỳ câu hỏi nào về lịch trình cập nhật, vui lòng liên hệ với giảng viên của bạn.
            </div>
            
            <div class='message' style='margin-top: 20px;'>
                Trân trọng,<br>
                <strong>Đội ngũ Quản lý Đào tạo LSSCTC</strong>
            </div>
        </div>
        
        <div class='footer'>
            <p>© 2024 Trung tâm Đào tạo LSSCTC. Đã đăng ký bản quyền.</p>
            <p class='footer-note'>
                Đây là tin nhắn tự động. Vui lòng không trả lời trực tiếp email này.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}