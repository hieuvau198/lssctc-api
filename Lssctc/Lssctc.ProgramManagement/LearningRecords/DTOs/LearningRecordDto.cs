namespace Lssctc.ProgramManagement.LearningRecords.DTOs
{
    public class LearningRecordDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public int TrainingProgressId { get; set; }
        public string? SectionName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTraineeAttended { get; set; }
        public decimal? Progress { get; set; }
    }

    public class CreateLearningRecordDto
    {
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public int TrainingProgressId { get; set; }
        public string? SectionName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTraineeAttended { get; set; }
        public decimal? Progress { get; set; }
    }

    public class UpdateLearningRecordDto
    {
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public int TrainingProgressId { get; set; }
        public string? SectionName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTraineeAttended { get; set; }
        public decimal? Progress { get; set; }
    }
}
