namespace Lssctc.ProgramManagement.SimulationComponents.DTOs
{
    public class SimulationComponentDto
    {
        public int Id { get; set; }
        public int BrandModelId { get; set; }
        public string BrandModelName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CreateSimulationComponentDto
    {
        public int BrandModelId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateSimulationComponentDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
