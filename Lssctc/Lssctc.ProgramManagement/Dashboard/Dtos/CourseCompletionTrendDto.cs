namespace Lssctc.ProgramManagement.Dashboard.Dtos
{
    public class CourseCompletionTrendDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = null!;
        public int CompletedCount { get; set; }
    }
}
