namespace Lssctc.ProgramManagement.Practices.Dtos
{
    public class TraineeTaskDto
    {
        public int TaskId { get; set; }
        public int PracticeAttemptTaskId { get; set; }
        public string TaskName { get; set; } = null!;
        public string? TaskCode { get; set; } // Added
        public string? TaskDescription { get; set; }
        public string? ExpectedResult { get; set; }
        public bool IsPass { get; set; }
        public decimal? Score { get; set; }
        public string? Description { get; set; }
    }
}
