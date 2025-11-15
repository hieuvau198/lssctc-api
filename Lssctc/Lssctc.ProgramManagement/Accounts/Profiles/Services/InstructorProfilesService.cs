using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;
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
