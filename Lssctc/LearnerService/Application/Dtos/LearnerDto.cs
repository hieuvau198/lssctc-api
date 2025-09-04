namespace LearnerService.Application.Dtos
{
    public class LearnerDto
    {
        public int UserId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? EnrollmentStatus { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CreateLearnerDto
    {
        public int UserId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }

    public class UpdateLearnerDto
    {
        public DateOnly? DateOfBirth { get; set; }
        public string? EnrollmentStatus { get; set; }
    }

    public class LearnerQueryParameters
    {
        public string? SearchTerm { get; set; }
        public string? EnrollmentStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
