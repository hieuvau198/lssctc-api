namespace Lssctc.ProgramManagement.Dashboard.Dtos
{
    public class ClassAverageScoreDto
    {
        public string ClassName { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public decimal? AverageScore { get; set; }
    }
}
