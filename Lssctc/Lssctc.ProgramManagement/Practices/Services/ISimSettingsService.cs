using Lssctc.ProgramManagement.Practices.Dtos;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public interface ISimSettingsService
    {
        Task<SimSettingDto> GetSimSettingAsync();
        Task<SimSettingDto> UpdateSimSettingAsync(SimSettingDto dto);
    }
}
