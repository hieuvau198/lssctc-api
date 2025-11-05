namespace Lssctc.ProgramManagement.ClassManage.Progresses.Dtos
{
    public class SectionRecordDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public bool IsCompleted { get; set; }
        public decimal? Progress { get; set; }
        public ICollection<ActivityRecordDto> ActivityRecords { get; set; } = new List<ActivityRecordDto>();
    }
}
