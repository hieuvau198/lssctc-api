using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.ProgramManagement.Accounts.Users.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    }
}