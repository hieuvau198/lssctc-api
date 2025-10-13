namespace Lssctc.SimulationManagement.SimActions.Dtos
{
    public class SimActionDto
    {
        public int ActionId { get; set; }

        public string ActionName { get; set; } = null!;

        public string? ActionDescription { get; set; }

        public string? ActionKey { get; set; }

        public bool? IsActive { get; set; }
    }
    public class CreateSimActionDto
    {
        public string ActionName { get; set; } = null!;
        public string? ActionDescription { get; set; }
        public string? ActionKey { get; set; }
        public bool? IsActive { get; set; }
    }
    public class UpdateSimActionDto
    {
        public string? ActionName { get; set; }
        public string? ActionDescription { get; set; }
        public string? ActionKey { get; set; }
        public bool? IsActive { get; set; }
    }
}
