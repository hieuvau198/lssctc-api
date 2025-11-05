namespace Lssctc.ProgramManagement.ClassManage.Progresses.Dtos
{
    public class ActivityRecordDto
    {
        public int Id { get; set; }
        public int? ActivityId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
