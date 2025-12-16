using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ClassImportHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IMailService _mailService;
        private static readonly Random _random = new Random();
        private const int VietnamTimeZoneOffset = 7;

        public ClassImportHandler(IUnitOfWork uow, IMailService mailService)
        {
            _uow = uow;
            _mailService = mailService;
        }

        public async Task<string> ImportTraineesToClassAsync(int classId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty or null.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid file format. Please upload an Excel file (.xlsx).");

            // Verify that the class exists and get class details for email
            var targetClass = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.Enrollments)
                .Include(c => c.ClassCode)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (targetClass == null)
                throw new KeyNotFoundException($"Class with ID {classId} not found.");

            int createdUsersCount = 0;
            int enrolledCount = 0;
            int skippedCount = 0;
            int totalRows = 0;
            var errors = new List<string>();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new Exception("Excel file does not contain any worksheets.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        totalRows = rowCount - 1; // Exclude header row

                        if (rowCount < 2)
                            throw new Exception("Excel file must contain at least one data row (plus header row).");

                        // Start a transaction for batch import
                        var dbContext = _uow.GetDbContext();
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                // Process each row (skip header row 1)
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        // Read values from Excel
                                        string? username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        string? email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        string? fullname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        string? password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        string? phoneNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        string? avatarUrl = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        // Skip completely empty rows
                                        if (string.IsNullOrWhiteSpace(username) &&
                                            string.IsNullOrWhiteSpace(email) &&
                                            string.IsNullOrWhiteSpace(fullname))
                                        {
                                            continue;
                                        }

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(username) ||
                                            string.IsNullOrWhiteSpace(email) ||
                                            string.IsNullOrWhiteSpace(fullname) ||
                                            string.IsNullOrWhiteSpace(password))
                                        {
                                            errors.Add($"Row {row}: Missing required fields (Username, Email, Fullname, or Password).");
                                            skippedCount++;
                                            continue;
                                        }

                                        // Step 1: Find or Create User
                                        var existingUser = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .FirstOrDefaultAsync(u =>
                                                (u.Username == username || u.Email.ToLower() == email.ToLower())
                                                && !u.IsDeleted);

                                        int userId;
                                        bool userCreated = false;
                                        string traineeFullname = fullname;
                                        string traineeEmail = email;

                                        if (existingUser != null)
                                        {
                                            // Use existing user
                                            userId = existingUser.Id;
                                            traineeFullname = existingUser.Fullname;
                                            traineeEmail = existingUser.Email;
                                        }
                                        else
                                        {
                                            // Create new trainee user
                                            string hashedPassword = PasswordHashHandler.HashPassword(password);
                                            string traineeCode = await GenerateUniqueTraineeCode();

                                            var traineeProfile = new TraineeProfile { };

                                            var trainee = new Trainee
                                            {
                                                TraineeCode = traineeCode,
                                                IsActive = true,
                                                IsDeleted = false,
                                                TraineeProfile = traineeProfile
                                            };

                                            var user = new User
                                            {
                                                Username = username,
                                                Password = hashedPassword,
                                                Email = email,
                                                Fullname = fullname,
                                                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                                                AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
                                                Role = (int)UserRoleEnum.Trainee,
                                                IsActive = true,
                                                IsDeleted = false,
                                                Trainee = trainee
                                            };

                                            await _uow.UserRepository.CreateAsync(user);
                                            await _uow.SaveChangesAsync(); // Save to get the UserId

                                            userId = user.Id;
                                            createdUsersCount++;
                                            userCreated = true;
                                        }

                                        // Step 2: Enroll in Class (if not already enrolled)
                                        var existingEnrollment = await _uow.EnrollmentRepository
                                            .GetAllAsQueryable()
                                            .FirstOrDefaultAsync(e => e.ClassId == classId && e.TraineeId == userId);

                                        if (existingEnrollment != null)
                                        {
                                            skippedCount++;
                                            if (userCreated)
                                            {
                                                errors.Add($"Row {row}: User '{username}' was created but is already enrolled in this class.");
                                            }
                                            continue;
                                        }

                                        // Check class capacity
                                        if (targetClass.Capacity.HasValue)
                                        {
                                            int currentEnrolled = await _uow.EnrollmentRepository
                                                .GetAllAsQueryable()
                                                .CountAsync(e => e.ClassId == classId &&
                                                    (e.Status == (int)EnrollmentStatusEnum.Enrolled ||
                                                     e.Status == (int)EnrollmentStatusEnum.Inprogress ||
                                                     e.Status == (int)EnrollmentStatusEnum.Pending));

                                            if (currentEnrolled >= targetClass.Capacity.Value)
                                            {
                                                errors.Add($"Row {row}: Class is full. Cannot enroll user '{username}'.");
                                                skippedCount++;
                                                continue;
                                            }
                                        }

                                        // Create new enrollment
                                        var newEnrollment = new Enrollment
                                        {
                                            ClassId = classId,
                                            TraineeId = userId,
                                            EnrollDate = DateTime.UtcNow,
                                            Status = (int)EnrollmentStatusEnum.Enrolled,
                                            IsActive = true,
                                            IsDeleted = false
                                        };

                                        await _uow.EnrollmentRepository.CreateAsync(newEnrollment);
                                        enrolledCount++;

                                        // Step 3: Send Email Notification
                                        try
                                        {
                                            string classCode = targetClass.ClassCode?.Name ?? "N/A";

                                            // [Fix] Display Dates in Vietnam Time
                                            string startDate = targetClass.StartDate.AddHours(VietnamTimeZoneOffset).ToString("dd/MM/yyyy");
                                            string endDate = targetClass.EndDate.HasValue
                                                ? targetClass.EndDate.Value.AddHours(VietnamTimeZoneOffset).ToString("dd/MM/yyyy")
                                                : "TBD";

                                            string emailSubject = $"🎓 Enrollment Confirmation - {targetClass.Name}";

                                            // (Simplified Email Body for brevity, reusing the previous style)
                                            string emailBody = GetEnrollmentEmailBody(traineeFullname, targetClass.Name, classCode, startDate, endDate);

                                            await _mailService.SendEmailAsync(traineeEmail, emailSubject, emailBody);
                                        }
                                        catch (Exception emailEx)
                                        {
                                            errors.Add($"Row {row}: User '{username}' enrolled successfully, but failed to send email notification: {emailEx.Message}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Row {row}: {ex.Message}");
                                        skippedCount++;
                                    }
                                }

                                await _uow.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            catch (Exception)
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    }
                }

                var resultMessage = $"Import completed successfully. Processed: {totalRows} rows. Created Users: {createdUsersCount}. Enrolled in Class: {enrolledCount}. Skipped: {skippedCount}.";
                if (errors.Any())
                {
                    resultMessage += $" Errors: {string.Join("; ", errors.Take(10))}";
                    if (errors.Count > 10) resultMessage += $" (and {errors.Count - 10} more...)";
                }

                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing Excel file: {ex.Message}");
            }
        }

        private async Task<string> GenerateUniqueTraineeCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string traineeCode;
            bool isUnique;
            do
            {
                var randomPart = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
                traineeCode = "CS" + randomPart;
                isUnique = !await _uow.TraineeRepository.GetAllAsQueryable().AnyAsync(t => t.TraineeCode == traineeCode);
            } while (!isUnique);
            return traineeCode;
        }

        private string GetEnrollmentEmailBody(string name, string className, string classCode, string start, string end)
        {
            // Keeping the template string separate to keep code clean
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Enrollment Confirmed!</h2>
                <p>Dear <strong>{name}</strong>,</p>
                <p>You have been successfully enrolled in <strong>{className}</strong>.</p>
                <ul>
                    <li><strong>Class Code:</strong> {classCode}</li>
                    <li><strong>Start Date:</strong> {start}</li>
                    <li><strong>End Date:</strong> {end}</li>
                </ul>
                <p>Please log in to the portal to view your schedule.</p>
            </body>
            </html>";
        }
    }
}