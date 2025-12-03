namespace Lssctc.ProgramManagement.Dashboard.Dtos
{
    public class MonthlyPracticeCompletionDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = null!;
        public int CompletedCount { get; set; }
        public int NotCompletedCount { get; set; }
    }
}
