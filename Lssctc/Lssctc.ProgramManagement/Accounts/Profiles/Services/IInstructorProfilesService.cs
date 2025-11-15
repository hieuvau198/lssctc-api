using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Services
{
    public interface IInstructorProfilesService
    {
        Task<InstructorProfileDto?> GetInstructorProfile(int instructorId);
        Task<InstructorProfileWithUserDto?> GetInstructorProfileByUserId(int userId);
        Task<bool> UpdateInstructorProfile(int instructorId, UpdateInstructorProfileDto dto);
    }
}
