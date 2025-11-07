namespace Lssctc.ProgramManagement.BrandModel.DTOs
{
    public class BrandModelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CreateBrandModelDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? CountryOfOrigin { get; set; }
    }

    public class UpdateBrandModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool? IsActive { get; set; }
    }
}
