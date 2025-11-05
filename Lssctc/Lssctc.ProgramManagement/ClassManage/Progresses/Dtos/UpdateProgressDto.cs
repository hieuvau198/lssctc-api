namespace Lssctc.ProgramManagement.ClassManage.Progresses.Dtos
{
    public class UpdateProgressDto
    {
        public decimal? TheoryScore { get; set; }
        public decimal? PracticalScore { get; set; }
        public decimal? FinalScore { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
