using Lssctc.ProgramManagement.Accounts.Managemetns.Dtos;

namespace Lssctc.ProgramManagement.Accounts.Managemetns.Services
{
    public interface IUsersService
    {
        #region Users
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllTraineesAsync();
        Task<IEnumerable<UserDto>> GetAllInstructorsAsync();
        Task<IEnumerable<UserDto>> GetAllSimulationManagersAsync();
        Task<UserDto> CreateTraineeAccountAsync(CreateUserDto dto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        #endregion

        #region Profiles
        Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto dto);
        #endregion
    }
}
