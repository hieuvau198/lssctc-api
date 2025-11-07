namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Dtos
{
    public class PracticeAttemptDto
    {
        public int Id { get; set; }
        public int ActivityRecordId { get; set; }
        public int? PracticeId { get; set; }
        public decimal? Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public string? AttemptStatus { get; set; }
        public string? Description { get; set; }
        public bool? IsPass { get; set; }
        public bool IsCurrent { get; set; }
        public List<PracticeAttemptTaskDto> PracticeAttemptTasks { get; set; } = new List<PracticeAttemptTaskDto>();
    }

    public class PracticeAttemptTaskDto
    {
        public int Id { get; set; }
        public int PracticeAttemptId { get; set; }
        public int? TaskId { get; set; }
        public decimal? Score { get; set; }
        public string? Description { get; set; }
        public bool? IsPass { get; set; }
    }
}
