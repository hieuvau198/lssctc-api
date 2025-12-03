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

        public async Task<PagedResult<UserDto>> GetAllTraineesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.Trainee)
                .Select(u => MapToDto(u));
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<UserDto>> GetAllInstructorsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.Instructor)
                .Select(u => MapToDto(u));
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<UserDto>> GetAllSimulationManagersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var query = _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted && u.Role == (int)UserRoleEnum.SimulationManager)
                .Select(u => MapToDto(u));
            return await query.ToPagedResultAsync(pageNumber, pageSize);
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
                throw new Exception("Username or Email already exists.");

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
                throw new Exception("Username or Email already exists.");

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
                throw new Exception("Username or Email already exists.");

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
                throw new Exception("User not found.");

            user.Fullname = dto.Fullname ?? user.Fullname;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
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
                throw new Exception("User not found.");

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
                throw new Exception("User not found.");

            if (!PasswordHashHandler.VerifyPassword(dto.CurrentPassword, user.Password))
                throw new Exception("Current password is incorrect.");

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

        #endregion

        public async Task<string> ImportTraineesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty or null.");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid file format. Please upload an Excel file (.xlsx).");

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
                            throw new Exception("Excel file does not contain any worksheets.");

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
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

                                        // Check for duplicates in database (deduplication logic)
                                        bool exists = await _uow.UserRepository
                                            .GetAllAsQueryable()
                                            .AnyAsync(u => 
                                                (u.Username == username || u.Email.ToLower() == email.ToLower()) 
                                                && !u.IsDeleted);

                                        if (exists)
                                        {
                                            // Skip duplicate silently as per requirements
                                            skippedCount++;
                                            continue;
                                        }

                                        // Hash password
                                        string hashedPassword = PasswordHashHandler.HashPassword(password);

                                        // Generate unique trainee code
                                        string traineeCode = await GenerateUniqueTraineeCode();

                                        // Create Trainee Profile
                                        var traineeProfile = new TraineeProfile
                                        {
                                        };

                                        // Create Trainee
                                        var trainee = new Trainee
                                        {
                                            TraineeCode = traineeCode,
                                            IsActive = true,
                                            IsDeleted = false,
                                            TraineeProfile = traineeProfile
                                        };

                                        // Create User with Trainee role
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
                                        errors.Add($"Row {row}: {ex.Message}");
                                        skippedCount++;
                                    }
                                }

                                // Save all changes within transaction
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

                // Build result message
                var resultMessage = $"Import completed successfully. Imported {importedCount} trainees. Skipped {skippedCount} duplicates/errors.";
                
                if (errors.Any())
                {
                    resultMessage += $" Errors: {string.Join("; ", errors.Take(10))}"; // Limit error messages
                    if (errors.Count > 10)
                    {
                        resultMessage += $" (and {errors.Count - 10} more errors...)";
                    }
                }

                return resultMessage;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing Excel file: {ex.Message}");
            }
        }
    }
}