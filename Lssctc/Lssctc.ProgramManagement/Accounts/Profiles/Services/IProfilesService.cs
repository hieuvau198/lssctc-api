using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Services
{
    public interface IProfilesService
    {
        Task<TraineeProfileDto?> GetTraineeProfile(int traineeId);
        Task<TraineeProfileWithUserDto?> GetTraineeProfileByUserId(int userId);
        Task<TraineeProfileDto> CreateTraineeProfile(int traineeId, CreateTraineeProfileDto dto);
        Task<bool> UpdateTraineeProfile(int traineeId, UpdateTraineeProfileDto dto);
        Task<bool> DeleteTraineeProfile(int traineeId);
        Task<TraineeProfileWithUserDto> UpdateUserAndTraineeProfile(int userId, UpdateUserAndTraineeProfileDto dto);
    }
}
