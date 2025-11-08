namespace Lssctc.ProgramManagement.Accounts.Profiles.Dtos
{
    public class TraineeProfileDto
    {
        public int Id { get; set; }
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
