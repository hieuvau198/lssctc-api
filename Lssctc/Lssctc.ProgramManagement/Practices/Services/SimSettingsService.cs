using Lssctc.ProgramManagement.Common.Services;
using Lssctc.ProgramManagement.Practices.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.Practices.Services
{
    public class SimSettingsService : ISimSettingsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private const int DEFAULT_SETTING_ID = 1;

        public SimSettingsService(IUnitOfWork uow, IFirebaseStorageService firebaseStorageService)
        {
            _uow = uow;
            _firebaseStorageService = firebaseStorageService;
        }

        public async Task<SimSettingDto> GetSimSettingAsync()
        {
            var setting = await _uow.SimSettingRepository.GetByIdAsync(DEFAULT_SETTING_ID);

            if (setting == null)
            {
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

        public async Task<SimSettingDto> UploadSimulationSourceAsync(Stream fileStream, string fileName, string contentType)
        {
            var setting = await _uow.SimSettingRepository.GetByIdAsync(DEFAULT_SETTING_ID);

            if (setting == null)
            {
                throw new KeyNotFoundException($"Default SimSetting with ID {DEFAULT_SETTING_ID} not found.");
            }

            // 1. Delete old file from Firebase if it exists to save space
            if (!string.IsNullOrEmpty(setting.SourceUrl))
            {
                await _firebaseStorageService.DeleteFileAsync(setting.SourceUrl);
            }

            // 2. Upload new file
            // Prefixing with timestamp to ensure uniqueness and avoid caching issues
            var uniqueFileName = $"SimSource_{DateTime.UtcNow.Ticks}_{fileName}";
            var newUrl = await _firebaseStorageService.UploadFileAsync(fileStream, uniqueFileName, contentType);

            // 3. Update the entity
            setting.SourceUrl = newUrl;

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