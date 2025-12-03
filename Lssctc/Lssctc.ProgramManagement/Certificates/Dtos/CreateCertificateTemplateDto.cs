namespace Lssctc.ProgramManagement.Certificates.Dtos
{
    public class CreateCertificateTemplateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // This contains the HTML string with placeholders {{TraineeName}}
        public string TemplateHtml { get; set; }
    }
}
