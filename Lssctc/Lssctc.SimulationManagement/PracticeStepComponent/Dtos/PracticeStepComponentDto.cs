namespace Lssctc.SimulationManagement.PracticeStepComponent.Dtos
{
    public class PracticeStepComponentDto
    {
        public int Id { get; set; }
        public int StepId { get; set; }
        public int ComponentId { get; set; }
        public int ComponentOrder { get; set; }
    }

    public class CreatePracticeStepComponentDto
    {
        public int StepId { get; set; }
        public int ComponentId { get; set; }
        public int? ComponentOrder { get; set; } // null => tự max+1 trong Step
    }

    public class UpdatePracticeStepComponentDto
    {
        public int? ComponentId { get; set; }
        public int? ComponentOrder { get; set; }
    }
}
