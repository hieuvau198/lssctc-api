using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class CreateTraineeProfileDto
    {
        [StringLength(100, ErrorMessage = "Driver license number cannot exceed 100 characters.")]
        public string? DriverLicenseNumber { get; set; }

        [StringLength(50, ErrorMessage = "Driver license level cannot exceed 50 characters.")]
        public string? DriverLicenseLevel { get; set; }

        public DateTime? DriverLicenseIssuedDate { get; set; }

        public DateTime? DriverLicenseValidStartDate { get; set; }

        public DateTime? DriverLicenseValidEndDate { get; set; }

        [Url(ErrorMessage = "Invalid URL format for driver license image.")]
        public string? DriverLicenseImageUrl { get; set; }

        [StringLength(255, ErrorMessage = "Education level cannot exceed 255 characters.")]
        public string? EducationLevel { get; set; }

        [Url(ErrorMessage = "Invalid URL format for education image.")]
        public string? EducationImageUrl { get; set; }

        [StringLength(20, ErrorMessage = "Citizen card ID cannot exceed 20 characters.")]
        public string? CitizenCardId { get; set; }

        public DateOnly? CitizenCardIssuedDate { get; set; }

        [StringLength(255, ErrorMessage = "Citizen card place of issue cannot exceed 255 characters.")]
        public string? CitizenCardPlaceOfIssue { get; set; }

        [Url(ErrorMessage = "Invalid URL format for citizen card image.")]
        public string? CitizenCardImageUrl { get; set; }
    }
}
