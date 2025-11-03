namespace Lssctc.ProgramManagement.Classes.Dtos
{
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Mobile Crane Training Class";
        public int? Capacity { get; set; }
        public int ProgramId { get; set; }
        public int CourseId { get; set; }
        public string ClassCode { get; set; } = "MCTC-001";
        public string Description { get; set; } = "Mobile Crane Training Class";
        public string Status { get; set; } = "Cancelled";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
