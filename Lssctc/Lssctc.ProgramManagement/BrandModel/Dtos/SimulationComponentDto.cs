namespace Lssctc.ProgramManagement.BrandModel.DTOs
{
    public class SimulationComponentDto
    {
        public int Id { get; set; }
        public int BrandModelId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
