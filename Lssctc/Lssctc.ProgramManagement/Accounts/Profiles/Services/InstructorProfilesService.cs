using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Services
{
    public class InstructorProfilesService : IInstructorProfilesService
    {
        private readonly IUnitOfWork _uow;

        public InstructorProfilesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<InstructorProfileDto?> GetInstructorProfile(int instructorId)
        {
            var instructorProfile = await _uow.InstructorProfileRepository.GetByIdAsync(instructorId);
            if (instructorProfile == null)
                return null;

            return new InstructorProfileDto
            {
                Id = instructorProfile.Id,
                ExperienceYears = instructorProfile.ExperienceYears,
                Biography = instructorProfile.Biography,
                ProfessionalProfileUrl = instructorProfile.ProfessionalProfileUrl,
                Specialization = instructorProfile.Specialization
            };
        }

        public async Task<InstructorProfileWithUserDto?> GetInstructorProfileByUserId(int userId)
        {
            // Get user with instructor and profile information using Include
            var user = await _uow.UserRepository.GetAllAsQueryable()
                .Include(u => u.Instructor)
                    .ThenInclude(i => i!.InstructorProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Instructor == null)
                return null;

            var instructor = user.Instructor;
            var profile = instructor.InstructorProfile;

            return new InstructorProfileWithUserDto
            {
                // User Information
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Fullname = user.Fullname,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,

                // Instructor Information
                InstructorCode = instructor.InstructorCode,
                HireDate = instructor.HireDate,
                IsInstructorActive = instructor.IsActive,

                // Profile Information (may be null if profile doesn't exist yet)
                ExperienceYears = profile?.ExperienceYears,
                Biography = profile?.Biography,
                ProfessionalProfileUrl = profile?.ProfessionalProfileUrl,
                Specialization = profile?.Specialization
            };
        }

        public async Task<InstructorProfileWithUserDto> UpdateUserAndInstructorProfile(int userId, UpdateUserAndInstructorProfileDto dto)
        {
            // Get user with instructor and profile information
            var user = await _uow.UserRepository.GetAllAsQueryable()
                .Include(u => u.Instructor)
                    .ThenInclude(i => i!.InstructorProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.Instructor == null)
                throw new Exception("Instructor not found for this user.");

            // Update User fields
            if (dto.Username != null)
                user.Username = dto.Username;

            // Kiểm tra email đã tồn tại chưa
            if (dto.Email != null && dto.Email != user.Email)
            {
                var existingUser = await _uow.UserRepository.GetAllAsQueryable()
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != userId);
                if (existingUser != null)
                    throw new Exception("Email already exists.");
            }

            if (dto.Email != null)
                user.Email = dto.Email;

            if (dto.Fullname != null)
                user.Fullname = dto.Fullname;

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.AvatarUrl != null)
                user.AvatarUrl = dto.AvatarUrl;

            await _uow.UserRepository.UpdateAsync(user);

            // Update Instructor fields
            var instructor = user.Instructor;

            if (dto.InstructorCode != null && dto.InstructorCode != instructor.InstructorCode)
            {
                var existingInstructor = await _uow.InstructorRepository.GetAllAsQueryable()
                    .FirstOrDefaultAsync(i => i.InstructorCode == dto.InstructorCode && i.Id != instructor.Id);
                if (existingInstructor != null)
                    throw new Exception("Instructor code already exists.");
            }

            if (dto.InstructorCode != null)
                instructor.InstructorCode = dto.InstructorCode;

            if (dto.HireDate != null)
                instructor.HireDate = dto.HireDate;

            if (dto.IsInstructorActive.HasValue)
                instructor.IsActive = dto.IsInstructorActive.Value;

            await _uow.InstructorRepository.UpdateAsync(instructor);

            // Update or Create Instructor Profile
            var instructorProfile = instructor.InstructorProfile;
            
            if (instructorProfile == null && (dto.ExperienceYears != null || dto.Biography != null || 
                dto.ProfessionalProfileUrl != null || dto.Specialization != null))
            {
                // Create new profile if it doesn't exist and any profile field is provided
                instructorProfile = new InstructorProfile
                {
                    Id = instructor.Id,
                    ExperienceYears = dto.ExperienceYears,
                    Biography = dto.Biography,
                    ProfessionalProfileUrl = dto.ProfessionalProfileUrl,
                    Specialization = dto.Specialization
                };
                await _uow.InstructorProfileRepository.CreateAsync(instructorProfile);
            }
            else if (instructorProfile != null)
            {
                // Update existing profile
                if (dto.ExperienceYears != null)
                    instructorProfile.ExperienceYears = dto.ExperienceYears;
                
                if (dto.Biography != null)
                    instructorProfile.Biography = dto.Biography;
                
                if (dto.ProfessionalProfileUrl != null)
                    instructorProfile.ProfessionalProfileUrl = dto.ProfessionalProfileUrl;
                
                if (dto.Specialization != null)
                    instructorProfile.Specialization = dto.Specialization;
                
                await _uow.InstructorProfileRepository.UpdateAsync(instructorProfile);
            }

            await _uow.SaveChangesAsync();

            // Reload the updated data
            var updatedUser = await _uow.UserRepository.GetAllAsQueryable()
                .Include(u => u.Instructor)
                    .ThenInclude(i => i!.InstructorProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var updatedInstructor = updatedUser!.Instructor!;
            var profile = updatedInstructor.InstructorProfile;

            return new InstructorProfileWithUserDto
            {
                // User Information
                UserId = updatedUser.Id,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                Fullname = updatedUser.Fullname,
                PhoneNumber = updatedUser.PhoneNumber,
                AvatarUrl = updatedUser.AvatarUrl,
                Role = updatedUser.Role,

                // Instructor Information
                InstructorCode = updatedInstructor.InstructorCode,
                HireDate = updatedInstructor.HireDate,
                IsInstructorActive = updatedInstructor.IsActive,

                // Profile Information
                ExperienceYears = profile?.ExperienceYears,
                Biography = profile?.Biography,
                ProfessionalProfileUrl = profile?.ProfessionalProfileUrl,
                Specialization = profile?.Specialization
            };
        }

        public async Task<bool> UpdateInstructorProfile(int instructorId, UpdateInstructorProfileDto dto)
        {
            // Validate instructorId
            if (instructorId <= 0)
                throw new ArgumentException("Instructor ID must be greater than 0.", nameof(instructorId));

            // Check if instructor exists
            var instructorExists = await _uow.InstructorRepository.GetByIdAsync(instructorId);
            if (instructorExists == null)
                throw new Exception("Instructor not found.");

            // Check if instructor profile exists
            var instructorProfile = await _uow.InstructorProfileRepository.GetByIdAsync(instructorId);
            if (instructorProfile == null)
                throw new Exception("Instructor profile not found.");

            // Validate at least one field is being updated
            if (dto.ExperienceYears == null && 
                dto.Biography == null && 
                dto.ProfessionalProfileUrl == null && 
                dto.Specialization == null)
            {
                throw new Exception("At least one field must be provided for update.");
            }

            // Update fields only if provided
            if (dto.ExperienceYears.HasValue)
            {
                instructorProfile.ExperienceYears = dto.ExperienceYears.Value;
            }

            if (dto.Biography != null)
            {
                instructorProfile.Biography = dto.Biography;
            }

            if (dto.ProfessionalProfileUrl != null)
            {
                instructorProfile.ProfessionalProfileUrl = dto.ProfessionalProfileUrl;
            }

            if (dto.Specialization != null)
            {
                instructorProfile.Specialization = dto.Specialization;
            }

            await _uow.InstructorProfileRepository.UpdateAsync(instructorProfile);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
