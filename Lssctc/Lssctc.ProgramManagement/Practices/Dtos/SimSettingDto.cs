namespace Lssctc.ProgramManagement.Practices.Dtos
{
    public class SimSettingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? SettingCode { get; set; }
        public string? SourceUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
