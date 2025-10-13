namespace Lssctc.SimulationManagement.TraineePractices.Dtos
{
    public class TraineeStepDto
    {
        public int StepId { get; set; } // PracticeStep
        public string? StepName { get; set; } // PracticeStep
        public string? StepDescription { get; set; } // PracticeStep
        public string? ExpectedResult { get; set; } // PracticeStep
        public int StepOrder { get; set; } // PracticeStep
        public bool? IsCompleted { get; set; } // SectionPracticeAttemptStep
        public int PracticeId { get; set; } // Practice
        public int ActionId { get; set; } // SimAction
        public string? ActionName { get; set; } // SimAction
        public string? ActionDescription { get; set; } // SimAction
        public string? ActionKey { get; set; } // SimAction
        public int ComponentId { get; set; } // SimulationComponent
        public string? ComponentName { get; set; } // SimulationComponent
        public string? ComponentDescription { get; set; } // SimulationComponent
        public string? ComponentImageUrl { get; set; } // SimulationComponent
    }
}
