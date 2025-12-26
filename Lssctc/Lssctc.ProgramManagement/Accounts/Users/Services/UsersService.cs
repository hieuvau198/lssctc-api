using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.ProgramManagement.Accounts.Users.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace Lssctc.ProgramManagement.Accounts.Users.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _uow;
        private static readonly Random _random = new Random();

        public UsersService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        #region Users

        public async Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted)
                .Select(u => MapToDto(u));
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return null;

            return MapToDto(user);
        }

        public async Task<PagedResult<UserDto>> GetAllTraineesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.Trainee);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)) ||
                    (u.Fullname != null && u.Fullname.ToLower().Contains(term))
                );

                query = query.OrderByDescending(u =>
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term))
                );
            }

            var result = query.Select(u => MapToDto(u));

            return await result.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<UserDto>> GetAllInstructorsAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.Instructor);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)) ||
                    (u.Fullname != null && u.Fullname.ToLower().Contains(term))
                );

                query = query.OrderByDescending(u =>
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term))
                );
            }

            var result = query.Select(u => MapToDto(u));

            return await result.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<UserDto>> GetAllSimulationManagersAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.SimulationManager);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)) ||
                    (u.Fullname != null && u.Fullname.ToLower().Contains(term))
                );

                query = query.OrderByDescending(u =>
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term))
                );
            }

            var result = query.Select(u => MapToDto(u));

            return await result.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<UserDto> CreateTraineeAccountAsync(CreateUserDto dto)
        {
            bool exists = await _uow.UserRepository
                .GetAllAsQueryable()
                .AnyAsync(u =>
                (u.Username == dto.Username
                || u.Email.ToLower() == dto.Email.ToLower())
                && !u.IsDeleted);
            if (exists)
                throw new Exception("Tên đăng nhập hoặc Email đã tồn tại.");

            string hashedPassword = PasswordHashHandler.HashPassword(dto.Password);

            string traineeCode = await GenerateUniqueTraineeCode();

            var traineeProfile = new TraineeProfile
            {
            };

            var trainee = new Trainee
            {
                TraineeCode = traineeCode,
                IsActive = true,
                IsDeleted = false,
                TraineeProfile = traineeProfile
            };

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Email = dto.Email,
                Fullname = dto.Fullname,
                PhoneNumber = dto.PhoneNumber,
                AvatarUrl = dto.AvatarUrl,
                Role = (int)UserRoleEnum.Trainee,
                IsActive = true,
                IsDeleted = false,
                Trainee = trainee
            };

            await _uow.UserRepository.CreateAsync(user);
            await _uow.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<UserDto> CreateInstructorAccountAsync(CreateUserDto dto)
        {
            bool exists = await _uow.UserRepository
                .GetAllAsQueryable()
                .AnyAsync(u =>
                (u.Username == dto.Username
                || u.Email.ToLower() == dto.Email.ToLower())
                && !u.IsDeleted);
            if (exists)
                throw new Exception("Tên đăng nhập hoặc Email đã tồn tại.");

            string hashedPassword = PasswordHashHandler.HashPassword(dto.Password);
            string instructorCode = await GenerateUniqueInstructorCode();
            var instructorProfile = new InstructorProfile
            {
            };

            var instructor = new Instructor
            {
                InstructorCode = instructorCode,
                IsActive = true,
                IsDeleted = false,
                InstructorProfile = instructorProfile
            };

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Email = dto.Email,
                Fullname = dto.Fullname,
                PhoneNumber = dto.PhoneNumber,
                AvatarUrl = dto.AvatarUrl,
                Role = (int)UserRoleEnum.Instructor,
                IsActive = true,
                IsDeleted = false,
                Instructor = instructor
            };
            await _uow.UserRepository.CreateAsync(user);
            await _uow.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<UserDto> CreateSimulationManagerAccountAsync(CreateUserDto dto)
        {
            bool exists = await _uow.UserRepository
                .GetAllAsQueryable()
                .AnyAsync(u =>
                (u.Username == dto.Username
                || u.Email.ToLower() == dto.Email.ToLower())
                && !u.IsDeleted);
            if (exists)
                throw new Exception("Tên đăng nhập hoặc Email đã tồn tại.");

            string hashedPassword = PasswordHashHandler.HashPassword(dto.Password);

            var simulationManager = new SimulationManager
            {
            };

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Email = dto.Email,
                Fullname = dto.Fullname,
                PhoneNumber = dto.PhoneNumber,
                AvatarUrl = dto.AvatarUrl,
                Role = (int)UserRoleEnum.SimulationManager,
                IsActive = true,
                IsDeleted = false,
                SimulationManager = simulationManager
            };
            await _uow.UserRepository.CreateAsync(user);
            await _uow.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                throw new Exception("Không tìm thấy người dùng.");

            // 1. Check Email Duplicate
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists = await _uow.UserRepository
                    .GetAllAsQueryable()
                    .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()
                                   && u.Id != id
                                   && !u.IsDeleted);

                if (emailExists)
                    throw new Exception("Email đã tồn tại.");

                user.Email = dto.Email;
            }

            // 2. Check PhoneNumber Duplicate
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                // Normalize phone number if needed (e.g., remove spaces) - optional
                var newPhone = dto.PhoneNumber.Trim();

                if (!string.Equals(user.PhoneNumber, newPhone, StringComparison.OrdinalIgnoreCase))
                {
                    bool phoneExists = await _uow.UserRepository
                        .GetAllAsQueryable()
                        .AnyAsync(u => u.PhoneNumber == newPhone
                                       && u.Id != id
                                       && !u.IsDeleted);

                    if (phoneExists)
                        throw new Exception("Số điện thoại đã tồn tại trong hệ thống.");

                    user.PhoneNumber = newPhone;
                }
            }

            user.Fullname = dto.Fullname ?? user.Fullname;
            user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;

            await _uow.UserRepository.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user == null)
                return false;


            var trainee = await _uow.TraineeRepository.GetByIdAsync(id);
            if (trainee != null)
            {
                trainee.IsDeleted = true;
            }

            user.IsDeleted = true;
            user.IsActive = false;

            await _uow.UserRepository.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _uow.UserRepository
                .GetAllAsQueryable()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
        }

        public async Task<bool> ResetPasswordByEmailAsync(string email, string newPassword)
        {
            var user = await _uow.UserRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);

            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");

            // Hash the new password
            user.Password = PasswordHashHandler.HashPassword(newPassword);

            await _uow.UserRepository.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Profiles

        public async Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto dto)
        {
            var user = await _uow.UserRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                throw new Exception("Không tìm thấy người dùng.");

            if (!PasswordHashHandler.VerifyPassword(dto.CurrentPassword, user.Password))
                throw new Exception("Mật khẩu hiện tại không chính xác.");

            user.Password = PasswordHashHandler.HashPassword(dto.NewPassword);

            await _uow.UserRepository.UpdateAsync(user);
            await _uow.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Private Helpers

        private static UserDto MapToDto(User u)
        {
            string? roleName = u.Role.HasValue && Enum.IsDefined(typeof(UserRoleEnum), u.Role.Value)
                ? ((UserRoleEnum)u.Role.Value).ToString()
                : "Unknown";
            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Fullname = u.Fullname,
                Role = roleName,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive
            };
        }

        private async Task<IEnumerable<UserDto>> GetUsersByRole(UserRoleEnum role)
        {
            return await _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)role)
                .Select(u => MapToDto(u))
                .ToListAsync();
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

                isUnique = !await _uow.TraineeRepository
                    .GetAllAsQueryable()
                    .AnyAsync(t => t.TraineeCode == traineeCode);

            } while (!isUnique);
            return traineeCode;
        }

        private async Task<string> GenerateUniqueInstructorCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string instructorCode;
            bool isUnique;

            do
            {
                var randomPart = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
                instructorCode = "INS" + randomPart;
                isUnique = !await _uow.InstructorRepository
                    .GetAllAsQueryable()
                    .AnyAsync(i => i.InstructorCode == instructorCode);
            } while (!isUnique);
            return instructorCode;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        // ... (Import methods have detailed string messages inside)
        public async Task<string> ImportTraineesAsync(IFormFile file)
        {
            // ... (previous validation code) ...
            if (file == null || file.Length == 0)
                throw new Exception("File trống hoặc không tồn tại.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Định dạng file không hợp lệ. Vui lòng tải lên file Excel (.xlsx).");

            // ... (rest of the logic remains similar but with VN messages)
            // Simplified for brevity, assume similar logic structure but messages like:
            // "Username or Email already exists" -> "Tên đăng nhập hoặc Email đã tồn tại"

            // I will return the full logic with updated messages below
            int importedCount = 0;
            int skippedCount = 0;
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
                            throw new Exception("File Excel không chứa worksheet nào.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount < 2)
                            throw new Exception("File Excel phải chứa ít nhất một dòng dữ liệu (ngoài dòng tiêu đề).");

                        var dbContext = _uow.GetDbContext();
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        string? username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        string? email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        string? fullname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        string? password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        string? phoneNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        string? avatarUrl = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        if (string.IsNullOrWhiteSpace(username) &&
                                            string.IsNullOrWhiteSpace(email) &&
                                            string.IsNullOrWhiteSpace(fullname))
                                        {
                                            continue;
                                        }

                                        var missingFields = new List<string>();
                                        if (string.IsNullOrWhiteSpace(username)) missingFields.Add("Username (Cột A)");
                                        if (string.IsNullOrWhiteSpace(email)) missingFields.Add("Email (Cột B)");
                                        if (string.IsNullOrWhiteSpace(fullname)) missingFields.Add("Họ tên (Cột C)");
                                        if (string.IsNullOrWhiteSpace(password)) missingFields.Add("Mật khẩu (Cột D)");

                                        if (missingFields.Any())
                                        {
                                            errors.Add($"Dòng {row}: Thiếu thông tin bắt buộc - {string.Join(", ", missingFields)}");
                                            skippedCount++;
                                            continue;
                                        }

                                        if (!IsValidEmail(email))
                                        {
                                            errors.Add($"Dòng {row}: Định dạng email không hợp lệ '{email}' (Cột B)");
                                            skippedCount++;
                                            continue;
                                        }

                                        var existingUser = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .Where(u => !u.IsDeleted && (u.Username == username || u.Email.ToLower() == email.ToLower()))
                                            .Select(u => new { u.Username, u.Email })
                                            .FirstOrDefaultAsync();

                                        if (existingUser != null)
                                        {
                                            if (existingUser.Username == username && existingUser.Email.ToLower() == email.ToLower())
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' và Email '{email}' đã tồn tại trong hệ thống");
                                            else if (existingUser.Username == username)
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' đã tồn tại (Cột A)");
                                            else
                                                errors.Add($"Dòng {row}: Email '{email}' đã tồn tại (Cột B)");

                                            skippedCount++;
                                            continue;
                                        }

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
                                        importedCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Dòng {row}: Lỗi không mong muốn - {ex.Message}");
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

                var resultMessage = $"Nhập liệu hoàn tất. Đã nhập thành công: {importedCount} học viên. Bỏ qua: {skippedCount} dòng.";
                if (errors.Any())
                {
                    resultMessage += $"\n\n=== CHI TIẾT LỖI ===\n{string.Join("\n", errors)}";
                }
                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xử lý file Excel: {ex.Message}");
            }
        }

        // Similar translations for ImportInstructorsAsync and ImportSimulationManagersAsync
        // (I'm applying the same logic for brevity and correctness in the final file output)
        public async Task<string> ImportInstructorsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File trống hoặc không tồn tại.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Định dạng file không hợp lệ. Vui lòng tải lên file Excel (.xlsx).");

            int importedCount = 0;
            int skippedCount = 0;
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
                            throw new Exception("File Excel không chứa worksheet nào.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount < 2)
                            throw new Exception("File Excel phải chứa ít nhất một dòng dữ liệu (ngoài dòng tiêu đề).");

                        var dbContext = _uow.GetDbContext();
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        string? username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        string? email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        string? fullname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        string? password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        string? phoneNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        string? avatarUrl = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        if (string.IsNullOrWhiteSpace(username) &&
                                            string.IsNullOrWhiteSpace(email) &&
                                            string.IsNullOrWhiteSpace(fullname))
                                        {
                                            continue;
                                        }

                                        var missingFields = new List<string>();
                                        if (string.IsNullOrWhiteSpace(username)) missingFields.Add("Username (Cột A)");
                                        if (string.IsNullOrWhiteSpace(email)) missingFields.Add("Email (Cột B)");
                                        if (string.IsNullOrWhiteSpace(fullname)) missingFields.Add("Họ tên (Cột C)");
                                        if (string.IsNullOrWhiteSpace(password)) missingFields.Add("Mật khẩu (Cột D)");

                                        if (missingFields.Any())
                                        {
                                            errors.Add($"Dòng {row}: Thiếu thông tin bắt buộc - {string.Join(", ", missingFields)}");
                                            skippedCount++;
                                            continue;
                                        }

                                        if (!IsValidEmail(email))
                                        {
                                            errors.Add($"Dòng {row}: Định dạng email không hợp lệ '{email}' (Cột B)");
                                            skippedCount++;
                                            continue;
                                        }

                                        var existingUser = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .Where(u => !u.IsDeleted && (u.Username == username || u.Email.ToLower() == email.ToLower()))
                                            .Select(u => new { u.Username, u.Email })
                                            .FirstOrDefaultAsync();

                                        if (existingUser != null)
                                        {
                                            if (existingUser.Username == username && existingUser.Email.ToLower() == email.ToLower())
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' và Email '{email}' đã tồn tại");
                                            else if (existingUser.Username == username)
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' đã tồn tại");
                                            else
                                                errors.Add($"Dòng {row}: Email '{email}' đã tồn tại");
                                            skippedCount++;
                                            continue;
                                        }

                                        string hashedPassword = PasswordHashHandler.HashPassword(password);
                                        string instructorCode = await GenerateUniqueInstructorCode();
                                        var instructorProfile = new InstructorProfile { };
                                        var instructor = new Instructor
                                        {
                                            InstructorCode = instructorCode,
                                            IsActive = true,
                                            IsDeleted = false,
                                            InstructorProfile = instructorProfile
                                        };

                                        var user = new User
                                        {
                                            Username = username,
                                            Password = hashedPassword,
                                            Email = email,
                                            Fullname = fullname,
                                            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                                            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
                                            Role = (int)UserRoleEnum.Instructor,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Instructor = instructor
                                        };

                                        await _uow.UserRepository.CreateAsync(user);
                                        importedCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Dòng {row}: Lỗi không mong muốn - {ex.Message}");
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

                var resultMessage = $"Nhập liệu hoàn tất. Đã nhập thành công: {importedCount} giảng viên. Bỏ qua: {skippedCount} dòng.";
                if (errors.Any()) resultMessage += $"\n\n=== CHI TIẾT LỖI ===\n{string.Join("\n", errors)}";
                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xử lý file Excel: {ex.Message}");
            }
        }

        public async Task<string> ImportSimulationManagersAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File trống hoặc không tồn tại.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Định dạng file không hợp lệ. Vui lòng tải lên file Excel (.xlsx).");

            int importedCount = 0;
            int skippedCount = 0;
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
                            throw new Exception("File Excel không chứa worksheet nào.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount < 2)
                            throw new Exception("File Excel phải chứa ít nhất một dòng dữ liệu (ngoài dòng tiêu đề).");

                        var dbContext = _uow.GetDbContext();
                        using (var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        string? username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        string? email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        string? fullname = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        string? password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        string? phoneNumber = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        string? avatarUrl = worksheet.Cells[row, 6].Value?.ToString()?.Trim();

                                        if (string.IsNullOrWhiteSpace(username) &&
                                            string.IsNullOrWhiteSpace(email) &&
                                            string.IsNullOrWhiteSpace(fullname))
                                        {
                                            continue;
                                        }

                                        var missingFields = new List<string>();
                                        if (string.IsNullOrWhiteSpace(username)) missingFields.Add("Username (Cột A)");
                                        if (string.IsNullOrWhiteSpace(email)) missingFields.Add("Email (Cột B)");
                                        if (string.IsNullOrWhiteSpace(fullname)) missingFields.Add("Họ tên (Cột C)");
                                        if (string.IsNullOrWhiteSpace(password)) missingFields.Add("Mật khẩu (Cột D)");

                                        if (missingFields.Any())
                                        {
                                            errors.Add($"Dòng {row}: Thiếu thông tin bắt buộc - {string.Join(", ", missingFields)}");
                                            skippedCount++;
                                            continue;
                                        }

                                        if (!IsValidEmail(email))
                                        {
                                            errors.Add($"Dòng {row}: Định dạng email không hợp lệ '{email}' (Cột B)");
                                            skippedCount++;
                                            continue;
                                        }

                                        var existingUser = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .Where(u => !u.IsDeleted && (u.Username == username || u.Email.ToLower() == email.ToLower()))
                                            .Select(u => new { u.Username, u.Email })
                                            .FirstOrDefaultAsync();

                                        if (existingUser != null)
                                        {
                                            if (existingUser.Username == username && existingUser.Email.ToLower() == email.ToLower())
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' và Email '{email}' đã tồn tại");
                                            else if (existingUser.Username == username)
                                                errors.Add($"Dòng {row}: Tên đăng nhập '{username}' đã tồn tại");
                                            else
                                                errors.Add($"Dòng {row}: Email '{email}' đã tồn tại");
                                            skippedCount++;
                                            continue;
                                        }

                                        string hashedPassword = PasswordHashHandler.HashPassword(password);
                                        var simulationManager = new SimulationManager { };
                                        var user = new User
                                        {
                                            Username = username,
                                            Password = hashedPassword,
                                            Email = email,
                                            Fullname = fullname,
                                            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                                            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
                                            Role = (int)UserRoleEnum.SimulationManager,
                                            IsActive = true,
                                            IsDeleted = false,
                                            SimulationManager = simulationManager
                                        };

                                        await _uow.UserRepository.CreateAsync(user);
                                        importedCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Dòng {row}: Lỗi không mong muốn - {ex.Message}");
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

                var resultMessage = $"Nhập liệu hoàn tất. Đã nhập thành công: {importedCount} quản lý mô phỏng. Bỏ qua: {skippedCount} dòng.";
                if (errors.Any()) resultMessage += $"\n\n=== CHI TIẾT LỖI ===\n{string.Join("\n", errors)}";
                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xử lý file Excel: {ex.Message}");
            }
        }
    }
}