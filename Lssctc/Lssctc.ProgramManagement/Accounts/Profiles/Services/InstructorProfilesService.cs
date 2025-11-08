using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;
using Lssctc.Share.Interfaces;

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
