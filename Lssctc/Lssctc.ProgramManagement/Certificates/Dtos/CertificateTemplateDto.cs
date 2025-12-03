namespace Lssctc.ProgramManagement.Certificates.Dtos
{
    public class CertificateTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateHtml { get; set; }
        public bool IsActive { get; set; }
    }
}
