namespace Lssctc.ProgramManagement.Dashboard.Dtos
{
    public class PracticeAverageScoreDto
    {
        public string PracticeName { get; set; } = string.Empty;
        public string PracticeCode { get; set; } = string.Empty;
        public decimal? AverageScore { get; set; }
        public int TotalAttempts { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }
}
