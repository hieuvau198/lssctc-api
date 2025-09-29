namespace Lssctc.SimulationManagement.PracticeSteps.Dtos
{
    public class PracticeStepDto
    {
        public int Id { get; set; }
        public int PracticeId { get; set; }
        public string StepName { get; set; } = null!;
        public string? StepDescription { get; set; }
        public string? ExpectedResult { get; set; }
        public int StepOrder { get; set; }
    }

    public class CreatePracticeStepDto
    {
        public int PracticeId { get; set; }
        public string StepName { get; set; } = null!;
        public string? StepDescription { get; set; }
        public string? ExpectedResult { get; set; }
        public int StepOrder { get; set; }
    }

    public class UpdatePracticeStepDto
    {
        public string? StepName { get; set; }
        public string? StepDescription { get; set; }
        public string? ExpectedResult { get; set; }
        public int? StepOrder { get; set; }
    }

    public class PracticeStepComponentDto
    {
        public int Id { get; set; }
        public int PracticeStepId { get; set; }
        public int ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public string? ComponentDescription { get; set; }
        public int DisplayOrder { get; set; }
        public string? ComponentImageUrl { get; set; }

    }
}
