using Lssctc.ProgramManagement.Accounts.Users.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Accounts.Users.Services
{
    public interface IUsersService
    {
        #region Users
        // Change all get all methods to paged result, using the same page result as other services
        Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<PagedResult<UserDto>> GetAllTraineesAsync(int pageNumber, int pageSize);
        Task<PagedResult<UserDto>> GetAllInstructorsAsync(int pageNumber, int pageSize);
        Task<PagedResult<UserDto>> GetAllSimulationManagersAsync(int pageNumber, int pageSize);
        Task<UserDto> CreateTraineeAccountAsync(CreateUserDto dto);
        Task<UserDto> CreateInstructorAccountAsync(CreateUserDto dto);
        Task<UserDto> CreateSimulationManagerAccountAsync(CreateUserDto dto);
        Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> IsEmailExistsAsync(string email);
        #endregion

        #region Profiles
        Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto dto);
        #endregion
    }
}
