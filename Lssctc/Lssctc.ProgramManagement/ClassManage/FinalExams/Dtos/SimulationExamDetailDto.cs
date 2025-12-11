namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos
{
    public class SimulationExamDetailDto : FinalExamPartialDto
    {
        public PracticeInfoDetailDto? PracticeInfo { get; set; }
    }

    public class PracticeInfoDetailDto
    {
        public int Id { get; set; }
        public string PracticeName { get; set; } = null!;
        public string PracticeCode { get; set; } = null!;
        public string? Description { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? TopicName { get; set; }
    }
    public class ClassSimulationResultDto
    {
        public int TraineeId { get; set; }
        public string? TraineeName { get; set; }
        public string? TraineeCode { get; set; }
        public string? AvatarUrl { get; set; } // Nếu user có avatar

        public SimulationExamDetailDto? SimulationResult { get; set; }
    }
}
