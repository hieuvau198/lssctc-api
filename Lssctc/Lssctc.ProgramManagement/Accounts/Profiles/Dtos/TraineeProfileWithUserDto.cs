namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class TraineeProfileWithUserDto
    {
        // User Information
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Fullname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Role { get; set; }

        // Trainee Information
        public string TraineeCode { get; set; } = null!;
        public bool? IsTraineeActive { get; set; }

        // Profile Information
        public string? DriverLicenseNumber { get; set; }
        public string? DriverLicenseLevel { get; set; }
        public DateTime? DriverLicenseIssuedDate { get; set; }
        public DateTime? DriverLicenseValidStartDate { get; set; }
        public DateTime? DriverLicenseValidEndDate { get; set; }
        public string? DriverLicenseImageUrl { get; set; }
        public string? EducationLevel { get; set; }
        public string? EducationImageUrl { get; set; }
        public string? CitizenCardId { get; set; }
        public DateOnly? CitizenCardIssuedDate { get; set; }
        public string? CitizenCardPlaceOfIssue { get; set; }
        public string? CitizenCardImageUrl { get; set; }
    }
}
