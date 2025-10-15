namespace Lssctc.ProgramManagement.Learnings.LearningsClasses.Dtos
{
    public class LearningsClassDto
    {
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public DateTime? ClassStartDate { get; set; }
        public DateTime? ClassEndDate { get; set; }
        public int? ClassCapacity { get; set; }
        public string? ClassCode { get; set; }
        public int ProgramCourseId { get; set; }
        public int CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public int CourseDurationHours { get; set; }
        public string? ClassStatus { get; set; }
        public decimal? ClassProgress { get; set; }
    }
}
