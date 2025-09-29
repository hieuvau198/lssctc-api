namespace Lssctc.SimulationManagement.PracticeStepComponents.Dtos
{
    public class PracticeStepComponentDto
    {
        public int Id { get; set; }
        public int PracticeStepId { get; set; }
        public int SimulationComponentId { get; set; }
        public string? SimulationComponentName { get; set; }
        public string? Description { get; set; }
        public int ComponentOrder { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CreatePracticeStepComponentDto
    {
        public int PracticeStepId { get; set; }
        public int SimulationComponentId { get; set; }
        public int ComponentOrder { get; set; }
    }

    public class UpdatePracticeStepComponentDto
    {
        public int? ComponentOrder { get; set; }
    }
}
