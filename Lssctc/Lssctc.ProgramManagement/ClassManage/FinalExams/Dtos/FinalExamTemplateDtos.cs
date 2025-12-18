namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos
{
    public class FinalExamTemplateDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int Status { get; set; }
        public List<FinalExamPartialsTemplateDto> PartialTemplates { get; set; } = new List<FinalExamPartialsTemplateDto>();
    }

    public class FinalExamPartialsTemplateDto
    {
        public int Id { get; set; }
        public int FinalExamTemplateId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty; // Helper for frontend
        public decimal Weight { get; set; }
    }
}