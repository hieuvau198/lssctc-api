namespace Lssctc.ProgramManagement.SectionQuizzes.DTOs
{
    public class SectionQuizDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int SectionPartitionId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateSectionQuizDto
    {
        public int QuizId { get; set; }
        public int SectionPartitionId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateSectionQuizDto
    {
        public int? QuizId { get; set; }
        public int? SectionPartitionId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
