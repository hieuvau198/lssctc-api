using Lssctc.ProgramManagement.Accounts.Helpers;
using Lssctc.ProgramManagement.Accounts.Managemetns.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Accounts.Managemetns.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _uow;
        private static readonly Random _random = new Random(); // For trainee code generation

        public UsersService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        #region Users

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _uow.UserRepository
                .GetAllAsQueryable()
                .Where(u => !u.IsDeleted)
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _uow.UserRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return null;

            return MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllTraineesAsync()
        {
            return await GetUsersByRole(UserRoleEnum.Trainee);
        }

        public async Task<IEnumerable<UserDto>> GetAllInstructorsAsync()
        {
            return await GetUsersByRole(UserRoleEnum.Instructor);
        }

        public async Task<IEnumerable<UserDto>> GetAllSimulationManagersAsync()
        {
            return await GetUsersByRole(UserRoleEnum.SimulationManager);
        }

        // --- MODIFIED METHOD: CreateUserAsync is now CreateTraineeAccountAsync ---
        public async Task<UserDto> CreateTraineeAccountAsync(CreateUserDto dto)
        {
            // 1. Check for unique Username and Email
            bool exists = await _uow.UserRepository
                .GetAllAsQueryable()
                .AnyAsync(u =>
                (u.Username == dto.Username
                || u.Email.ToLower() == dto.Email.ToLower())
                && !u.IsDeleted);
            if (exists)
                throw new Exception("Username or Email already exists.");

            // 2. Hash the password
            string hashedPassword = PasswordHashHandler.HashPassword(dto.Password);

            // 3. Generate a unique trainee code
            string traineeCode = await GenerateUniqueTraineeCode();

            // 4. Create the linked entities (Profile -> Trainee -> User)
            // EF Core will link these using the shared primary key

            var traineeProfile = new TraineeProfile
            {
                // Set any default non-nullable values here if needed
            };

            var trainee = new Trainee
            {
                TraineeCode = traineeCode,
                IsActive = true,
                IsDeleted = false,
                TraineeProfile = traineeProfile // Link to the profile
            };

            var user = new User
            {
                Username = dto.Username,
                Password = hashedPassword,
                Email = dto.Email,
                Fullname = dto.Fullname,
                PhoneNumber = dto.PhoneNumber,
                AvatarUrl = dto.AvatarUrl,
                Role = (int)UserRoleEnum.Trainee, // Force role to Trainee
                IsActive = true,
                IsDeleted = false,
                Trainee = trainee // Link to the trainee
            };

            // 5. Create the user (EF Core will cascade-create the Trainee and TraineeProfile)
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

        // --- NEW HELPER METHOD ---
        private async Task<string> GenerateUniqueTraineeCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string traineeCode;
            bool isUnique;

            do
            {
                // Generate 6 random characters
                var randomPart = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                traineeCode = "CS" + randomPart;

                // Check for uniqueness in the Trainee table
                // (Assumes _uow has a TraineeRepository)
                isUnique = !await _uow.TraineeRepository
                    .GetAllAsQueryable()
                    .AnyAsync(t => t.TraineeCode == traineeCode);

            } while (!isUnique);

            return traineeCode;
        }

        #endregion
    }
}