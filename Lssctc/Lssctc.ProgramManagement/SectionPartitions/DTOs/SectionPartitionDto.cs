namespace Lssctc.ProgramManagement.SectionPartitions.DTOs
{
    public class SectionPartitionDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public int PartitionTypeId { get; set; }
        public string? Description { get; set; }
    }

    public class CreateSectionPartitionDto
    {
        public int SectionId { get; set; }
        public string? Name { get; set; }
        public int PartitionTypeId { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateSectionPartitionDto
    {
        public string? Name { get; set; }
        public int? PartitionTypeId { get; set; }
        public string? Description { get; set; }
    }
}
