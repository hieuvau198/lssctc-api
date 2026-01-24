using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class SimSettingsService : ISimSettingsService
    {
        private readonly IUnitOfWork _uow;
        private const int DEFAULT_SETTING_ID = 1;

        public SimSettingsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<SimSettingDto> GetSimSettingAsync()
        {
            var setting = await _uow.SimSettingRepository.GetByIdAsync(DEFAULT_SETTING_ID);

            if (setting == null)
            {
                // Optional: Create default if it doesn't exist, or return null/throw
                throw new KeyNotFoundException($"Default SimSetting with ID {DEFAULT_SETTING_ID} not found.");
            }

            return MapToDto(setting);
        }

        public async Task<SimSettingDto> UpdateSimSettingAsync(SimSettingDto dto)
        {
            var setting = await _uow.SimSettingRepository.GetByIdAsync(DEFAULT_SETTING_ID);

            if (setting == null)
            {
                throw new KeyNotFoundException($"Default SimSetting with ID {DEFAULT_SETTING_ID} not found.");
            }

            // Update properties
            setting.Name = dto.Name;
            setting.Description = dto.Description;
            setting.ImageUrl = dto.ImageUrl;
            setting.SettingCode = dto.SettingCode;
            setting.SourceUrl = dto.SourceUrl;

            if (dto.IsActive.HasValue)
            {
                setting.IsActive = dto.IsActive;
            }

            await _uow.SimSettingRepository.UpdateAsync(setting);
            await _uow.SaveChangesAsync();

            return MapToDto(setting);
        }

        private static SimSettingDto MapToDto(SimSetting entity)
        {
            return new SimSettingDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                SettingCode = entity.SettingCode,
                SourceUrl = entity.SourceUrl,
                IsActive = entity.IsActive
            };
        }
    }
}
