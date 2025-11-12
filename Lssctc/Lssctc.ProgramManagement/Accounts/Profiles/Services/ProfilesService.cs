using Lssctc.ProgramManagement.Accounts.Profiles.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Services
{
    public class ProfilesService : IProfilesService
    {
        private readonly IUnitOfWork _uow;

        public ProfilesService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<TraineeProfileDto?> GetTraineeProfile(int traineeId)
        {
            var traineeProfile = await _uow.TraineeProfileRepository.GetByIdAsync(traineeId);
            if (traineeProfile == null)
                return null;

            return new TraineeProfileDto
            {
                Id = traineeProfile.Id,
                DriverLicenseNumber = traineeProfile.DriverLicenseNumber,
                DriverLicenseLevel = traineeProfile.DriverLicenseLevel,
                DriverLicenseIssuedDate = traineeProfile.DriverLicenseIssuedDate,
                DriverLicenseValidStartDate = traineeProfile.DriverLicenseValidStartDate,
                DriverLicenseValidEndDate = traineeProfile.DriverLicenseValidEndDate,
                DriverLicenseImageUrl = traineeProfile.DriverLicenseImageUrl,
                EducationLevel = traineeProfile.EducationLevel,
                EducationImageUrl = traineeProfile.EducationImageUrl,
                CitizenCardId = traineeProfile.CitizenCardId,
                CitizenCardIssuedDate = traineeProfile.CitizenCardIssuedDate,
                CitizenCardPlaceOfIssue = traineeProfile.CitizenCardPlaceOfIssue,
                CitizenCardImageUrl = traineeProfile.CitizenCardImageUrl
            };
        }

        public async Task<TraineeProfileDto> CreateTraineeProfile(int traineeId, CreateTraineeProfileDto dto)
        {
            var traineeExists = await _uow.TraineeRepository.GetByIdAsync(traineeId);
            if (traineeExists == null)
                throw new Exception("Trainee not found.");

            var profileExists = await _uow.TraineeProfileRepository.GetByIdAsync(traineeId);
            if (profileExists != null)
                throw new Exception("Trainee profile already exists.");

            var traineeProfile = new TraineeProfile
            {
                Id = traineeId,
                DriverLicenseNumber = dto.DriverLicenseNumber,
                DriverLicenseLevel = dto.DriverLicenseLevel,
                DriverLicenseIssuedDate = dto.DriverLicenseIssuedDate,
                DriverLicenseValidStartDate = dto.DriverLicenseValidStartDate,
                DriverLicenseValidEndDate = dto.DriverLicenseValidEndDate,
                DriverLicenseImageUrl = dto.DriverLicenseImageUrl,
                EducationLevel = dto.EducationLevel,
                EducationImageUrl = dto.EducationImageUrl,
                CitizenCardId = dto.CitizenCardId,
                CitizenCardIssuedDate = dto.CitizenCardIssuedDate,
                CitizenCardPlaceOfIssue = dto.CitizenCardPlaceOfIssue,
                CitizenCardImageUrl = dto.CitizenCardImageUrl
            };

            await _uow.TraineeProfileRepository.CreateAsync(traineeProfile);
            await _uow.SaveChangesAsync();

            return new TraineeProfileDto
            {
                Id = traineeProfile.Id,
                DriverLicenseNumber = traineeProfile.DriverLicenseNumber,
                DriverLicenseLevel = traineeProfile.DriverLicenseLevel,
                DriverLicenseIssuedDate = traineeProfile.DriverLicenseIssuedDate,
                DriverLicenseValidStartDate = traineeProfile.DriverLicenseValidStartDate,
                DriverLicenseValidEndDate = traineeProfile.DriverLicenseValidEndDate,
                DriverLicenseImageUrl = traineeProfile.DriverLicenseImageUrl,
                EducationLevel = traineeProfile.EducationLevel,
                EducationImageUrl = traineeProfile.EducationImageUrl,
                CitizenCardId = traineeProfile.CitizenCardId,
                CitizenCardIssuedDate = traineeProfile.CitizenCardIssuedDate,
                CitizenCardPlaceOfIssue = traineeProfile.CitizenCardPlaceOfIssue,
                CitizenCardImageUrl = traineeProfile.CitizenCardImageUrl
            };
        }

        public async Task<bool> UpdateTraineeProfile(int traineeId, UpdateTraineeProfileDto dto)
        {
            var traineeProfile = await _uow.TraineeProfileRepository.GetByIdAsync(traineeId);
            if (traineeProfile == null)
                throw new Exception("Trainee profile not found.");

            traineeProfile.DriverLicenseNumber = dto.DriverLicenseNumber ?? traineeProfile.DriverLicenseNumber;
            traineeProfile.DriverLicenseLevel = dto.DriverLicenseLevel ?? traineeProfile.DriverLicenseLevel;
            traineeProfile.DriverLicenseIssuedDate = dto.DriverLicenseIssuedDate ?? traineeProfile.DriverLicenseIssuedDate;
            traineeProfile.DriverLicenseValidStartDate = dto.DriverLicenseValidStartDate ?? traineeProfile.DriverLicenseValidStartDate;
            traineeProfile.DriverLicenseValidEndDate = dto.DriverLicenseValidEndDate ?? traineeProfile.DriverLicenseValidEndDate;
            traineeProfile.DriverLicenseImageUrl = dto.DriverLicenseImageUrl ?? traineeProfile.DriverLicenseImageUrl;
            traineeProfile.EducationLevel = dto.EducationLevel ?? traineeProfile.EducationLevel;
            traineeProfile.EducationImageUrl = dto.EducationImageUrl ?? traineeProfile.EducationImageUrl;
            traineeProfile.CitizenCardId = dto.CitizenCardId ?? traineeProfile.CitizenCardId;
            traineeProfile.CitizenCardIssuedDate = dto.CitizenCardIssuedDate ?? traineeProfile.CitizenCardIssuedDate;
            traineeProfile.CitizenCardPlaceOfIssue = dto.CitizenCardPlaceOfIssue ?? traineeProfile.CitizenCardPlaceOfIssue;
            traineeProfile.CitizenCardImageUrl = dto.CitizenCardImageUrl ?? traineeProfile.CitizenCardImageUrl;

            await _uow.TraineeProfileRepository.UpdateAsync(traineeProfile);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTraineeProfile(int traineeId)
        {
            var traineeProfile = await _uow.TraineeProfileRepository.GetByIdAsync(traineeId);
            if (traineeProfile == null)
                return false;

            await _uow.TraineeProfileRepository.DeleteAsync(traineeProfile);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
