using System.ComponentModel.DataAnnotations;

namespace Lssctc.SimulationManagement.StepActions.Dtos
{
    public class StepActionDto
    {
        public int StepActionId { get; set; }

        public int StepId { get; set; }

        public int ActionId { get; set; }

        public string? StepActionName { get; set; }

        public string? StepActionDescription { get; set; }
    }

    public class CreateStepActionDto
    {
        public int StepId { get; set; }
        public int ActionId { get; set; }
        public string? StepActionName { get; set; }
        public string? StepActionDescription { get; set; }
    }

    public class UpdateStepActionDto
    {
        public int StepId { get; set; }
        public int ActionId { get; set; }
        public string? StepActionName { get; set; }
        public string? StepActionDescription { get; set; }
    }
}
